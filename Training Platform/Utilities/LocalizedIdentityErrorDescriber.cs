using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;

namespace Training_Platform.Utilities
{
    public class LocalizedIdentityErrorDescriber : IdentityErrorDescriber
    {
        private readonly IStringLocalizer<SharedResource> _localizer;

        public LocalizedIdentityErrorDescriber(IStringLocalizer<SharedResource> localizer)
        {
            _localizer = localizer;
        }

        private IdentityError ErrorWithCode(string code, params object[] args) =>
            new() { Code = code, Description = _localizer[code, args] };

        public override IdentityError DefaultError() => ErrorWithCode("DefaultError");
        public override IdentityError ConcurrencyFailure() => ErrorWithCode("ConcurrencyFailure");
        public override IdentityError PasswordMismatch() => ErrorWithCode("PasswordMismatch");
        public override IdentityError InvalidToken() => ErrorWithCode("InvalidToken");
        public override IdentityError RecoveryCodeRedemptionFailed() => ErrorWithCode("RecoveryCodeRedemptionFailed");
        public override IdentityError LoginAlreadyAssociated() => ErrorWithCode("LoginAlreadyAssociated");
        public override IdentityError InvalidUserName(string? userName) => ErrorWithCode("InvalidUserName", userName ?? string.Empty);
        public override IdentityError InvalidEmail(string? email) => ErrorWithCode("InvalidEmail", email ?? string.Empty);
        public override IdentityError DuplicateUserName(string? userName) => ErrorWithCode("DuplicateUserName", userName ?? string.Empty);
        public override IdentityError DuplicateEmail(string? email) => ErrorWithCode("DuplicateEmail", email ?? string.Empty);
        public override IdentityError InvalidRoleName(string? role) => ErrorWithCode("InvalidRoleName", role ?? string.Empty);
        public override IdentityError DuplicateRoleName(string? role) => ErrorWithCode("DuplicateRoleName", role ?? string.Empty);
        public override IdentityError UserAlreadyHasPassword() => ErrorWithCode("UserAlreadyHasPassword");
        public override IdentityError UserLockoutNotEnabled() => ErrorWithCode("UserLockoutNotEnabled");
        public override IdentityError UserAlreadyInRole(string? role) => ErrorWithCode("UserAlreadyInRole", role ?? string.Empty);
        public override IdentityError UserNotInRole(string? role) => ErrorWithCode("UserNotInRole", role ?? string.Empty);
        public override IdentityError PasswordTooShort(int length) => ErrorWithCode("PasswordTooShort", length);
        public override IdentityError PasswordRequiresUniqueChars(int uniqueCount) => ErrorWithCode("PasswordRequiresUniqueChars", uniqueCount);
        public override IdentityError PasswordRequiresNonAlphanumeric() => ErrorWithCode("PasswordRequiresNonAlphanumeric");
        public override IdentityError PasswordRequiresDigit() => ErrorWithCode("PasswordRequiresDigit");
        public override IdentityError PasswordRequiresLower() => ErrorWithCode("PasswordRequiresLower");
        public override IdentityError PasswordRequiresUpper() => ErrorWithCode("PasswordRequiresUpper");
    }
}