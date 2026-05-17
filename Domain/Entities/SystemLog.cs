using Domain.Base;

namespace Domain.Entities;

public class SystemLog : BaseEntity
{
    public string Level { get; set; } = null!;
    public string Source { get; set; } = null!;
    public int StatusCode { get; set; }
    public string? ErrorCode { get; set; }
    public string? ResponseMessage { get; set; }
    public string? ExceptionType { get; set; }
    public string? ExceptionMessage { get; set; }
    public string? StackTrace { get; set; }
    public string? TraceId { get; set; }
    public string? HttpMethod { get; set; }
    public string? Path { get; set; }
    public string? QueryString { get; set; }
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public string? RemoteIp { get; set; }
    public string? UserAgent { get; set; }
}
