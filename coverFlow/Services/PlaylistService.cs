using coverFlow.Models;

namespace coverFlow.Services;

public class PlaylistService
{
    private readonly ApiClient _apiClient;

    public PlaylistService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    /// <summary>
    /// 获取用户歌单列表
    /// </summary>
    /// <param name="uid">用户 ID</param>
    /// <param name="limit">返回数量, 默认为 30</param>
    /// <param name="offset">偏移数量，用于分页, 默认为 0</param>
    /// <param name="cookies">登录后获取的 Cookies</param>
    public async Task<PlaylistModels.UserPlaylistResponse?> GetUserPlaylistsAsync(long uid, int limit = 30,
        int offset = 0, string? cookies = null)
    {
        string endpoint = $"/user/playlist?uid={uid}&limit={limit}&offset={offset}";
        return await _apiClient.GetAsync<PlaylistModels.UserPlaylistResponse>(endpoint, cookies);
    }

    /// <summary>
    /// 获取特定歌单的详细信息，包括所有歌曲
    /// </summary>
    /// <param name="playlistId">歌单 ID</param>
    /// <param name="cookies">登录后获取的 Cookies (某些歌单可能需要登录才能获取完整信息)</param>
    public async Task<PlaylistModels.PlaylistDetailResponse?> GetPlaylistDetailAsync(long playlistId,
        string? cookies = null)
    {
        // 接口地址通常是 /playlist/detail
        string endpoint = $"/playlist/detail?id={playlistId}";
        return await _apiClient.GetAsync<PlaylistModels.PlaylistDetailResponse>(endpoint, cookies);
    }
}