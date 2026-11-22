using Microsoft.AspNetCore.Identity;

namespace AutenticacionSegura.Negocio;

// Traduce los mensajes de error de Identity al español
public class DescriptorErroresEs : IdentityErrorDescriber
{
    public override IdentityError DefaultError() =>
        new() { Code = nameof(DefaultError), Description = "Ocurrió un error. Intente de nuevo." };

    public override IdentityError PasswordTooShort(int length) =>
        new() { Code = nameof(PasswordTooShort), Description = $"La contraseña debe tener al menos {length} caracteres." };

    public override IdentityError PasswordRequiresNonAlphanumeric() =>
        new() { Code = nameof(PasswordRequiresNonAlphanumeric), Description = "La contraseña debe tener al menos un símbolo (por ejemplo # o $)." };

    public override IdentityError PasswordRequiresDigit() =>
        new() { Code = nameof(PasswordRequiresDigit), Description = "La contraseña debe tener al menos un número." };

    public override IdentityError PasswordRequiresLower() =>
        new() { Code = nameof(PasswordRequiresLower), Description = "La contraseña debe tener al menos una letra minúscula." };

    public override IdentityError PasswordRequiresUpper() =>
        new() { Code = nameof(PasswordRequiresUpper), Description = "La contraseña debe tener al menos una letra mayúscula." };

    public override IdentityError PasswordMismatch() =>
        new() { Code = nameof(PasswordMismatch), Description = "La contraseña actual es incorrecta." };

    public override IdentityError DuplicateEmail(string email) =>
        new() { Code = nameof(DuplicateEmail), Description = "Ese correo ya está registrado." };

    public override IdentityError DuplicateUserName(string userName) =>
        new() { Code = nameof(DuplicateUserName), Description = "Ese correo ya está registrado." };

    public override IdentityError InvalidEmail(string? email) =>
        new() { Code = nameof(InvalidEmail), Description = "El correo no tiene un formato válido." };

    public override IdentityError InvalidUserName(string? userName) =>
        new() { Code = nameof(InvalidUserName), Description = "El correo no tiene un formato válido." };

    public override IdentityError InvalidToken() =>
        new() { Code = nameof(InvalidToken), Description = "El enlace no es válido, ya fue usado o ha caducado." };
}