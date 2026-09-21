namespace ResourceBooking.Api.Services;

/// <summary>
/// Greška koju servis namjerno baca kad ulazni podaci ili stanje nisu ispravni
/// (npr. zauzet termin). Middleware je pretvara u uredan HTTP odgovor,
/// pa kontroleri ne moraju biti puni try/catch blokova.
/// </summary>
public class AppException : Exception
{
    public int StatusCode { get; }

    public AppException(string message, int statusCode = StatusCodes.Status400BadRequest)
        : base(message)
    {
        StatusCode = statusCode;
    }

    public static AppException NotFound(string message) =>
        new(message, StatusCodes.Status404NotFound);

    public static AppException Forbidden(string message) =>
        new(message, StatusCodes.Status403Forbidden);

    public static AppException Unauthorized(string message) =>
        new(message, StatusCodes.Status401Unauthorized);
}
