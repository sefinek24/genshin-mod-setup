﻿using System;
using System.IO;
using System.Threading;
using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.WindowsAPICodePack.Taskbar;

namespace Genshin_Impact_MP_Installer.Scripts
{
    internal abstract class Log
    {
        public static readonly string Folder = $@"{Program.AppData}\logs";
        public static readonly string ErrorFile = $@"{Folder}\installer.error.log";
        public static readonly string OutputFile = $@"{Folder}\installer.output.log";
        public static readonly string ModInstFile = $@"{Folder}\mod_installation.log";
        private static int _reportTry = 1;

        public static void Output(string log)
        {
            if (!Directory.Exists(Program.AppData)) Directory.CreateDirectory(Program.AppData);
            if (!Directory.Exists(Folder)) Directory.CreateDirectory(Folder);

            using (var sw = File.AppendText(OutputFile))
            {
                sw.WriteLine($"[{DateTime.Now}]: {Console.Title}\n{log.Trim()}\n\n");
            }
        }

        public static void ErrorAuditLog(Exception log, bool sendWebHook)
        {
            if (!Directory.Exists(Program.AppData)) Directory.CreateDirectory(Program.AppData);
            if (!Directory.Exists(Folder)) Directory.CreateDirectory(Folder);

            using (var sw = File.AppendText(ErrorFile))
            {
                sw.WriteLine($"[{DateTime.Now}]: {Console.Title}\n{log}\n\n");
            }

            if (sendWebHook) WebHook.Error(log);
        }

        private static void TryAgain(bool tryAgain)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine(tryAgain ? "\n» Press ENTER to try again..." : "\n» Press ENTER to continue...");
            Console.ReadLine();

            Console.WriteLine(">> Waiting 5 seconds. Please wait... <<");
            Thread.Sleep(5000);

            Console.ResetColor();
            TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.Normal);
        }

        public static void ErrorString(string msg, bool tryAgain)
        {
            ErrorAuditLog(new Exception(msg), true);

            try
            {
                var builder = new ToastContentBuilder()
                    .AddText("Oh no! Something went wrong 😿")
                    .AddText(msg);
                builder.Show();
            }
            catch (Exception e)
            {
                ErrorAuditLog(e, false);
            }

            TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.Error);

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(msg);

            TryAgain(tryAgain);
        }

        public static void Error(Exception msg, bool tryAgain)
        {
            ErrorAuditLog(msg, true);

            try
            {
                var builder = new ToastContentBuilder()
                    .AddText("Oh no! Error occurred 😿")
                    .AddText("Go back to the installer.");
                builder.Show();
            }
            catch (Exception e)
            {
                ErrorAuditLog(e, false);
            }

            TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.Error);

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(string.IsNullOrEmpty(msg.InnerException?.ToString()) ? msg : msg.InnerException);

            TryAgain(tryAgain);
        }

        public static async void ErrorAndExit(Exception log, bool hideError, bool reportIssue)
        {
            ErrorAuditLog(log, true);

            if (!hideError)
            {
                TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.Error);

                try
                {
                    var builder = new ToastContentBuilder()
                        .AddText("Failed to install 😿")
                        .AddText("🎶 Sad song... Something went wrong...");
                    builder.Show();
                }
                catch (Exception e)
                {
                    ErrorAuditLog(e, false);
                }

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{log.Message}\n");

                if (reportIssue)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine(
                        $"Sorry, something went wrong. Critical error occurred.\nPlease report this issue if you can or try again.\n• Discord server: {Program.DiscordUrl} [My username: Sefinek#0001]\n• E-mail: contact@sefinek.net\n• Use the available chat on my website.");
                    Console.ResetColor();
                    Console.WriteLine($"\n{Program.Line}\n");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine(
                        $"Visit our Discord server for help or try again. Good luck!\n• Discord: {Program.DiscordUrl} [My username: Sefinek#0001]\n• E-mail: contact@sefinek.net\n• Use the available chat on my website.");
                    while (true) Console.ReadLine();
                }
            }

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write("» Would you like to send us log files? [Yes/no]: ");
            Console.ResetColor();

            var reportError = Console.ReadLine()?.ToLower();
            switch (reportError)
            {
                case "y":
                case "yes":
                    var hookSuccess = await WebHook.SendLogFiles();
                    if (hookSuccess)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"Success. Sorry for any problems.\nAttached files: {Folder}\n");
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("We apologize. Fatal error with sending webhook.");
                    }

                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine("You can close this window.");
                    break;

                case "n":
                case "no":
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine("): Okay, sorry for any problems. You can close this window.");
                    break;

                default:
                {
                    if (_reportTry >= 3)
                    {
                        Console.WriteLine("Too many attempts. Please close this window.");
                        while (true) Console.ReadLine();
                    }

                    Console.WriteLine("Wrong answer. Please try again...\n");
                    _reportTry++;
                    ErrorAndExit(log, true, true);
                    break;
                }
            }

            while (true) Console.ReadLine();
        }
    }
}