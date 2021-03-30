using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;

namespace TelegramTestBot_12062020.Controllers {
    class FileController {

        private String[] Files;
        private String file;
        private long chatID;
        private TelegramBotClient Bot;
        public string path { get; }
        public string[] SendSymbol { get; }
        public bool IsWithFiles { get; }
        public List<String> directories { get; }

        public FileController(TelegramBotClient Bot, long chatID, String[] Files) {
            this.Bot = Bot;
            this.chatID = chatID;
            this.Files = Files;
        }

        public FileController(TelegramBotClient Bot, long chatID, String File) {
            this.Bot = Bot;
            this.chatID = chatID;
            this.file = File;
        }

        public FileController(String path, String[] SendSymbol, bool isWithFiles) {
            this.path = path;
            this.SendSymbol = SendSymbol;
            IsWithFiles = isWithFiles;
            directories = new List<string>();
            foreach (string dir in Directory.GetDirectories(path))
                directories.Add(dir);
            if (IsWithFiles)
                foreach (string dir in Directory.GetFiles(path))
                    directories.Add(dir + SendSymbol[1]);
        }

        public async Task sendFile() {
            await Task.Run(async () => {
                await Bot.SendChatActionAsync(chatID, ChatAction.UploadDocument);
                using (FileStream SourceStream = File.Open(file, FileMode.Open)) {
                    InputOnlineFile fileForSend = new InputOnlineFile(SourceStream);
                    fileForSend.FileName = getClearNameFolder(file);
                    await Bot.SendDocumentAsync(chatID, fileForSend);
                }
            });
        }

        public async Task sendFiles() {
            await Task.Run(async () => {
                await Bot.SendChatActionAsync(chatID, ChatAction.UploadDocument);
                foreach (String file in Files) {
                    using (FileStream SourceStream = File.Open(file, FileMode.Open)) {
                        InputOnlineFile fileForSend = new InputOnlineFile(SourceStream);
                        fileForSend.FileName = getClearNameFolder(file);
                        await Bot.SendDocumentAsync(chatID, fileForSend);
                    }
                }
            });
        }

        private String getClearNameFolder(String folderName) {
            String[] splitfolderName = folderName.Split('\\');
            folderName = splitfolderName[splitfolderName.Length - 1];
            return folderName;
        }

        public List<String> getDirectory() {
            return directories;
        }

        public void checkFinalDirectory() {
            for (int i = 0; i < directories.Count; i++) {
                if (directories[i].Contains(SendSymbol[1]))
                    continue;
                string[] dirs = Directory.GetDirectories(directories[i]);
                string[] files = Directory.GetFiles(directories[i]);
                if (dirs.Length == 0) {
                    if (files.Length == 0)
                        continue;
                    directories[i] += SendSymbol[0];
                }
            }
        }
    }
}
