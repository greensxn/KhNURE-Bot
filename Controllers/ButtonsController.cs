using System;
using System.Collections.Generic;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramTestBot_12062020.Controllers {
    class ButtonsController {

        private readonly string botDirectory;
        private readonly string splitter;
        private readonly string[] sendSymbol;
        private readonly List<String> directories;

        public ButtonsController(String BotDirectory, List<String> directories, String Splitter, String[] sendSymbol) {
            botDirectory = BotDirectory;
            this.directories = directories;
            splitter = Splitter;
            this.sendSymbol = sendSymbol;
        }

        public InlineKeyboardMarkup getButtons(bool isBack = false, bool isMain = false, bool isInfo = false) {
            List<List<InlineKeyboardButton>> btns = new List<List<InlineKeyboardButton>>();
            for (int i = 0; i < directories.Count; i++) {
                btns.Add(new List<InlineKeyboardButton> {
                    new InlineKeyboardButton{
                         CallbackData = getNameFolder(directories[i], true),
                         Text = getNameFolder(directories[i], false)
                    }
                });
            }

            List<InlineKeyboardButton> Navigation = new List<InlineKeyboardButton>();
            if (isBack)
                Navigation.Add(new InlineKeyboardButton {
                    CallbackData = "Назад",
                    Text = "Назад",
                });
            if (isMain)
                Navigation.Add(new InlineKeyboardButton {
                    CallbackData = "Главная",
                    Text = "Главная"
                });
            if (Navigation.Count > 0)
                btns.Add(Navigation);
           
            InlineKeyboardMarkup buttons = new InlineKeyboardMarkup(btns);
            return buttons;
        }

        private String getNameFolder(String text, bool isCallback) {
            string[] textSplitted = text.Split('\\');
            return isCallback ?
                textSplitted[textSplitted.Length - 1] :
                textSplitted[textSplitted.Length - 1].Replace(sendSymbol[0], "").Replace(sendSymbol[1], "");
        }

        private String getClearNameFolder(String text) {
            return text.Replace(botDirectory + "\\", "");
        }

    }
}
