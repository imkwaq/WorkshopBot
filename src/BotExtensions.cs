using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using static Move;

public static class BotExtensions
{
    private const string CantPlayAnotherPersonsGame = "Sorry, @{0}, but you can't play game of another user, try start your own. Type: `/startgame` to start game";

    public static async Task HandleMessageAsync(this ITelegramBotClient client, Message message, CancellationToken cancellationToken)
    {
        if (message is not null && message.Text.Contains(Commands.StartGame))
            await client.InviteToGame(message.Chat.Id, message.From);
    }

    public static async Task HandleCallbackQuery(this ITelegramBotClient client, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        var callbackUsername = callbackQuery.From.Username;
        var chatId = callbackQuery.Message.Chat.Id;

        if (!callbackQuery.Message.Text.Contains(callbackUsername))
        {
            await client.SendTextMessageAsync(callbackQuery.Message.Chat.Id, string.Format(CantPlayAnotherPersonsGame, callbackUsername), cancellationToken: cancellationToken);
        }
        else
        {
            await client.PublishGameResult(chatId, callbackUsername, callbackQuery.Data, cancellationToken);
        }
    }

    private static async Task PublishGameResult(this ITelegramBotClient client, long chatId, string username, string message, CancellationToken cancellationToken)
    {
        var random = new Random();
        if (Enum.TryParse<Move>(message, true, out Move move))
        {
            var botMove = (Move)random.Next(3);
            var gameLog = $"Bot option: `{botMove}`, your option: `{move}`";
            var task = (move, botMove) switch
            {
                (Rock, Paper) or (Paper, Scissors) or (Scissors, Rock) => client.SendTextMessageAsync(chatId, $"Sorry, @{username}, but you lose. {gameLog}", cancellationToken: cancellationToken),
                (Rock, Scissors) or (Paper, Rock) or (Scissors, Paper) => client.SendTextMessageAsync(chatId, $"Hurray! You win, @{username}, congrats! {gameLog}", cancellationToken: cancellationToken),
                _ => client.SendTextMessageAsync(chatId, $"This game was a draw. {gameLog}", cancellationToken: cancellationToken)
            };

            await task;
        }
    }

    public static async Task<Message> InviteToGame(this ITelegramBotClient client, long chatId, User user)
    {
        var inlineKeyboardMarkup = new InlineKeyboardMarkup(GetGameButtons());
        return await client.SendTextMessageAsync(chatId, $"Choose your option, @{user.Username}", replyMarkup: inlineKeyboardMarkup);
    }

    private static List<InlineKeyboardButton> GetGameButtons() =>
        new List<InlineKeyboardButton>()
        {
            new InlineKeyboardButton("📝")
            {
                CallbackData = $"{Paper}"
            },
            new InlineKeyboardButton("✂️")
            {
                CallbackData = $"{Scissors}"
            },
            new InlineKeyboardButton("🗿")
            {
                CallbackData = $"{Rock}"
            }
        };
}