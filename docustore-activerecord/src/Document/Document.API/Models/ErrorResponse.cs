namespace Document.API.Models;

public record ErrorResponse(
    string Message,
    int StatusCode,
    Dictionary<string, string[]>? Errors = null
);