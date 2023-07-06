using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Polling;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System.ComponentModel.DataAnnotations;

namespace Passwork.Server.Application.Services
{
    public class TgBotService
    {
        private readonly string token = "5814712472:AAG-PMenYnjT4QnmeMc0l2SDBm9IZ7CPb6D";

        readonly TelegramBotClient botClient;
        private Dictionary<string, long> safeOwners = new();
        public string ConnectionString { get; set; }
        public ILogger<TgBotService> _logger { get; }

        public TgBotService(ILogger<TgBotService> logger)
        {
            var token = Environment.GetEnvironmentVariable("BOT_TOKEN");
            _logger = logger;
            botClient = new TelegramBotClient(token);
            botClient.StartReceiving(HandleUpdateAsync, HandlePollingErrorAsync);
            _logger.LogInformation("TG BOT STARTED: " + botClient.BotId);
            _logger.LogInformation("TG BOT TOKEN: " + token);
        }


        public async Task Notify(string text, string email)
        {
            if(safeOwners.TryGetValue(email, out var chatId))
            {
                await botClient.SendTextMessageAsync(chatId: chatId, text:
                   "Новое событие: " + DateTime.Now.ToString() + "\n" + text);
            }
        }

        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Telegram.Bot.Types.Update update, CancellationToken cancellationToken)
        {
            if (update.Message is not { } message)
                return;
            var chatId = message.Chat.Id;
            if (message.Text is not { } messageText)
                return;
            if (message.Text.Contains("/start"))
            {
                await botClient.SendTextMessageAsync(chatId: chatId, text: "Для получения информации о изминениях в сейфе -> \n" +
                                                                           "Введите email и ключ из админ панели в виде:\n" +
                                                                           "key xxxx you_email@mail.ru");
            }
            if (message.Text == "key " + ConnectionString)
            {
                var words = ConnectionString.Split(' ');
                if(words.Length < 1)
                {
                    await botClient.SendTextMessageAsync(chatId: chatId,
                   text: "Ошибка в строке подключения, должна быть введена три слова через пробелы", cancellationToken: cancellationToken);
                    return;
                }
                AddListener(message.Chat.Id, words[1]);
                Message sentMessage = await botClient.SendTextMessageAsync(chatId: chatId,
                    text: "Отправка информации об изминениях на сейфах настроен на этот чат - " + chatId, cancellationToken: cancellationToken);
                return;
            }
            if (message.Text.Contains("key"))
            {
                await botClient.SendTextMessageAsync(chatId: chatId,
                    text: "Обновите админ панель и введите правильный ключ");
            }

            _logger.LogDebug($"Received a '{messageText}' message in chat {chatId}.");
        }

        private Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }

        private void AddListener(long chatId, string email)
        {
            if (!safeOwners.Any(e => e.Key == email))
            {
                safeOwners.Add(email, chatId);
            }
        }
    }
}