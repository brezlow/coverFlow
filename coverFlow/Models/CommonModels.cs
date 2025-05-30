using System.Text.Json.Serialization;

namespace coverFlow.Models;

public class CommonModels
{
    public class BaseResponse
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; } // 有些接口可能没有 message
    }
}