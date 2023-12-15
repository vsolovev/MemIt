using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Refit;

namespace MemIt
{
    public class PostLikes
    {
        public int Count { get; set; }
    }
    public class PostReposts
    {
        public int Count { get; set; }
    }
    public class PostViews
    {
        public int Count { get; set; }
    }
    public class WallResult
    {
        public WallResponse Response { get; set; }
    }
    public class WallResponse
    {
        public int Count { get; set; }
        public List<Post> Items { get; set; }
    }
    public class Attachment
    {
        public string Type { get; set; }
        public VideoAttachmentItem Video { get; set; }
        public PhotoAttachmentItem Photo { get; set; }
    }
    public class PhotoSize
    {
        public string Url { get; set; }
        public int Height { get; set; }
    }
    public class VideoAttachmentItem
    {        
        public string Id { get; set; }
        [JsonProperty("access_key")]
        public string AccessKey { get; set; }
    }
    public class PhotoAttachmentItem
    {
        public string Id { get; set; }
        public List<PhotoSize> Sizes { get; set; }
    }
    public class Post
    {
        public int Date { get; set; }
        public string Text { get; set; }
        public PostLikes Likes { get; set; }
        public PostReposts Reposts { get; set; }
        public PostViews Views { get; set; }
        public List<Attachment> Attachments { get; set; }
    }

    public interface IVkApi
    {       

        [Get("/method/wall.get?count=100&offset={offset}&filter=owner&owner_id=-30666517&v=5.199")]
        public Task<WallResult> GetPosts(int offset = 0);
    }
}
