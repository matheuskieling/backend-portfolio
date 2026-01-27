namespace Portfolio.Api.Common;

public sealed class ErrorDetail
{
    public required string Code { get; init; }
    public required string Message { get; init; }

    public static ErrorDetail Create(string code, string message) => new()
    {
        Code = code,
        Message = message
    };
}
