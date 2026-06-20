using BG.Invoice.Application.Abstractions;
using BG.Invoice.Application.Dtos;
using BG.Invoice.Application.Validators;
using BG.Invoice.Domain.Entities;
using BG.Invoice.Domain.Exceptions;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace BG.Invoice.Application.Services;

public class AuthService : IAuthService
{
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<Role> _roleRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IRepository<User> userRepository,
        IRepository<Role> roleRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        IUnitOfWork unitOfWork,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var validator = new LoginRequestValidator();
        var validationResult = await validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
            return Result.ValidationError<LoginResponse>(validationResult.Errors.Select(e => e.ErrorMessage).ToList());

        var users = await _userRepository.ListAsync(u => u.UserName == request.UserName, ct);
        var user = users.FirstOrDefault();
        if (user is null)
            return Result.Failure<LoginResponse>("Invalid credentials.");

        if (!user.IsActive)
            return Result.Failure<LoginResponse>("Account is inactive.");

        if (user.IsLockedOut(DateTime.UtcNow))
            return Result.Failure<LoginResponse>("Account is locked. Try again later.");

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            user.RecordFailedLogin(5, TimeSpan.FromMinutes(15));
            await _unitOfWork.SaveChangesAsync(ct);
            return Result.Failure<LoginResponse>("Invalid credentials.");
        }

        user.ResetFailedLogins();
        await _unitOfWork.SaveChangesAsync(ct);

        var roleName = user.Role?.Name ?? "";
        var token = _jwtTokenService.GenerateToken(user.Id, user.UserName, roleName, user.RoleId);

        return Result.Success(new LoginResponse(token, new UserInfo(
            user.Id, user.UserName, user.Email, user.FirstName, user.LastName,
            roleName, user.RoleId, user.MustChangePassword)));
    }

    public async Task<Result> ChangePasswordAsync(ChangePasswordRequest request, int userId, CancellationToken ct = default)
    {
        var validator = new ChangePasswordRequestValidator();
        var validationResult = await validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
            return Result.ValidationError(validationResult.Errors.Select(e => e.ErrorMessage).ToList());

        var user = await _userRepository.GetByIdAsync(userId, ct);
        if (user is null)
            return Result.Failure("User not found.");

        if (!_passwordHasher.Verify(request.CurrentPassword, user.PasswordHash))
            return Result.Failure("Current password is incorrect.");

        user.ChangePassword(_passwordHasher.Hash(request.NewPassword));
        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var validator = new RegisterUserRequestValidator();
        var validationResult = await validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
            return Result.ValidationError(validationResult.Errors.Select(e => e.ErrorMessage).ToList());

        var existing = await _userRepository.ListAsync(u => u.UserName == request.UserName, ct);
        if (existing.Any())
            return Result.Failure("Username is already taken.");

        var existingEmail = await _userRepository.ListAsync(u => u.Email == request.Email, ct);
        if (existingEmail.Any())
            return Result.Failure("Email is already registered.");

        var hash = _passwordHasher.Hash(request.Password);
        var user = User.Create(request.UserName, request.Email, hash, request.FirstName, request.LastName, request.RoleId);

        await _userRepository.AddAsync(user, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
