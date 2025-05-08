using System.Net;

namespace EventApp.Models.SharedDTO;

public record ErrorResponse {
    public int Status { get; init; }
    public string Title { get; init; }

    public string? Detail { get; init; }

    public ErrorResponse(HttpStatusCode statusCode, string title, string? detail = null) {
        Status = (int)statusCode;
        Title = title;
        Detail = detail;
    }

    public ErrorResponse(int statusCode, string title, string? detail = null) {
        Status = statusCode;
        Title = title;
        Detail = detail;
    }

}

