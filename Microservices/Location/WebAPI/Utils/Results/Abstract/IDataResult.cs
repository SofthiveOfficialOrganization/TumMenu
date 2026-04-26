using System.Text.Json.Serialization;

namespace WebAPI.Utils.Results.Abstract;

public interface IDataResult<T> : IResult
{
    [JsonPropertyName("data")]
    T Data { get; }
}
