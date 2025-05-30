using System.Text.Json.Serialization;

namespace coverFlow.Models;

public class AuthModels
{
    // 1. 二维码 key 生成接口 (/login/qr/key)
    public class QrKeyData
    {
        [JsonPropertyName("code")]
        public int Code { get; set; } // 这个是业务code，通常是200

        [JsonPropertyName("unikey")]
        public string UniKey { get; set; } = string.Empty;
    }

    public class QrKeyResponse : CommonModels.BaseResponse // 继承自通用响应，包含外层 code
    {
        [JsonPropertyName("data")]
        public QrKeyData Data { get; set; } = new QrKeyData();
    }


    // 2. 二维码生成接口 (/login/qr/create)
    public class QrCreateData
    {
        [JsonPropertyName("qrurl")]
        public string QrUrl { get; set; } = string.Empty; // 二维码信息内容

        [JsonPropertyName("qrimg")]
        public string? QrImg { get; set; } // base64 图片, 可选
    }
    public class QrCreateResponse : CommonModels.BaseResponse
    {
        [JsonPropertyName("data")]
        public QrCreateData Data { get; set; } = new QrCreateData();
    }


    // 3. 二维码检测扫码状态接口 (/login/qr/check)
    public class QrCheckResponse : CommonModels.BaseResponse // code 在这里是核心状态码
    {
        // code: 800 为二维码过期, 801 为等待扫码, 802 为待确认, 803 为授权登录成功
        [JsonPropertyName("cookie")]
        public string? Cookie { get; set; } // 803 时返回

        // message 通常也包含状态信息
        [JsonPropertyName("avatarUrl")]
        public string? AvatarUrl {get; set; } // 802 时可能返回头像

        [JsonPropertyName("nickname")]
        public string? Nickname {get; set; } // 802 时可能返回昵称
    }
}