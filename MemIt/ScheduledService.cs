using Telegram.Bot;

namespace MemIt
{
    public class BotHostedService : ScheduledBackgroundServiceBase
    {
        private readonly ILogger<BotHostedService> _logger;
        private ITelegramBotClient botClient;
        private BotService botService;
        private BotUpdateService botUpdateService;
        private readonly MemContext memContext;


        public BotHostedService(ILogger<BotHostedService> logger, ITimeProvider timeProvider, ITelegramBotClient telegramBotClient,
             MemContext memContext,
             BotService botService,
             BotUpdateService botUpdateService)
            : base(logger, timeProvider, "0 18 * * *", triggerOnStart: false)
        {
            _logger = logger;            
            this.botClient = telegramBotClient;
            this.botService = botService;
            this.memContext = memContext;
            this.memContext.Restore();
            this.botUpdateService = botUpdateService;
            this.botClient.ReceiveAsync(this.botUpdateService);
           
        }

        protected override async Task Handle(DateTime occurenceTimeUtc, CancellationToken cancellation)
        {
            await botService.TransferNewPosts();
        }
    }
}
