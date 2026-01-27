using System.Net;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace Common.Contracts;

public class ApiResponse<T> : IActionResult
{
    public bool Succeeded { get; set; }
    public T? Data { get; set; }
    public List<ErrorDetail> Errors { get; set; } = [];
    public List<string> Warnings { get; set; } = [];

    [JsonIgnore]
    public HttpStatusCode StatusCode { get; set; }

    public async Task ExecuteResultAsync(ActionContext context)
    {
        var objectResult = new ObjectResult(this)
        {
            StatusCode = (int)StatusCode
        };

        await objectResult.ExecuteResultAsync(context);
    }

    public static ApiResponse<T> Success(T data, HttpStatusCode statusCode = HttpStatusCode.OK) => new()
    {
        Succeeded = true,
        Data = data,
        StatusCode = statusCode
    };

    public static ApiResponse<T> Failure(List<ErrorDetail> errors, HttpStatusCode statusCode = HttpStatusCode.BadRequest) => new()
    {
        Succeeded = false,
        Errors = errors,
        StatusCode = statusCode
    };

    public static ApiResponse<T> Failure(string code, string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest) =>
        Failure([ErrorDetail.Create(code, message)], statusCode);

    public ApiResponse<T> AddWarning(string warning)
    {
        Warnings.Add(warning);
        return this;
    }

    public ApiResponse<T> AddWarnings(IEnumerable<string> warnings)
    {
        Warnings.AddRange(warnings);
        return this;
    }

    public ApiResponse<T> AddError(ErrorDetail error)
    {
        Errors.Add(error);
        return this;
    }

    public ApiResponse<T> AddError(string code, string message)
    {
        Errors.Add(ErrorDetail.Create(code, message));
        return this;
    }

    public ApiResponse<T> WithStatusCode(HttpStatusCode statusCode)
    {
        StatusCode = statusCode;
        return this;
    }
}

public static class ApiResponse
{
    public static ApiResponse<T> Success<T>(T data, HttpStatusCode statusCode = HttpStatusCode.OK) =>
        ApiResponse<T>.Success(data, statusCode);

    public static ApiResponse<T> Created<T>(T data) =>
        ApiResponse<T>.Success(data, HttpStatusCode.Created);

    public static ApiResponse<T> Failure<T>(List<ErrorDetail> errors, HttpStatusCode statusCode = HttpStatusCode.BadRequest) =>
        ApiResponse<T>.Failure(errors, statusCode);

    public static ApiResponse<T> Failure<T>(string code, string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest) =>
        ApiResponse<T>.Failure(code, message, statusCode);

    public static ApiResponse<T> NotFound<T>(string message = "Resource not found") =>
        ApiResponse<T>.Failure("NOT_FOUND", message, HttpStatusCode.NotFound);

    public static ApiResponse<T> Unauthorized<T>(string message = "Unauthorized") =>
        ApiResponse<T>.Failure("UNAUTHORIZED", message, HttpStatusCode.Unauthorized);

    public static ApiResponse<T> Forbidden<T>(string message = "Forbidden") =>
        ApiResponse<T>.Failure("FORBIDDEN", message, HttpStatusCode.Forbidden);
}