using Newtonsoft.Json;
using IResult = WebAPI.Utils.Results.Abstract.IResult;

namespace WebAPI.Utils.Results.Abstract;

public interface IDataResult<T> : IResult
{

    [JsonProperty("data")]
    T Data { get; }
}
