using System;
using System.Threading.Tasks;
using coverFlow;
using coverFlow.Services;

// 主程序入口
class Program
{
    static async Task Main(string[] args)
    {
        // 1. 创建 AppLogic 实例
        // 如果你的 API 服务器不在 http://localhost:3000，请修改这里的 URL
        AppLogic myApiHandler = new AppLogic("http://localhost:3000");

        try
        {
            // 2. 执行二维码登录
            await myApiHandler.PerformQrLogin();

            // 3. 登录成功后，获取用户歌单 (你需要知道用户的 UID)
            //    通常，登录成功后，可以调用一个获取当前登录用户信息的接口来得到 UID
            //    这里我们假设一个 UID 用于测试，例如：32953014 (这是示例UID，你需要替换成实际的)
            //    注意：你需要确保 PerformQrLogin 成功并且 _userCookies 被设置
            Console.WriteLine("\n请输入要查询歌单的用户的 UID (例如: 32953014):");
            string? uidInput = Console.ReadLine();
            if (long.TryParse(uidInput, out long userId))
            {
                await myApiHandler.FetchUserPlaylists(userId);
            }
            else
            {
                Console.WriteLine("输入的 UID 无效。");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"发生了未处理的异常: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
        finally
        {
            myApiHandler.DisposeClient(); // 释放资源
        }

        Console.WriteLine("\n按任意键退出...");
        Console.ReadKey();
    }
}

// 这是之前示例中的 AppLogic 类，你可以直接复制过来
public class AppLogic
{
    private readonly AuthService _authService;
    private readonly PlaylistService _playlistService;
    private string? _userCookies; // 保存登录后的 cookies
    private readonly ApiClient _apiClient; // 添加 ApiClient 成员

    public AppLogic(string apiBaseUrl = "http://localhost:3000")
    {
        _apiClient = new ApiClient(apiBaseUrl); // 初始化 ApiClient
        _authService = new AuthService(_apiClient);
        _playlistService = new PlaylistService(_apiClient);
    }

    public async Task PerformQrLogin()
    {
        Console.WriteLine("开始二维码登录流程...");
        // 1. 获取 Key
        var keyResponse = await _authService.GenerateQrKeyAsync();
        if (keyResponse?.Data?.UniKey == null)
        {
            Console.WriteLine("获取二维码 Key 失败。");
            return;
        }

        string key = keyResponse.Data.UniKey;
        Console.WriteLine($"成功获取 Key: {key}");

        // 2. 生成二维码 (例如获取 URL，让用户扫描)
        var qrCreateResponse = await _authService.CreateQrCodeAsync(key, qrimg: false); // qrimg: true 可以获取base64图片
        if (qrCreateResponse?.Data?.QrUrl == null)
        {
            Console.WriteLine("生成二维码信息失败。");
            return;
        }

        Console.WriteLine($"请扫描此二维码链接指向的内容: {qrCreateResponse.Data.QrUrl}");
        if (!string.IsNullOrEmpty(qrCreateResponse.Data.QrImg))
        {
            Console.WriteLine("二维码 Base64 图片已生成 (如果需要，可以将其保存或显示)。");
            // 例如：System.IO.File.WriteAllBytes("qrcode.png", Convert.FromBase64String(qrCreateResponse.Data.QrImg.Split(',')[1]));
        }


        // 3. 轮询检查状态
        Console.WriteLine("请扫描二维码并在手机上确认登录...");
        while (true)
        {
            await Task.Delay(3000); // 每3秒检查一次
            var checkResponse = await _authService.CheckQrStatusAsync(key);
            if (checkResponse == null)
            {
                Console.WriteLine("检测状态失败，可能网络问题或API服务器未响应。");
                continue;
            }

            Console.WriteLine($"当前扫码状态: Code={checkResponse.Code}, Message='{checkResponse.Message}'");
            if (checkResponse.Code == 803) // 登录成功
            {
                _userCookies = checkResponse.Cookie;
                Console.WriteLine("登录成功!");
                Console.WriteLine(
                    $"获取到的 Cookies (部分): {_userCookies?.Substring(0, Math.Min(30, _userCookies.Length))}...");
                break;
            }

            if (checkResponse.Code == 800)
            {
                Console.WriteLine("二维码已过期，请重新开始登录流程。");
                break;
            }

            if (checkResponse.Code == 801)
            {
                // 等待扫码，可以不输出或者只输出一次
            }

            if (checkResponse.Code == 802)
            {
                Console.WriteLine($"已扫码，待确认。用户: {checkResponse.Nickname}, 头像: {checkResponse.AvatarUrl}");
            }

            // 根据API文档，如果扫码后返回502,则需加上noCookie参数
            if (checkResponse.Code == 502)
            {
                Console.WriteLine("检测返回502，尝试使用 noCookie=true 参数...");
                checkResponse = await _authService.CheckQrStatusAsync(key, noCookie: true);
                if (checkResponse != null && checkResponse.Code == 803)
                {
                    _userCookies = checkResponse.Cookie;
                    Console.WriteLine("登录成功 (noCookie)!");
                    break;
                }
                else
                {
                    Console.WriteLine($"使用noCookie后状态: Code={checkResponse?.Code}, Message='{checkResponse?.Message}'");
                    if (checkResponse?.Code == 800)
                    {
                        // 再次检查是否过期
                        Console.WriteLine("二维码已过期 (noCookie 尝试后)。");
                        break;
                    }
                }
            }
        }
    }

    public async Task FetchUserPlaylists(long userId)
    {
        if (string.IsNullOrEmpty(_userCookies))
        {
            Console.WriteLine("错误：用户未登录或 Cookies 为空，无法获取用户歌单。请先登录。");
            return;
        }

        Console.WriteLine($"\n正在获取用户 UID: {userId} 的歌单...");
        var playlistsResponse =
            await _playlistService.GetUserPlaylistsAsync(userId, limit: 5, cookies: _userCookies); // 获取前5个歌单
        if (playlistsResponse?.Playlist != null && playlistsResponse.Playlist.Any())
        {
            Console.WriteLine($"成功获取到 {playlistsResponse.Playlist.Count} 个歌单:");
            foreach (var item in playlistsResponse.Playlist)
            {
                Console.WriteLine(
                    $"- 歌单名: {item.Name} (ID: {item.Id}), 创建者: {item.Creator?.Nickname}, 歌曲数: {item.TrackCount}");
                // 获取此歌单的歌曲详情
                await FetchPlaylistTracks(item.Id);
            }
        }
        else
        {
            Console.WriteLine(
                $"未能获取到用户 {userId} 的歌单，或歌单列表为空。响应代码: {playlistsResponse?.Code}, 消息: {playlistsResponse?.Message}");
        }
    }

    public async Task FetchPlaylistTracks(long playlistId)
    {
        Console.WriteLine($"  正在获取歌单 ID: {playlistId} 的歌曲详情...");
        // 获取歌单详情通常不需要 cookies，除非是私密歌单或特定情况
        var detailResponse = await _playlistService.GetPlaylistDetailAsync(playlistId, _userCookies);
        if (detailResponse?.Playlist?.Tracks != null && detailResponse.Playlist.Tracks.Any())
        {
            Console.WriteLine($"  --- 歌单 '{detailResponse.Playlist.Name}' 中的歌曲 (前5首) ---");
            foreach (var track in detailResponse.Playlist.Tracks.Take(5)) // 只显示前5首歌作为示例
            {
                string artists = string.Join(", ", track.Artists?.Select(a => a.Name) ?? Enumerable.Empty<string>());
                Console.WriteLine($"    > 歌曲: {track.Name}");
                Console.WriteLine($"      歌手: {artists}");
                Console.WriteLine($"      专辑: {track.Album?.Name}");
                Console.WriteLine($"      专辑图: {track.Album?.PicUrl}");
            }

            if (detailResponse.Playlist.Tracks.Count > 5)
            {
                Console.WriteLine("      ...");
            }
        }
        else
        {
            Console.WriteLine(
                $"  未能获取歌单 ID: {playlistId} 的歌曲详情，或歌单中没有歌曲。响应代码: {detailResponse?.Code}, 消息: {detailResponse?.Message}");
        }
    }

    // 别忘了释放 ApiClient
    public void DisposeClient()
    {
        _apiClient?.Dispose();
    }
}