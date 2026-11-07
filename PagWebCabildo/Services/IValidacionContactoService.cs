namespace PagWebCabildo.Services;

/// <summary>
/// Servicio transversal para validar el medio de contacto proporcionado por un
/// visitante en el formulario de contacto. El medio se considera válido cuando
/// es un correo electrónico (<c>nombre@dominio.extensión</c>) o un número de
/// teléfono de 7 a 15 dígitos (Requisitos 16.2 y 16.5).
/// </summary>
public interface IValidacionContactoService
{
    /// <summary>
    /// Determina si el medio de contacto indicado es válido.
    /// </summary>
    /// <param name="medio">
    /// Cadena ingresada por el visitante. Se recorta el espacio en blanco al
    /// inicio y al final antes de validar; una cadena nula, vacía o compuesta
    /// solo por espacios se considera inválida.
    /// </param>
    /// <returns>
    /// <c>true</c> si <paramref name="medio"/> cumple el formato de correo
    /// electrónico (<c>nombre@dominio.extensión</c>) o el de teléfono (de 7 a 15
    /// dígitos); <c>false</c> en cualquier otro caso.
    /// </returns>
    bool EsMedioContactoValido(string medio);
}
