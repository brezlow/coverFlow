using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using coverFlow.Models;
using NeteaseCloudMusicApi;

namespace coverFlow.Services;

public class AuthService
{
    CloudMusicApi _cloudApi = new CloudMusicApi();

    // 获取当前Unix时间戳 (毫秒)
    private long GetCurrentTimestampMillis() => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    /// <summary>
    /// 1. 生成二维码 Key
    /// </summary>
    public async Task<AuthModels.QrKeyResponse?> GenerateQrKeyAsync()
    {
        var queries = new Dictionary<string, object>
        {
            { "timestamp", GetCurrentTimestampMillis() }
        };

        // 重要: LoginQrKeyGet 是假设的 Provider 名称，请替换为实际库中的正确名称
        var (isOk, json) = await _cloudApi.RequestAsync(CloudMusicApiProviders.LoginQrKeyGet, queries);

        if (isOk)
        {
            // 假设json结构与你的DTO兼容，或者库直接返回了更具体的对象
            // Node.js API 的 /login/qr/key 返回的 unikey 在 data.unikey
            // 但外层也有 code。我们需要适配。
            // 如果库的 JObject 直接是 { "data": { "unikey": "xxx", "code": 200 }, "code": 200 } 这样的结构
            // 那么 ToObject<QrKeyResponse> 是合适的。
            // 如果 JObject 只是 Node.js API 响应体的内容，那可能需要调整。
            // 为了与之前的DTO兼容，我们假设 JObject 能被正确反序列化。
            return json.ToObject<AuthModels.QrKeyResponse>();
        }

        Console.WriteLine($"GenerateQrKeyAsync 失败: {json}");
        return null;
    }

    /// <summary>
    /// 2. 根据 Key 生成二维码
    /// </summary>
    public async Task<AuthModels.QrCreateResponse?> CreateQrCodeAsync(string key, bool qrimg = false)
    {
        var queries = new Dictionary<string, object>
        {
            { "key", key },
            { "timestamp", GetCurrentTimestampMillis() }
        };
        if (qrimg)
        {
            queries["qrimg"] = "true"; // API 通常接受字符串 "true" 或布尔值
        }

        // 重要: LoginQrCreate 是假设的 Provider 名称
        var (isOk, json) = await _cloudApi.RequestAsync(CloudMusicApiProviders.LoginQrCreate, queries);

        if (isOk)
        {
            return json.ToObject<AuthModels.QrCreateResponse>();
        }

        Console.WriteLine($"CreateQrCodeAsync 失败: {json}");
        return null;
    }

    /// <summary>
    /// 3. 检测二维码扫码状态
    /// </summary>
    public async Task<AuthModels.QrCheckResponse?> CheckQrStatusAsync(string key, bool noCookie = false)
    {
        var queries = new Dictionary<string, object>
        {
            { "key", key },
            { "timestamp", GetCurrentTimestampMillis() }
        };
        if (noCookie)
        {
            queries["noCookie"] = "true";
        }

        // 重要: LoginQrCheckStatus 是假设的 Provider 名称
        var (isOk, json) = await _cloudApi.RequestAsync(CloudMusicApiProviders.LoginQrCheckStatus, queries);

        if (isOk)
        {
            // 这个接口的响应比较特殊，code 和 message 是顶层的状态
            // cookies 也是顶层的。我们的 QrCheckResponse DTO 就是这样设计的。
            return json.ToObject<AuthModels.QrCheckResponse>();
        }

        // 即使 isOk 为 false，json 可能也包含有用的错误信息 (如 code: 800 二维码过期)
        // 这里的逻辑可能需要根据实际库返回的 isOk 和 json 的具体含义调整
        // 例如，某些 code (如 800, 801, 802) 可能 isOk 也是 true，因为请求本身是成功的
        // 但如果 isOk 为 false 通常表示网络或服务器级别的错误
        if (json != null)
        {
            // 尝试反序列化，即使 isOk 为 false，因为某些业务“失败”也是有效响应
            try
            {
                return json.ToObject<AuthModels.QrCheckResponse>();
            }
            catch
            {
                /* ignore */
            }
        }

        Console.WriteLine($"CheckQrStatusAsync 请求本身可能失败或未返回有效 JSON: isOk={isOk}, json={json}");
        return null;
    }
}