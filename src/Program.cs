using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

var client = new TelegramBotClient(Configuration.BotKey);
var cancellationSource = new CancellationTokenSource();
var cancellationToken = cancellationSource.Token;

client.StartReceiving(
    HandleUpdateAsync,
    HandleErrorAsync,
    new ReceiverOptions { AllowedUpdates = { } },
    cancellationToken
);

var commands = new List<BotCommand> { new BotCommand { Command = Commands.StartGame, Description = "start new game" } };
await client.SetMyCommandsAsync(commands, cancellationToken: cancellationToken); 

Console.WriteLine($"Start listening..");
Console.ReadLine();

cancellationSource.Cancel();

async Task HandleUpdateAsync(ITelegramBotClient client, Update update, CancellationToken cancellationToken)
{
    var task = update.Type switch
    {
        UpdateType.Message => client.HandleMessageAsync(update.Message, cancellationToken),
        UpdateType.CallbackQuery => client.HandleCallbackQuery(update.CallbackQuery, cancellationToken),
        _ => Task.CompletedTask
    };

    try
    {
        await task;
    }
    catch (Exception ex)
    {
        LogUnexpectedError(ex);
    }
}

Task HandleErrorAsync(ITelegramBotClient client, Exception exception, CancellationToken cancellationToken)
{
    LogUnexpectedError(exception);
    return Task.CompletedTask;
}

void LogUnexpectedError(Exception exception) => Console.WriteLine($"Unexpected error is happened: {exception.Message}\n StackTrace: {exception.StackTrace}");