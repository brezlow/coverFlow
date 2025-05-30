using System.Text.Json.Serialization;

namespace coverFlow.Models;

public class PlaylistModels
{
      public class Creator
    {
        [JsonPropertyName("userId")]
        public long UserId { get; set; }

        [JsonPropertyName("nickname")]
        public string Nickname { get; set; } = string.Empty;
    }

    public class TrackArtist
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
    }

    public class TrackAlbum
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("picUrl")]
        public string? PicUrl { get; set; }
    }

    public class Track
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("ar")]
        public List<TrackArtist>? Artists { get; set; } // ar 字段代表 artists

        [JsonPropertyName("al")]
        public TrackAlbum? Album { get; set; } // al 字段代表 album

        // 你可以根据需要添加更多歌曲信息字段
        // 例如：时长 (dt), publishTime 等
    }

    // 用户歌单列表中的单个歌单项 (/user/playlist)
    public class UserPlaylistItem
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("coverImgUrl")]
        public string? CoverImgUrl { get; set; }

        [JsonPropertyName("creator")]
        public Creator? Creator { get; set; }

        [JsonPropertyName("trackCount")]
        public int TrackCount { get; set; }

        // 注意：/user/playlist 返回的歌单列表通常不包含完整的歌曲列表，
        // 如果需要歌单内所有歌曲，需要调用 /playlist/detail
        // 有些版本的API可能会返回部分预览曲目，这里先不加 tracks
    }

    public class UserPlaylistResponse : CommonModels.BaseResponse
    {
        [JsonPropertyName("playlist")]
        public List<UserPlaylistItem>? Playlist { get; set; }

        [JsonPropertyName("more")]
        public bool More { get; set; } // 是否还有更多歌单
    }


    // 获取歌单详情 (/playlist/detail) - 用于获取歌单内所有歌曲
    public class PlaylistDetail
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("coverImgUrl")]
        public string? CoverImgUrl { get; set; }

        [JsonPropertyName("creator")]
        public Creator? Creator { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("trackCount")]
        public int TrackCount { get; set; }

        [JsonPropertyName("playCount")]
        public long PlayCount { get; set; }

        [JsonPropertyName("tracks")] // 这里是歌单内的完整歌曲列表
        public List<Track>? Tracks { get; set; }

        // 可以根据需要添加更多如 subscribedCount, shareCount, commentCount 等字段
    }

    public class PlaylistDetailResponse : CommonModels.BaseResponse
    {
        [JsonPropertyName("playlist")]
        public PlaylistDetail? Playlist { get; set; }
        // 可能还会有 privileges 等字段，根据API实际返回添加
    }
}