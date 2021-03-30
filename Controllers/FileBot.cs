using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.Payments;
using Telegram.Bot.Types.ReplyMarkups;


namespace TelegramTestBot_12062020.Controllers {
    class FileBot {

        public static TelegramBotClient Bot;
        public static String BotDirectory = "G:\\BotFolder";
        public static Dictionary<long, UserController> Users = new Dictionary<long, UserController>();

        private static Dictionary<long, String> FilesUserQueue = new Dictionary<long, string>();
        private static Dictionary<long, Message> OrderQueue = new Dictionary<long, Message>();
        private static String ProviderToken = "635983722:LIVE:i83626385734";
        private static String[] Social = { "Telegram", "Instagram", "VK" };
        private static String BotLink = "@pripadoc_bot";
        private static String[] sendSymbol = { ">", "►" };
        private static long[] adminsID = { /*870406732*/ };
        private static String[] Links = { "https://t.me/CMETAHKA", "https://instagram.com/pripa.doc", "https://vk.com/mr.greenson" };
        private static long Channel = -1001406926048;
        private static int AdminInfoCounter = 0;
        private static int Discount = 20;
        private static bool IsWithFiles = true;

        private Update update;
        private UpdateType msgType;
        private String wayNow = "";
        private Message msg;
        private CallbackQuery callback;
        private PreCheckoutQuery preCheck;
        private ShippingQuery shipping;
        private String msgText;
        private long chatID;

        public FileBot() { }

        public FileBot(Update update) {
            msgType = update.Type;
            this.update = update;
        }

        public async Task Start() {
            getUserSettings(update);
            switch (msgType) {
                case UpdateType.Message:
                    Console.WriteLine($"@{msg.From.Username} ({msg.From.Id} {msg.From.FirstName} {msg.From.LastName}): {msgText}");
                    switch (msgText) {
                        case "/start":
                            UserProfilePhotos photos = await Bot.GetUserProfilePhotosAsync(msg.From.Id);
                            if (photos.Photos.Length != 0)
                                using (FileStream saveLocation = new FileStream($@"C:\Users\Home\Desktop\{chatID}.png", FileMode.Create)) {
                                    await Bot.GetInfoAndDownloadFileAsync(photos.Photos[0][2].FileId, saveLocation);
                                };
                            await sendCommandInfo();
                            await resetSettings(true);
                            await sendFolderContent(BotDirectory);
                            break;
                        case "/info":
                            await sendCommandInfo(true);
                            break;
                        case "/donate":
                            await sendCommandDonate();
                            break;
                        default:
                            break;
                    }
                    msgText = null;
                    break;

                case UpdateType.CallbackQuery:
                    Console.WriteLine($"@{callback.Message.Chat.Username} ({callback.Message.Chat.Id} {callback.Message.Chat.FirstName} {callback.Message.Chat.LastName}): {callback.Data}");
                    if (await isNavigation())
                        return;

                    Users[chatID].wayNow += Users[chatID].wayNow != "" ? $"\\{callback.Data}" : callback.Data;
                    wayNow = CheckWayError(Users[chatID].wayNow);

                    if (await isSendFile()) {
                        if (Users[chatID].MessagesForEdit.Count == 0)
                            await sendFolderContent(BotDirectory);
                        return;
                    }
                    await sendFolderContent(BotDirectory, wayNow);
                    break;

                case UpdateType.ShippingQuery:
                    break;

                case UpdateType.PreCheckoutQuery:

                    await Bot.AnswerPreCheckoutQueryAsync(preCheck.Id);

                    if (preCheck.InvoicePayload == "Донат") {
                        Users[chatID].isDonated = true;
                        await Bot.SendTextMessageAsync(chatID, "Большое спасибо за поддержку 😊❤️");
                        return;
                    }

                    await Bot.EditMessageTextAsync(
                   Channel,
                   OrderQueue[chatID].MessageId,
                   $"<b>Заказ ✅</b>\n\n" +
                   $"" +
                   $"🤖 Бот: {BotLink}\n" +
                   $"🆔 Пользователь: @{preCheck.From.Username} ({getUserName(preCheck.From)})\n" +
                   $"📎 Файл: <i>{preCheck.InvoicePayload}</i>\n" +
                   $"💳 Сумма: {((double)preCheck.TotalAmount / 100).ToString().Replace(',', '.')} {preCheck.Currency}",
                   ParseMode.Html);

                    Console.WriteLine($"@{preCheck.From.Username} ({preCheck.From.FirstName}): Оплачено.");

                    await sendFiles(preCheck.InvoicePayload, true);
                    await resetSettings();
                    await sendFolderContent(BotDirectory);
                    break;

                default:
                    break;
            }
        }

        private async Task<bool> isNavigation() {
            switch (callback.Data) {
                case "Главная":
                    wayNow = "";
                    await sendFolderContent(BotDirectory);
                    return true;
                case "Назад":
                    await backFolderContent();
                    return true;
                case "Создатель":
                    await sendCreatorInfo();
                    await resetSettings(true);
                    return true;
                case "Предложения":
                    return true;
                case "Отмена транзакции":
                    ChoisenFile order = Users[chatID].MessagesForDelete.Where(d => d.Name == "Оплата").FirstOrDefault();
                    Users[chatID].MessagesForDelete.Remove(order);
                    await Bot.DeleteMessageAsync(chatID, order.Message.MessageId);
                    await resetSettings(true);
                    await sendFolderContent(BotDirectory);
                    return true;
                case "btNext":
                    AdminInfoCounter++;
                    if (AdminInfoCounter == Social.Length)
                        AdminInfoCounter = 0;
                    await sendCreatorLink();
                    return true;
                case "btBack":
                    AdminInfoCounter--;
                    if (AdminInfoCounter == -1)
                        AdminInfoCounter = Social.Length - 1;
                    await sendCreatorLink();
                    return true;
                case "Инфо":
                    await sendCommandInfo(true);
                    return true;
                case "Номер":
                    await Bot.SendContactAsync(chatID, "+380936596683", "Дмитрий");
                    return true;
                case "Поддержать":
                    List<List<InlineKeyboardButton>> buttons = new List<List<InlineKeyboardButton>>();
                    buttons.Add(
                        new List<InlineKeyboardButton>() {
                            new InlineKeyboardButton{ Pay = true, Text = "Поддержать" },
                            new InlineKeyboardButton{ Text = "Другой способ оплаты", Url = "https://www.liqpay.ua/ru/checkout/card/i83626385734" }
                        }
                    );
                    await sendInvoice("Поддержка разработчика", "🍰 На кофе", "Донат", 1000, ReplyMarkup: new InlineKeyboardMarkup(buttons));
                    await sendInvoice("Поддержка разработчика", "🍰 На тортик", "Донат", 5000, ReplyMarkup: new InlineKeyboardMarkup(buttons));
                    await sendInvoice("Поддержка разработчика", "🍱 На обед", "Донат", 8000, ReplyMarkup: new InlineKeyboardMarkup(buttons));
                    await sendInvoice("Поддержка разработчика", "❤️ На спасибо", "Донат", 10000, ReplyMarkup: new InlineKeyboardMarkup(buttons));
                    return true;
                default:
                    return false;
            }
        }

        private async Task sendCommandDonate(bool isRequired = false) {
            List<List<InlineKeyboardButton>> buttons = new List<List<InlineKeyboardButton>>();
            buttons.Add(
                new List<InlineKeyboardButton>() {
                        new InlineKeyboardButton() { Text = "Поддержать", CallbackData = "Поддержать" },
                }
            );
            await Bot.SendTextMessageAsync(
                 chatID,
                "Для работы бота необходимы средства, на поддержку работы сервера и моё личное время на развитие и поддержку работы бота.\n\n" +

                "В знак благодарности всех, кто поддержал бота на сумму 100 гривен или больше, будут выданы небольшие бонусы:\n" +
                $"➖ Скидка {Discount}% на все ответы к заданиям\n" +
                "➖ Доступ к методичкам\n" +
                "➖ Бесконечные благодарности ❤️\n\n" +

                "Оплата через [LiqPay](https://www.liqpay.ua/ru/checkout/i83626385734)\n\n" +

                "(После оплаты этим способом, напишите по контактам ниже)\n\n" +

                "По вопросам оплаты: @CMETAHKA",
                 ParseMode.Markdown,
                 replyMarkup: new InlineKeyboardMarkup(buttons)
             );
        }

        private async Task sendCommandInfo(bool isRequired = false) {
            if (!Users[chatID].isHello || isRequired) {
                Users[chatID].isHello = true;
                await Bot.SendTextMessageAsync(
                    msg.From.Id,
                    "📚 Я помогу тебе с предметами на факультете КН специальности ВПС\n\n" +

                    "➖ Для того чтобы найти нужное задание по предмету, просто напишите /start, выбрав <i>Курс</i> > <i>Предмет</i> > <i>Задание</i>\n" +
                    "➖ Некоторые задания платные, цена написана в скобках\n" +
                    "Пример: <i>2 курс\\ОТПВ\\ПЗ 3 отчет</i> <b>(30грн)</b>\n\n" +

                    "▶️ Список команд:\n" +
                    "➖ /info - информация\n" +
                    "➖ /donate - поддержать разработчика\n\n" +

                    "⌚️ График работы: 24/7.\n" +
                    "📱 Контактный номер: +380936596683\n" +
                    "🙋🏻‍♂️ По всем вопросам и предложениям пишите @CMETAHKA\n" +
                    "☕️ Поддержать бота материально, в чем он очень нуждается: /donate",
                    ParseMode.Html
                );
            }
        }

        private async Task<Message> sendInvoice(String Title, String Description, String PayLoad, List<LabeledPrice> Prices, String PhotoUrl = null, int PhotoHeight = 0, int PhotoWidth = 0, InlineKeyboardMarkup ReplyMarkup = null) {
            return await Bot.SendInvoiceAsync(
                               (int)chatID,
                               Title,
                               Description,
                               PayLoad,
                               ProviderToken,
                               "start",
                               "UAH",
                               Prices,
                               photoUrl: PhotoUrl,
                               photoHeight: PhotoHeight,
                               photoWidth: PhotoWidth,
                               needName: true,
                               needEmail: true,
                               sendEmailToProvider: true,
                               replyMarkup: ReplyMarkup
                               );
        }

        private async Task<Message> sendInvoice(String Title, String Description, String PayLoad, int Price, String PhotoUrl = null, int PhotoHeight = 0, int PhotoWidth = 0, InlineKeyboardMarkup ReplyMarkup = null) {
            return await Bot.SendInvoiceAsync(
                               (int)chatID,
                               Title,
                               Description,
                               PayLoad,
                               ProviderToken,
                               "start",
                               "UAH",
                               new LabeledPrice[] { new LabeledPrice { Label = Description, Amount = Price } },
                               photoUrl: PhotoUrl,
                               photoHeight: PhotoHeight,
                               photoWidth: PhotoWidth,
                               needName: true,
                               needEmail: true,
                               sendEmailToProvider: true,
                               replyMarkup: ReplyMarkup
                               );
        }

        private async Task resetSettings(bool isActionCancel = false) {

            List<ChoisenFile> delete = Users[chatID].MessagesForDelete;
            if (delete.Count > 0)
                for (int i = 0; i < delete.Count; i++)
                    await Bot.DeleteMessageAsync(chatID, delete[i].Message.MessageId);

            if (OrderQueue.ContainsKey(chatID) && isActionCancel) {
                ChoisenFile messForEdit = Users[chatID].MessagesForEdit.Where(e => e.Name == "Заголовок").FirstOrDefault();
                await Bot.EditMessageTextAsync(chatID, messForEdit.Message.MessageId, $"{messForEdit.Text} <i>(Транзакция отменена)</i>", ParseMode.Html);
                await Bot.EditMessageTextAsync(Channel, OrderQueue[chatID].MessageId, $"" +
                    OrderQueue[chatID].Text.Replace("🛒", "❌"),
                    ParseMode.Html);
                OrderQueue.Remove(chatID);
            }

            Users[chatID].MessagesForEdit.Clear();
            Users[chatID].MessagesForDelete.Clear();
            Users[chatID].wayNow = "";
            callback = null;
            msg = null;
            msgText = String.Empty;
            wayNow = "";
        }

        public void getUserSettings(Update e) {
            Update upd = e;
            msgType = upd.Type;
            switch (msgType) {
                case UpdateType.CallbackQuery:
                    chatID = upd.CallbackQuery.Message.Chat.Id;
                    break;
                case UpdateType.PreCheckoutQuery:
                    chatID = upd.PreCheckoutQuery.From.Id;
                    break;
                case UpdateType.ShippingQuery:
                    chatID = upd.ShippingQuery.From.Id;
                    break;

                default:
                    chatID = upd.Id;
                    break;
            }
            if (Users.ContainsKey(chatID)) {
                wayNow = Users[chatID].wayNow;
                callback = upd.CallbackQuery;
                msg = upd.Message;
                update = upd;
                preCheck = upd.PreCheckoutQuery;
                if (msgType == UpdateType.CallbackQuery) {
                    Users[chatID].CallbackHistory.Add(upd.CallbackQuery);
                }
                else if (msgType == UpdateType.Message) {
                    Users[chatID].MessagesHistory.Add(upd.Message);
                    msgText = upd.Message.Text;
                }
            }
            else {
                if (msgType == UpdateType.Message) {
                    msg = e.Message;
                    msgText = msg.Text;
                    Users.Add(chatID, new UserController() {
                        MessagesHistory = new List<Message>() { upd.Message },
                        MessagesForEdit = new List<ChoisenFile>(),
                        MessagesForDelete = new List<ChoisenFile>(),
                        CallbackHistory = new List<CallbackQuery>(),
                    });
                }
                else if (msgType == UpdateType.CallbackQuery) {
                    callback = upd.CallbackQuery;
                    update = upd;
                    Users.Add(chatID, new UserController() {
                        MessagesHistory = new List<Message>(),
                        CallbackHistory = new List<CallbackQuery>() { upd.CallbackQuery },
                        MessagesForEdit = new List<ChoisenFile>(),
                        MessagesForDelete = new List<ChoisenFile>(),
                    });
                }
                else if (msgType == UpdateType.PreCheckoutQuery) {
                    shipping = upd.ShippingQuery;
                    update = upd;
                    Users.Add(chatID, new UserController() {
                        MessagesHistory = new List<Message>(),
                        CallbackHistory = new List<CallbackQuery>(),
                        MessagesForEdit = new List<ChoisenFile>(),
                        MessagesForDelete = new List<ChoisenFile>(),
                    });
                }
                else if (msgType == UpdateType.ShippingQuery) {
                    preCheck = upd.PreCheckoutQuery;
                    update = upd;
                    Users.Add(chatID, new UserController() {
                        MessagesHistory = new List<Message>(),
                        CallbackHistory = new List<CallbackQuery>(),
                        MessagesForEdit = new List<ChoisenFile>(),
                        MessagesForDelete = new List<ChoisenFile>(),
                    });
                }
            }
        }
        private async Task backFolderContent() {
            String[] splitWay = wayNow.Split('\\');

            String newWay = "";
            for (int i = 0; i < splitWay.Length - 1; i++)
                newWay += (i == 0) ? splitWay[i] : $"\\{splitWay[i]}";
            wayNow = newWay;

            String path = splitWay.Length < 2 ? BotDirectory : newWay;
            Users[chatID].wayNow = path;

            await sendFolderContent(BotDirectory, path);
        }

        private async Task sendFolderContent(String botPath, String dirPath = "") {
            dirPath = CheckWayError(dirPath);
            String path = $"{botPath}" + ((dirPath == botPath) ? "" : (dirPath == "" ? "" : $"\\{dirPath}"));
            FileController file = new FileController($"{path}", sendSymbol, IsWithFiles);
            file.checkFinalDirectory();
            List<String> directories = file.getDirectory();

            ButtonsController btns = new ButtonsController(botPath, directories, " \\ ", sendSymbol);
            InlineKeyboardMarkup buttons = btns.getButtons(isMain: path != botPath, isBack: path != botPath);

            if (Users[chatID].MessagesForEdit.Where(s => s.Name == "Заголовок").FirstOrDefault() != null) {
                String text = ((dirPath == "" || dirPath.Contains(botPath)) ? "Выберите папку" : $"{dirPath} \\ <b>(Выберите папку)</b>").Replace("\\", " \\ ");
                List<ChoisenFile> settings = Users[chatID].MessagesForEdit;
                ChoisenFile setting = settings.Where(s => s.Name == "Заголовок").FirstOrDefault();
                setEditMessage("Заголовок", text);
                await Bot.EditMessageTextAsync(chatID, setting.Message.MessageId, text, ParseMode.Html, replyMarkup: buttons);
                return;
            }
            dirPath = dirPath.Replace("\\", " \\ ");
            Message messForEdit = await Bot.SendTextMessageAsync(chatID, "Выберите папку", replyMarkup: buttons);
            Users[chatID].MessagesForEdit.Add(new ChoisenFile {
                Name = "Заголовок",
                Text = dirPath,
                Message = messForEdit
            });
        }

        private String CheckWayError(String dirPath) {
            bool isExist = Directory.Exists(BotDirectory + "\\" + dirPath.Replace(sendSymbol[0], "").Replace(sendSymbol[1], ""));
            if (dirPath.Contains(sendSymbol[1]))
                isExist = true;
            Users[chatID].wayNow = wayNow = isExist ? wayNow : "";
            return isExist ? dirPath : "";
        }

        private ChoisenFile getEditMessage(String Name) {
            return Users[chatID].MessagesForEdit.Where(s => s.Name == Name).FirstOrDefault();
        }

        private void setEditMessage(String Name, String Text, Message message = null) {
            List<ChoisenFile> msgsForEdit = Users[chatID].MessagesForEdit;
            ChoisenFile msgForEdit = msgsForEdit.Where(m => m.Name == Name).FirstOrDefault();
            var newMsgForEdit = new ChoisenFile {
                Message = (message == null) ? msgForEdit.Message : message,
                Text = Text,
                Name = Name
            };
            if (msgForEdit != null)
                Users[chatID].MessagesForEdit[Users[chatID].MessagesForEdit.IndexOf(msgForEdit)] = newMsgForEdit;
            else
                Users[chatID].MessagesForEdit.Add(newMsgForEdit);
            ChoisenFile MessagesForEdit = Users[chatID].MessagesForEdit.Where(s => s.Name == Name).FirstOrDefault();
        }

        private async Task<bool> isSendFile() {
            String filesLocaton = wayNow;
            if (filesLocaton.Contains(sendSymbol[0]) || filesLocaton.Contains(sendSymbol[1])) {
                String newFilesLocaton = filesLocaton.Remove(filesLocaton.Length - 1, 1);
                filesLocaton = newFilesLocaton.Replace("\\", " \\ ");

                setEditMessage("Заголовок", filesLocaton);
                ChoisenFile MessagesForEdit = getEditMessage("Заголовок");

                if (filesLocaton.Contains("грн)") && !adminsID.Contains(chatID)) {
                    await Bot.EditMessageTextAsync(MessagesForEdit.Message.Chat.Id, MessagesForEdit.Message.MessageId, $"<b>{MessagesForEdit.Text}</b> <i>(Ожидание транзакции...)</i>", ParseMode.Html);
                    bool isDonated = Users[chatID].isDonated;
                    String filesLocation = MessagesForEdit.Text.Replace(" \\ ", "\\").Replace(sendSymbol[0], "");
                    string[] splitLocation = filesLocation.Split('\\');
                    String[] splitPrice = splitLocation[splitLocation.Length - 1].Split(' ');
                    int price = int.Parse((double.Parse(splitPrice[splitPrice.Length - 1].Replace('.', ',').Replace("(", "").Replace("грн)", "")) * 100).ToString());

                    InlineKeyboardMarkup buttons = new InlineKeyboardMarkup(new[] {
                        new[] {
                            InlineKeyboardButton.WithPayment($"Оплатить"),
                            InlineKeyboardButton.WithCallbackData("Отмена","Отмена транзакции"),
                        }
                        //new[] {
                        //    InlineKeyboardButton.WithCallbackData("Скриншот файла","Скриншот файла"),
                        //},
                    });

                    List<LabeledPrice> prices = new List<LabeledPrice>();
                    prices.Add(new LabeledPrice($"{MessagesForEdit.Text}", price));
                    if (isDonated)
                        prices.Add(new LabeledPrice($"Скидка {Discount}%", -(price * Discount / 100)));

                    Message messForDelete = sendInvoice(
                        $"{MessagesForEdit.Text}",
                        $"Готовое задание по предмету",
                        $"{filesLocation}",
                        prices,
                        "https://lh3.googleusercontent.com/frJofbBD4ZHQQMLs_wcWZSAEckCkdakFQ2j0DbqxYuv_33xjG8zP2-RKXoO7jbyVFZUy-DLR2ekSoBiKgwFZxt1bycOaoJL6Q9XjBoexky30zTKyLht31LbVYNPa-eHmNOk_VQtsVdZabCeJcor9u-oq6Kg1OKJ8G2TQOsdnrj10d7Jvi378Ot5jYTdb_bES_ZbWcScoo3mHddBrebJlxm8vgLHjZ4VAYrKCdPbnYqTN3gxHozhlxQx8ySo_YGZKt_Bn6dzq9b0WAKTl6ahqyrzPGUC3j-4euuGfZLuUuJcFYNiwRDKxZDl5o_rxWJpQjuIhL0hy9F8zuY-cjxK9HySPmi1jpdOchjzyew0O-d-9gXYykCJV_ZH_GjnvCgnuIaxz_WuUa7Ynm2TqdbFuiaVXalPwOrjL3bteaCJLVoB8k-tiKh_15XZm1rMkBRgfu0zZ-YMXLPYNJSDwz4IJd7FmpQ9C6yIzZ6eHtkfNgOwEcjFnZW199sOPqybvvJ9ETVZ-NF7C8kZI1OaQNMA-fmgPyUpyOJICrtZC3WpTO0UhOWyBQC14ju9etfAav9aCygqFH76khQhYa3Qt7xHllO-mByzZTSPqqLc1AbQm6dwk2HnEjpWDHEQQngKuG8tgRBLmy-gg8eWDdAcezn2dl8TFd-wwl8uivo99DEH-hn_skgqi33f0ZYw-NaP3sQ=w976-h406-no?authuser=0",
                        PhotoHeight: 800,
                        PhotoWidth: 1920,
                        ReplyMarkup: buttons
                        ).Result;
                    Message order = await Bot.SendTextMessageAsync(
                        Channel,
                        $"<b>Заказ 🛒</b>\n\n" +
                        $"" +
                        $"🤖 Бот: {BotLink}\n" +
                        $"🆔 Пользователь: @{messForDelete.Chat.Username} ({getUserName(messForDelete.Chat)})\n" +
                        $"📎 Файл: <i>{filesLocation}</i>\n" +
                        $"💳 Сумма: {((double)messForDelete.Invoice.TotalAmount / 100).ToString().Replace(',', '.')} {messForDelete.Invoice.Currency}",
                        ParseMode.Html);

                    if (OrderQueue.ContainsKey(chatID))
                        await Bot.DeleteMessageAsync(Channel, OrderQueue[chatID].MessageId);

                    OrderQueue.Add(chatID, order);
                    Users[chatID].MessagesForDelete.Add(new ChoisenFile { Message = messForDelete, Name = "Оплата" });
                    return true;
                }
                await Task.Run(async () => {
                    await sendFiles(wayNow, false);
                });
                return true;
            }
            else
                return false;
        }

        private InlineKeyboardMarkup getKeyboardCreator() {
            return new InlineKeyboardMarkup(new[] {
                new[] {
                    InlineKeyboardButton.WithUrl("contact by " + Social[AdminInfoCounter], Links[AdminInfoCounter]),
                    //InlineKeyboardButton.WithUrl("Debug.Print(" + '"' + "my " + Social[AdminInfoCounter] + '"' + ");", Links[AdminInfoCounter]),
                },
                new[] {
                    InlineKeyboardButton.WithCallbackData("<","btBack"),
                    InlineKeyboardButton.WithCallbackData(">","btNext")
                },
                new[] {
                    InlineKeyboardButton.WithCallbackData("Вернуться", "Инфо"),
                }
            });
        }

        private async Task sendFiles(String Location, bool isPayed) {
            if (FilesUserQueue.ContainsKey(chatID))
                return;

            await WaitFile();

            ChoisenFile MessagesForEdit = Users[chatID].MessagesForEdit.Where(s => s.Name == "Заголовок").FirstOrDefault();
            if (MessagesForEdit != null)
                await Bot.EditMessageTextAsync(MessagesForEdit.Message.Chat.Id, MessagesForEdit.Message.MessageId, $"<b>{MessagesForEdit.Text}</b> <i>(Отправка...)</i>", ParseMode.Html);
            String location = Location.Replace(sendSymbol[0], "");
            if (location.Contains(sendSymbol[1])) {
                location = Location.Replace(sendSymbol[1], "");
                await new FileController(Bot, chatID, $"{BotDirectory}\\{location}").sendFile();
            }
            else
                await new FileController(Bot, chatID, Directory.GetFiles($"{BotDirectory}\\{location}")).sendFiles();
            String Payed = isPayed ? " <i>(Оплачено ✅)</i>" : "";
            if (MessagesForEdit != null)
                await Bot.EditMessageTextAsync(MessagesForEdit.Message.Chat.Id, MessagesForEdit.Message.MessageId, $"<b>{location.Replace("\\", " \\ ")}</b>{Payed}", ParseMode.Html);
            Users[chatID].MessagesForEdit = new List<ChoisenFile>();
            wayNow = "";
            FilesUserQueue.Remove(chatID);
        }

        private async Task WaitFile() {
            if (FilesUserQueue.ContainsValue(wayNow)) {
                ChoisenFile MessagesForEdit = Users[chatID].MessagesForEdit.Where(s => s.Name == "Заголовок").FirstOrDefault();
                await Bot.EditMessageTextAsync(MessagesForEdit.Message.Chat.Id, MessagesForEdit.Message.MessageId, $"<b>{MessagesForEdit.Text}</b> <i>(Ожидание очереди...)</i>", ParseMode.Html);
                if (!FilesUserQueue.ContainsKey(chatID))
                    FilesUserQueue.Add(chatID, wayNow);
                while (true) {
                    KeyValuePair<long, string> user = FilesUserQueue.FirstOrDefault();
                    if (user.Value == null || user.Key == chatID)
                        break;
                    await Task.Delay(300);
                }
                return;
            }
            if (!FilesUserQueue.ContainsKey(chatID))
                FilesUserQueue.Add(chatID, wayNow);
        }

        private async Task sendCreatorInfo() {
            InlineKeyboardMarkup keyb = getKeyboardCreator();
            await Bot.SendPhotoAsync(chatID, "AgACAgEAAxkBAAIW9V7n5rvEV1kyzfBowAVEtsLsHIJPAAJYqDEbVhRAR5PWPIG3oIvaCj8AAUkXAAMBAAMCAAN5AAOLMQACGgQ", replyMarkup: keyb);
        }

        private async Task sendCreatorLink() {
            InlineKeyboardMarkup keyb = getKeyboardCreator();
            await Bot.EditMessageCaptionAsync(chatID, callback.Message.MessageId, caption: null, replyMarkup: keyb);
        }

        private String getUserName(User user) {
            return $"{user.FirstName}{((!String.IsNullOrWhiteSpace(user.LastName)) ? $" {user.LastName}" : "")}";
        }

        private String getUserName(Chat user) {
            return $"{user.FirstName}{((!String.IsNullOrWhiteSpace(user.LastName)) ? $" {user.LastName}" : "")}";
        }
    }
}
