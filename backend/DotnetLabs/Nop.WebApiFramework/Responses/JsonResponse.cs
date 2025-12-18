using Newtonsoft.Json;

namespace Nop.WebApiFramework;

public class JsonResponse
{
    public JsonResponse(int errCode = 0, string errMessage = "ok", string? requestId = "")
    {
        ErrorCode = errCode;
        ErrorMessage = errMessage;
        RequestId = requestId;
    }

    [JsonIgnore]
    public bool IsError => ErrorCode != 0;

    /// <summary>
    /// 0是无错误
    /// </summary> 
    [JsonProperty("errCode")]
    public int ErrorCode { get; set; }

    [JsonProperty("errMsg")]
    public string ErrorMessage { get; set; } = null!;

    [JsonProperty("requestId")]

    public string? RequestId { get; set; }


    public static JsonResponse ErrorResponse(string errorMsg, string requestId)
    {
        return new JsonResponse(500, errorMsg, requestId);
    }
}

public class JsonResponse<T> : JsonResponse
{
    public JsonResponse(T data, int errCode = 0, string errMessage = "ok", string? requestId = "")
        : base(errCode, errMessage, requestId)
    {
        Data = data;
    }
    [JsonProperty("data")]
    public T Data { get; set; }


    public static JsonResponse<T> ErrorResponse(string errorMsg, string requestId)
    {
        return new JsonResponse<T>(default, 500, errorMsg, requestId);
    }
}

