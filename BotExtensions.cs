using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using static Move;

public static class BotExtensions
{
    public static async Task HandleMessageAsync(this ITelegramBotClient client, Message message, CancellationToken cancellationToken)
    {
        if (message is not null && message.Text.Contains(Commands.StartGame))
            await client.InviteToGame(message.Chat.Id, message.From);
    }

    public static async Task HandleCallbackQuery(this ITelegramBotClient client, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        var random = new Random();

        if (Enum.TryParse<Move>(callbackQuery.Data, true, out Move move))
        {
            var botMove = (Move)random.Next(3);
            var task = (move, botMove) switch
            {
                (Rock, Paper) or (Paper, Scissors) or (Scissors, Rock) => client.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Sorry, but you lose.", cancellationToken: cancellationToken),
                (Rock, Scissors) or (Paper, Rock) or (Scissors, Paper) => client.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "You win.", cancellationToken: cancellationToken),
                _ => client.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "This game was a draw.", cancellationToken: cancellationToken)
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