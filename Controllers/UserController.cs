using System;
using System.Collections.Generic;
using Telegram.Bot.Types;

namespace TelegramTestBot_12062020.Controllers {
    class UserController {

        public List<Message> MessagesHistory { get; set; }
        public List<CallbackQuery> CallbackHistory { get; set; }
        public List<ChoisenFile> MessagesForEdit { get; set; }
        public List<ChoisenFile> MessagesForDelete { get; set; }
        public String wayNow { get; set; } = "";
        public bool isHello { get; set; }
        public bool isDonated { get; set; }

    }

    class ChoisenFile {
        public Message Message { get; set; }
        public String Text { get; set; }
        public String Name { get; set; }
    }
}
