namespace Shared.Models;

public record ErrorResponse(
    string Message,
    int StatusCode,
    Dictionary<string, string[]>? Errors = null
);