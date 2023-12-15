using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot;

namespace MemIt
{
    public class BotUpdateService : IUpdateHandler
    {
        private readonly MemContext memContext;
        private readonly BotService botService;
        public BotUpdateService(MemContext memContext, BotService botService)
        {
            this.memContext = memContext;
            this.botService = botService;
        }

        public async Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {

        }
        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Message is Telegram.Bot.Types.Message message)
            {
                var chatId = message.Chat.Id;
                if (memContext.ChatIds.Add(chatId))
                {
                    memContext.Save();
                    await botService.TransferNewPosts(chatId);
                }
                else if (message.Text.ToLower().Contains("дай"))
                {
                    await botService.TransferNewPosts(chatId);
                }
            }
        }
    }
}
