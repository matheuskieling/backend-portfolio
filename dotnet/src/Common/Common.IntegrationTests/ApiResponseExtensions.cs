using System.Net;
using System.Net.Http.Json;
using Common.Contracts;
using Xunit;

namespace Common.IntegrationTests;

public static class ApiResponseExtensions
{
    public static async Task<ApiResponse<T>> ValidateSuccessAsync<T>(
        this HttpResponseMessage response,
        HttpStatusCode expectedStatus)
    {
        Assert.Equal(expectedStatus, response.StatusCode);

        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<T>>();
        Assert.NotNull(apiResponse);
        Assert.True(apiResponse.Succeeded, "Expected successful response but got failure");
        Assert.NotNull(apiResponse.Data);

        return apiResponse;
    }

    public static async Task<ApiResponse<object>> ValidateFailureAsync(
        this HttpResponseMessage response,
        HttpStatusCode expectedStatus,
        string? expectedErrorMessage = null,
        string? expectedWarningMessage = null)
    {
        Assert.Equal(expectedStatus, response.StatusCode);

        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        Assert.NotNull(apiResponse);
        Assert.False(apiResponse.Succeeded, "Expected failure response but got success");

        if (expectedErrorMessage is not null)
        {
            Assert.True(
                apiResponse.Errors.Any(e => e.Message.Contains(expectedErrorMessage, StringComparison.OrdinalIgnoreCase)),
                $"Expected error containing '{expectedErrorMessage}' but got: [{string.Join(", ", apiResponse.Errors.Select(e => e.Message))}]");
        }

        if (expectedWarningMessage is not null)
        {
            Assert.True(
                apiResponse.Warnings.Any(w => w.Contains(expectedWarningMessage, StringComparison.OrdinalIgnoreCase)),
                $"Expected warning containing '{expectedWarningMessage}' but got: [{string.Join(", ", apiResponse.Warnings)}]");
        }

        return apiResponse;
    }
}