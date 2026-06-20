using BG.Invoice.Application.Dtos;
using BG.Invoice.Application.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace BG.Invoice.UnitTests.Application.Validators;

public class ChangePasswordRequestValidatorTests
{
    private readonly ChangePasswordRequestValidator _validator = new();

    [Fact]
    public void ValidRequest_PassesValidation()
    {
        var request = new ChangePasswordRequest("oldPass123", "newPass123", "newPass123");
        var result = _validator.TestValidate(request);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void EmptyNewPassword_FailsValidation()
    {
        var request = new ChangePasswordRequest("oldPass123", "", "newPass123");
        var result = _validator.TestValidate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "NewPassword");
    }

    [Fact]
    public void ShortNewPassword_FailsValidation()
    {
        var request = new ChangePasswordRequest("oldPass123", "12345", "12345");
        var result = _validator.TestValidate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "NewPassword");
    }

    [Fact]
    public void PasswordsDoNotMatch_FailsValidation()
    {
        var request = new ChangePasswordRequest("oldPass123", "newPass123", "differentPass");
        var result = _validator.TestValidate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ConfirmPassword");
    }
}
