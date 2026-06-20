using BG.Invoice.Application.Abstractions;
using BG.Invoice.Application.Common;
using BG.Invoice.Application.Dtos;
using BG.Invoice.Application.Validators;
using BG.Invoice.Domain.Entities;
using BG.Invoice.Domain.Exceptions;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace BG.Invoice.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        IUnitOfWork unitOfWork,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
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

        var user = await _userRepository.GetByUserNameWithRoleAsync(request.UserName, ct);
        if (user is null)
            throw new UnauthorizedException(Errors.Auth.InvalidCredentials);

        if (!user.IsActive)
            throw new ForbiddenException(Errors.Auth.AccountInactive);

        if (user.IsLockedOut(DateTime.UtcNow))
            throw new AccountLockedException(Errors.Auth.AccountLocked);

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            user.RecordFailedLogin(5, TimeSpan.FromMinutes(15));
            await _unitOfWork.SaveChangesAsync(ct);
            throw new UnauthorizedException(Errors.Auth.InvalidCredentials);
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
            throw new NotFoundException("User", userId);

        if (!_passwordHasher.Verify(request.CurrentPassword, user.PasswordHash))
            return Result.Failure(Errors.Auth.CurrentPasswordIncorrect);

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
            throw new BusinessRuleException(Errors.User.UsernameTaken);

        var existingEmail = await _userRepository.ListAsync(u => u.Email == request.Email, ct);
        if (existingEmail.Any())
            throw new BusinessRuleException(Errors.User.EmailRegistered);

        var hash = _passwordHasher.Hash(request.Password);
        var user = User.Create(request.UserName, request.Email, hash, request.FirstName, request.LastName, request.RoleId);

        await _userRepository.AddAsync(user, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
