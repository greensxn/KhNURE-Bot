using System;
using System.Collections.Generic;

using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramTestBot_12062020.Controllers;

using Newtonsoft.Json;


namespace TelegramTestBot_12062020 {
    class Program {

        private readonly static String jsonLoc = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\KhNure Bot (ВПС).txt";

        static void Main(string[] args) {
            if (System.IO.File.Exists(jsonLoc)) {
                string json = System.IO.File.ReadAllText(jsonLoc);
                FileBot.Users = JsonConvert.DeserializeObject<Dictionary<long, UserController>>(json);
            }

            try {
                Console.Write("Путь к файлам: ");
                FileBot.BotDirectory = Console.ReadLine();
                Console.Write("Bot Key: ");
                FileBot.Bot = new TelegramBotClient(Console.ReadLine());
                FileBot.Bot.OnUpdate += Bot_OnUpdate;
                FileBot.Bot = new TelegramBotClient("1293212735:AAH3N_6GPjP73ecH0ge8etKnAN_tfNwxfoA");
                FileBot.Bot.OnUpdate += Bot_OnUpdate;
            }
            catch {
                Console.WriteLine("Ошибка.");
            }

            FileBot.Bot.StartReceiving();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Бот запущен...");
            Console.ResetColor();
            Console.ReadLine();
            FileBot.Bot.StopReceiving();

            System.IO.File.WriteAllText(jsonLoc, JsonConvert.SerializeObject(FileBot.Users));
        }

        private async static void Bot_OnUpdate(object sender, UpdateEventArgs e) {
            FileBot bot = new FileBot(e.Update);
            await bot.Start();
        }
    }
}
