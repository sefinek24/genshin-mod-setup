using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Genshin_Stella_Setup.Forms;
using Genshin_Stella_Setup.Scripts;

namespace Genshin_Stella_Setup
{
    internal abstract class Actions
    {
        // Other
        public const string Line =
            "===============================================================================================";

        // Questions
        public static string ShortcutQuestion;
        public static string MShortcutQuestion;

        // Other
        public static string GameGenshinImpact =
            $@"{Installation.ProgramFiles}\Genshin Impact\Genshin Impact game\GenshinImpact.exe";

        public static string GameYuanShen =
            $@"{Installation.ProgramFiles}\Genshin Impact\Genshin Impact game\YuanShen.exe";

        public static string GameExeGlobal;
        public static string GameDirGlobal;
        public static string ReShadeConfig;
        public static string ReShadeLogFile;

        // Attempts
        public static int AttemptNumber = 1;

        public static void Close()
        {
            Console.WriteLine();
            if (Debugger.IsAttached) return;

            Console.ReadLine();
            Environment.Exit(0);
        }

        private static void Canceled()
        {
            Console.WriteLine($"\n{Line}");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("                         Operation canceled. You can close this window.");

            while (true) Console.ReadLine();
        }

        public static async Task WrongAnswer(string fileName)
        {
            Console.ResetColor();
            Console.WriteLine($"\n{Line}");

            AttemptNumber++;
            if (AttemptNumber >= 6)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("                       Too many attempts. Close this window and try again.");

                while (true) Console.ReadLine();
            }

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("                   Wrong answer. Expected 'y' or 'n'. Click ENTER to try again.");
            Console.ResetColor();
            Console.ReadLine();

            switch (fileName)
            {
                case "Start":
                    Console.Clear();
                    await Program.Main();
                    break;
                case "Program":
                    await Questions();
                    break;
                default:
                    Log.ErrorAndExit(new Exception("Failed."), false, true);
                    break;
            }
        }

        public static async Task Questions()
        {
            if (File.Exists(Installation.InstalledViaSetup))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("?? Delete old program data? [Yes/no]: ");
                Console.ResetColor();

                var deleteData = Console.ReadLine() ?? string.Empty;
                if (Regex.Match(deleteData, "(?:y)", RegexOptions.IgnoreCase | RegexOptions.Multiline).Success)
                {
                    Directory.Delete(Program.AppData, true);
                    await Telemetry.Post("Deleted program data from %AppData%.");
                }
            }

            if (!Directory.Exists(Program.AppData)) Directory.CreateDirectory(Program.AppData);


            // Questions etc...
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("?? Create a desktop shortcut? [Yes/no]: ");
            Console.ResetColor();
            ShortcutQuestion = Console.ReadLine();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("?? Create new shortcuts in the Start menu? [Yes/no]: ");
            Console.ResetColor();
            MShortcutQuestion = Console.ReadLine();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("?? Enter the path to the game here: ");
            Console.ResetColor();


            if (File.Exists($@"{Program.AppData}\game-path.sfn"))
            {
                var fileWithGamePath = File.ReadAllText($@"{Program.AppData}\game-path.sfn").Trim();
                if (Directory.Exists(fileWithGamePath)) GameDirGlobal = fileWithGamePath;
            }
            else
            {
                if (File.Exists(GameGenshinImpact))
                    GameDirGlobal = Path.GetDirectoryName(Path.GetDirectoryName(GameGenshinImpact));

                if (File.Exists(GameYuanShen))
                    GameDirGlobal = Path.GetDirectoryName(Path.GetDirectoryName(GameYuanShen));
            }

            if (Directory.Exists(GameDirGlobal))
            {
                File.WriteAllText($@"{Program.AppData}\game-path.sfn", GameDirGlobal);
                Console.WriteLine(GameDirGlobal);
            }
            else
            {
                Application.Run(new SelectPath { Icon = Icon.ExtractAssociatedIcon("Data/Images/52x52.ico") });
            }


            if (Directory.Exists(GameDirGlobal))
            {
                ReShadeConfig = $@"{GameDirGlobal}\Genshin Impact game\ReShade.ini";
                ReShadeLogFile = $@"{GameDirGlobal}\Genshin Impact game\ReShade.log";
            }
            else
            {
                Console.WriteLine();
            }


            // Are you ready?
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("?? All is ready. Install now? [Yes/no]: ");
            Console.ResetColor();

            var answer = Console.ReadLine()?.ToLower().Trim();
            switch (answer)
            {
                case "y":
                case "yes":
                    Log.Output("Initialization...");
                    await Installation.Start();
                    break;

                case "n":
                case "no":
                    Canceled();
                    break;

                default:
                    await WrongAnswer("Program");
                    break;
            }

            Console.ReadLine();
        }
    }
}
