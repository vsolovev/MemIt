using MemIt;
using Refit;
using Telegram.Bot;

var builder = WebApplication.CreateBuilder(args);
var token = "";
builder.Services.AddSingleton<ITimeProvider, DefaultTimeProvider>();
builder.Services.AddSingleton<BotService>();
builder.Services.AddSingleton<BotUpdateService>();
builder.Services.AddSingleton<MemContext>();
builder.Services.AddHostedService<BotHostedService>();
builder.Services.AddSingleton<ITelegramBotClient>(x => new TelegramBotClient(""));
builder.Services
                .AddRefitClient<IVkApi>(new RefitSettings(new NewtonsoftJsonContentSerializer(SerializerDefaults.JsonSerializerSettings)))
                .ConfigureHttpClient((p, c) => {
                    c.BaseAddress = new Uri("https://api.vk.com");
                    c.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
                });

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
