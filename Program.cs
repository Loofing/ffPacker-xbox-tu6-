using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ffPacker
{
    class Program
    {
        public static compileInfo compInfo = new compileInfo();
        private static string settingsFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.xml");
        public static string modFolderPath = string.Empty;
        public static string modOutputPath = string.Empty;
        public static int platformValue;
        public static int gameValue;
        public static int languageValue;
        public static bool validParameters = true;
        static void Main(string[] args)
        {
            compInfo = compilerSettings();

            if (args.Length == 0)
            {
                WriteColoredLine("DRAG AND DROP FOLDER ONTO .EXE", ConsoleColor.Red);
                Console.ReadKey();
                return;
            }
            if (Directory.Exists(args[0]))
            {
                checkLanguage();
                checkPlatform();
                checkGame();
                if (!validParameters)
                {
                    Console.ReadKey();
                    return;
                }
                modFolderPath = args[0];
                modOutputPath = Path.Combine(modFolderPath.Substring(0, modFolderPath.LastIndexOf('\\')), compInfo.fileName);
                new new_ff_file().build_zone(Convert.ToByte(platformValue), compInfo.fileName, modOutputPath += ".ff");
            }
            else
                Console.Write("Could not find folder @ {0}", args[0]);

            Console.ReadKey();
        }
        public static compileInfo compilerSettings()
        {
            compileInfo cInfo = new compileInfo();
            XmlSerializer XML = new XmlSerializer(typeof(compileInfo));

            if (!File.Exists(settingsFile))
            {
                cInfo.fileName = "patch_mp";
                cInfo.game = "mw2";
                cInfo.platform = "xbox";
                cInfo.language = "english";

                using (FileStream fs = new FileStream(settingsFile, FileMode.Create))
                {
                    XML.Serialize(fs, cInfo);
                }
                return cInfo;
            }
            using (FileStream fs = new FileStream(settingsFile, FileMode.Open, FileAccess.Read))
            {
                cInfo = (compileInfo)XML.Deserialize(fs);
            }
            if (cInfo.fileName.Contains(".ff"))
            {
                string[] tempName = cInfo.fileName.Split('.');
                cInfo.fileName = tempName[0];
            }
            return cInfo;
        }

        public static void WriteColoredLine(string input, ConsoleColor color = ConsoleColor.White,bool line = true)
        {
            Console.ForegroundColor = color;

            if(line)
                Console.WriteLine(input);
            else
                Console.Write(input);

            Console.ResetColor();
        }

        public enum _languages
        {
            ENGLISH = 1,
            FRENCH = 2,
            GERMAN = 4,
            ITALIAN = 8,
            SPANISH = 16,
            BRITISH = 32,
            RUSSIAN = 64,
            POLISH = 128,
            KOREAN = 256,
            TAIWANESE = 512,
            JAPANESE = 1024,
            CHINESE = 2048,
            THAI = 4096,
            LEET = 8192,
            CZECH = 16384
        }
        public enum _platforms
        {
            xbox
        }
        public enum _games
        {
            mw2
        }

        private static void checkGame()
        {
            try
            {
                gameValue = (int)Enum.Parse(typeof(_games), compInfo.game);
            }
            catch
            {
                WriteColoredLine("Game > ( " + compInfo.game + " ) is not supported, please check the settings.xml file!", ConsoleColor.Red);
                validParameters = false;
            }
        }
        private static void checkPlatform()
        {
            try
            {
                platformValue = (int)Enum.Parse(typeof(_platforms), compInfo.platform);
            }
            catch
            {
                WriteColoredLine("Platform > ( " + compInfo.platform + " ) is not supported, please check the settings.xml file!", ConsoleColor.Red);
                validParameters = false;
            }
        }
        private static void checkLanguage()
        {
            try
            {
                languageValue = (int)Enum.Parse(typeof(_languages), compInfo.language.ToUpper());
            }
            catch
            {
                WriteColoredLine("Language > ( " + compInfo.language + " ) is not supported, please check the settings.xml file!", ConsoleColor.Red);
                validParameters = false;
            }
        }
    }

    [Serializable]
    public class compileInfo
    {
        public string platform { get; set; }
        public string game { get; set; }
        public string language { get; set; }
        public string fileName { get; set; }

    }
}
