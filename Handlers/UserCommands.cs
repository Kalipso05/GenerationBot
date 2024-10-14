using GenerationBOT.Controls;
using GenerationBOT.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace GenerationBOT.Handlers
{
    public class UserCommands
    {
        private static string runwareApi = "z1ilk4CqKMMMPSm3gynSdrsuoKsECcxK";
        public static async Task<string> CreateImage(string prompt, string model)
        {
            using (var client = new ClientWebSocket())
            {
                var uri = new Uri("wss://ws-api.runware.ai/v1");
                await client.ConnectAsync(uri, CancellationToken.None);

                var authRequest = new List<Dictionary<string, string>>
            {
                new Dictionary<string, string>
                {
                    { "taskType", "authentication" },
                    { "apiKey", runwareApi }
                }
            };
                await WebSocketControl.SendMessageAsync(client, authRequest);
                await WebSocketControl.ReceiveMessageAsync(client);

                var imageRequest = new List<Dictionary<object, object>>
            {
                new Dictionary<object, object>
                {
                    //runware:100@1
                    //civitai:133005@782002
                    { "positivePrompt", prompt },
                    { "model", $"{model}" },
                    { "steps", 40 },
                    { "width", 832 },
                    { "height", 1216 },
                    { "numberResults", 1 },
                    { "outputType", new[] { "URL" } },
                    { "taskType", "imageInference" },
                    { "taskUUID", Guid.NewGuid().ToString("N") }
                }
            };
                await WebSocketControl.SendMessageAsync(client, imageRequest);
                var imgResponse = await WebSocketControl.ReceiveMessageAsync(client);
                var data = JsonConvert.DeserializeObject<ImageResponse>(imgResponse);
                return data.Data[0].ImageURL;
            }
        }


        public static async Task StartCommand(ITelegramBotClient botClient, long chatId, bool isRepeat = false)
        {
            var replyKeyboard = new ReplyKeyboardMarkup(
                new List<KeyboardButton[]>()
                {
                    new KeyboardButton[]
                    {
                        new KeyboardButton("💼 Профиль"),
                        new KeyboardButton("💵 Пополнить баланс"),
                    },
                    new KeyboardButton[]
                    {
                        new KeyboardButton("⚙ Выбрать модель AI"),
                    },
                })
                { ResizeKeyboard = true };
            if(isRepeat)
            {
                await botClient.SendTextMessageAsync(chatId, "Вы поменяли модель генерации", replyMarkup: replyKeyboard);
                return;
            }
            await botClient.SendTextMessageAsync(
                chatId,
                $"🌟 Добро пожаловать в наш AI-бот! 🌟\r\n\r\nЗдесь ты сможешь превратить свои мысли и идеи в настоящие произведения искусства! 🖌️💫 С помощью искусственного интеллекта мы генерируем изображения по твоим запросам.\r\n\r\n🎨 Возможности бесконечны:\r\nПредставь что угодно — от пейзажей до фантастических миров, от абстракций до детализированных персонажей. Всё, что тебе нужно сделать — это описать, и наш AI воплотит твою идею в изображении!\r\n\r\n🔧 Как начать?\r\n\r\nВведи описание изображения, которое хочешь создать.\r\nНаслаждайся результатом! Каждый запрос — это уникальное творение.\r\nНе стесняйся экспериментировать и удивляться возможностям искусственного интеллекта! \U0001f929✨\r\nГотов? Тогда вперед — твои креативные идеи ждут воплощения! 🎉",
                replyMarkup: replyKeyboard
                );
            
        }

        public static async Task SettingsGeneration(ITelegramBotClient botClient, long chatId)
        {
            var replyKeyboard = new ReplyKeyboardMarkup(
                new List<KeyboardButton[]>()
                {
                    new KeyboardButton[]
                    {
                        new KeyboardButton("✅ Стандартная модель генерации"),
                    },
                    new KeyboardButton[]
                    {
                        new KeyboardButton("🔞 NSWF модель генерации"),
                    },
                })
            { ResizeKeyboard = true };

            await botClient.SendTextMessageAsync(chatId, "Выберите модель для генерации изображений", replyMarkup: replyKeyboard);
        }
    }
}
