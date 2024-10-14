using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json; // Не забудьте добавить Newtonsoft.Json через NuGet
using System.Collections.Generic;
using GenerationBOT.Controls;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot.Exceptions;
using GenerationBOT.Handlers;
using GenerationBOT.Models;
using Microsoft.EntityFrameworkCore;
using System.Net.Sockets;
using static System.Net.Mime.MediaTypeNames;

class Program
{
    private static ITelegramBotClient _botClient;
    private static ReceiverOptions _receiverOptions;
    private static UserPreference UserPreference {  get; set; }
    private static string SelectModel {  get; set; }

    static async Task Main(string[] args)
    {
        _botClient = new TelegramBotClient("7405180620:AAFsErgXT1Bej2UP6PrTAGtZYH47mxgpg7M");
        _receiverOptions = new ReceiverOptions()
        {
            AllowedUpdates = new[]
            {
                UpdateType.Message,
                UpdateType.CallbackQuery,
            },
            ThrowPendingUpdates = true,
        };

        using var cts = new CancellationTokenSource();
        _botClient.StartReceiving(UpdateHandler, ErrorHandler, _receiverOptions, cts.Token);
        var me = await _botClient.GetMeAsync();
        Console.WriteLine($"Бот {me.FirstName} запущен!");
        await Task.Delay(-1);
    }

    private static async Task UpdateHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        try
        {
            

            switch (update.Type)
            {
                case UpdateType.Message:
                    var message = update.Message;
                    var user = message.From;
                    var chat = message.Chat;

                    using (var context = new ApplicationDbContext())
                    {
                        await context.Database.EnsureCreatedAsync();

                        UserPreference = await context.UserPreferences.FirstOrDefaultAsync(p => p.UserId == user.Id, CancellationToken.None);

                        if (UserPreference == null)
                        {

                            Console.WriteLine($"Новый пользователь: {user.FirstName} ({user.Id})");
                            SelectModel = "runware:100@1";
                            var userPref = new UserPreference()
                            {
                                UserId = user.Id,
                                SelectedModel = "runware:100@1",
                                Money = 0,
                                CountGeneration = 0,
                            };
                            await context.UserPreferences.AddAsync(userPref);
                            await context.SaveChangesAsync();
                        }
                        else
                        {
                            SelectModel = UserPreference.SelectedModel;
                        }

                    }

                    switch (message.Type)
                    {
                        case MessageType.Text:
                            if(message.Text == "/start" || message.Text == "/help")
                            {
                                
                                Console.WriteLine(SelectModel);
                                await UserCommands.StartCommand(botClient, chat.Id);

                                return;
                            }
                            else if(message.Text == "💼 Профиль")
                            {
                                try
                                {
                                    var photo = await botClient.GetUserProfilePhotosAsync(chat.Id);
                                    var fileId = photo.Photos[0][0].FileId;
                                    var text = $"""
                                    📝 Имя: {user.FirstName}
                                    🆔 ИД: {user.Id}

                                    💳 Баланс: {UserPreference.Money}
                                    📊 Всего сгенерировано: {UserPreference.CountGeneration}
                                    💻 Модель AI: {((UserPreference.SelectedModel == "runware:100@1") ? ("✅ Стандартная модель генерации") : ("🔞 NSWF модель генерации"))}
                                    """;
                                    await botClient.SendPhotoAsync(chat.Id, new InputFileId(fileId), caption: text);
                                }
                                catch
                                {
                                    var text = $"""
                                    📝 Имя: {user.FirstName}
                                    🆔 ИД: {user.Id}

                                    💳 Баланс: {UserPreference.Money}
                                    📊 Всего сгенерировано: {UserPreference.CountGeneration}
                                    """;
                                    await botClient.SendTextMessageAsync(chat.Id, text);
                                }
                                
                            }
                            else if(message.Text == "💵 Пополнить баланс")
                            {
                                return;
                            }
                            else if(message.Text == "⚙ Выбрать модель AI")
                            {
                                await botClient.DeleteMessageAsync(chat.Id, message.MessageId - 1);
                                await UserCommands.SettingsGeneration(botClient, chat.Id);

                            }
                            else if(message.Text == "✅ Стандартная модель генерации")
                            {
                                using (var context = new ApplicationDbContext())
                                {
                                    var existingUser = await context.UserPreferences.FirstOrDefaultAsync(p => p.UserId == user.Id);
                                    if (existingUser != null)
                                    {
                                        await UserCommands.StartCommand(botClient, chat.Id, true);
                                        existingUser.SelectedModel = "runware:100@1";
                                        SelectModel = "runware:100@1";
                                        await context.SaveChangesAsync();
                                    }
                                }
                            }    
                            else if(message.Text == "🔞 NSWF модель генерации")
                            {
                                using (var context = new ApplicationDbContext())
                                {
                                    var existingUser = await context.UserPreferences.FirstOrDefaultAsync(p => p.UserId == user.Id);
                                    if (existingUser != null)
                                    {
                                        await UserCommands.StartCommand(botClient, chat.Id, true);
                                        existingUser.SelectedModel = "civitai:133005@782002";
                                        SelectModel = "civitai:133005@782002";
                                        await context.SaveChangesAsync();
                                    }
                                }
                            }
                            else
                            {
                                using (var context = new ApplicationDbContext())
                                {
                                    var existingUser = await context.UserPreferences.FirstOrDefaultAsync(p => p.UserId == user.Id);
                                    if (existingUser != null)
                                    {
                                        existingUser.CountGeneration += 1;
                                        await context.SaveChangesAsync();
                                    }
                                }
                                await botClient.SendTextMessageAsync(chat.Id, "Изображение скоро будет сгенерировано", replyToMessageId: message.MessageId);
                                var url = await UserCommands.CreateImage(message.Text ?? "R", UserPreference.SelectedModel);
                                using (var client = new HttpClient())
                                {
                                    var imageBytes = await client.GetByteArrayAsync(url);
                                    using (var stream = new System.IO.MemoryStream(imageBytes))
                                    {
                                        var inputFile = new InputFileStream(stream, "image.jpg");

                                        await botClient.SendPhotoAsync(chat.Id, inputFile, caption: $"Изображение по запросу {message.Text} сгенерировано");
                                        await botClient.DeleteMessageAsync(chat.Id, message.MessageId + 1);
                                    }
                                }
                            }
                            return;
                    }
                    return;
            }
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex.Message.ToString());
        }
    }

    private static Task ErrorHandler(ITelegramBotClient botClient, Exception error, CancellationToken cancellationToken)
    {
        
        var ErrorMessage = error switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => error.ToString()
        };

        Console.WriteLine(ErrorMessage);
        return Task.CompletedTask;
    }




}
