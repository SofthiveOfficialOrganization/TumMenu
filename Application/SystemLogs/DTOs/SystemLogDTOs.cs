namespace Application.SystemLogs.DTOs;

public sealed record SystemLogListDTO(
    Guid Id,
    DateTimeOffset CreatedAt,
    string Level,
    string Source,
    int StatusCode,
    string? UserName,
    string? UserId,
    string? Path,
    string? ResponseMessage,
    string? ExceptionMessage,
    string? TraceId
);

public sealed record SystemLogDetailDTO(
    Guid Id,
    DateTimeOffset CreatedAt,
    string Level,
    string Source,
    int StatusCode,
    string? ErrorCode,
    string? ResponseMessage,
    string? ExceptionType,
    string? ExceptionMessage,
    string? StackTrace,
    string? TraceId,
    string? HttpMethod,
    string? Path,
    string? QueryString,
    string? UserId,
    string? UserName,
    string? RemoteIp,
    string? UserAgent
);
