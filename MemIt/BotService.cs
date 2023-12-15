using Microsoft.Extensions.Hosting;
using System.IO;
using System.Net.Mail;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using YoutubeDLSharp;

namespace MemIt
{

   
    public class BotService
    {
        private readonly IVkApi vkApi;
        private readonly MemContext memContext;
        private readonly YoutubeDL ytdl = new YoutubeDL();
        private readonly ITelegramBotClient botClient;
        public BotService(IVkApi vkApi, MemContext memContext, ITelegramBotClient botClient)
        {
            this.vkApi = vkApi;
            YoutubeDLSharp.Utils.DownloadYtDlp();
            YoutubeDLSharp.Utils.DownloadFFmpeg();
            this.memContext = memContext;
            this.botClient = botClient;
        }
       
        public async Task TransferNewPosts(long? exactChatId = null)
        {
            var response = await vkApi.GetPosts();
            var anotherResponse = await vkApi.GetPosts(100);
            response.Response.Items.AddRange(anotherResponse.Response.Items);
            var best = await ProcessBestPosts(response);
            await PostMessages(best, exactChatId);
        }

        private async Task SendMessage(string text, long? exactChatId, List<IAlbumInputMedia> medias = null)
        {
            if (exactChatId != null)
            {
                if (medias != null)
                {
                    await botClient.SendMediaGroupAsync(exactChatId, medias);
                }
                else
                {
                    await botClient.SendTextMessageAsync(exactChatId, text);
                }

                return;
            }

            foreach(var chatId in memContext.ChatIds)
            {
                if (medias != null)
                {
                    await botClient.SendMediaGroupAsync(chatId, medias);
                }
                else
                {
                    await botClient.SendTextMessageAsync(chatId, text);
                }
            }
        }

        private async Task PostMessages(List<Post> posts, long? exactChatId = null)
        {
            if (posts.Count== 0)
            {
                await SendMessage("На сегодня топчик отсутствует", exactChatId);
            }
            foreach(var post in posts)
            {
                
                if (post.Attachments.Count > 0)
                {
                    var streams = new List<FileStream>();
                    var toDelete = new List<string>();
                    //There are attachments. We will process only photos and videos
                    var medias = new List<IAlbumInputMedia>();
                    foreach (var attachment in post.Attachments)
                    {
                        if (attachment.Type == "photo")
                        {
                            var media = new InputMediaPhoto(new InputFileUrl(attachment.Photo.Sizes.OrderByDescending(x=>x.Height).First().Url));
                            if (medias.Count == 0)
                            {
                                media.Caption = post.Text;
                            }
                            medias.Add(media);
                        }
                        else if (attachment.Type == "video")
                        {
                            var res = await ytdl.RunVideoDownload($"https://vk.com/video-30666517_{attachment.Video.Id}_{attachment.Video.AccessKey}");
                            var oldName = Path.GetFileNameWithoutExtension(res.Data);
                            var newPath = Path.Combine(Path.GetDirectoryName(res.Data), $"{Guid.NewGuid().ToString()}{Path.GetExtension(res.Data)}");
                            System.IO.File.Move(res.Data, newPath);
                            var s = new FileStream(newPath, FileMode.Open);
                            streams.Add(s);
                            var media = new InputMediaVideo(new InputFileStream(s, fileName: oldName));
                            toDelete.Add(newPath);
                            if (medias.Count == 0)
                            {
                                media.Caption = post.Text;
                            }
                            medias.Add(media);
                        }
                    }
                    if (medias.Count > 0)
                    {
                        await SendMessage(null, exactChatId, medias);
                        Cleanup(streams, toDelete);
                        return;
                    }                
                }

                await SendMessage(post.Text, exactChatId); 
            }
        }

        private void Cleanup(List<FileStream> streams, List<string> files)
        {
            foreach (var s in streams)
            {
                s.Flush();
                s.Dispose();
            }
            foreach (var td in files)
            {
                System.IO.File.Delete(td);
            }
        }

        private async Task<List<Post>> ProcessBestPosts(WallResult result)
        {
            var lastEpoch = memContext.LastEpoch;            
            //Get only today's posts
            var todays = result.Response.Items.OrderByDescending(x => x.Date).Where(x => x.Date > lastEpoch);
            //Filter by likes and views
            var filtered = todays.Where(x => x.Views.Count > 30000 && x.Likes.Count > 100);
            //Select top rated
            var best = filtered.OrderByDescending(x => x.Views.Count + x.Reposts.Count + x.Likes.Count).Take(3).ToList();
           
            memContext.LastEpoch = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            memContext.Save();
            return best;
        }
    }
}
