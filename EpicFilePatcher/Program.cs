using EpicFilePatcher.Common;
using System.Diagnostics;

namespace EpicFilePatcher
{
    internal class EpicFilePatcher
    {
        public const string FileExt = ".efptxt";

        public static bool MakeBackup = true;
        public static bool WriteDebugBTs = false;

        static void Main(string[] args)
        {
            
            args = new string[]
            {
                "--debug",
                @"E:\Workspace\Projects\SWBFII-Cinematic-Tools-Patch\main.efptxt" ,
                @"E:\Workspace\Projects\SWBFII-Cinematic-Tools-Patch\CT_SWBF2.dll"
            };
            


            HandleOptions(ref args);

            FileInfo fileToPatch = null;
            FileInfo patchDataFile = null;

            foreach (string s in args)
            {
                if (File.Exists(s))
                {
                    if (s.EndsWith(FileExt) && patchDataFile == null)
                    {
                        patchDataFile = new FileInfo(s);
                        continue;
                    }
                    fileToPatch = new FileInfo(s);
                    if (patchDataFile != null)
                    {
                        break;
                    }
                }
            }


            Lexer lexer = new Lexer(patchDataFile);
            List<Token> tokens = lexer.Tokenize();
            foreach (Token token in tokens)
            {
                Console.WriteLine($"[{token.Type}]   [data = {token.Literal}]");
            }

            Parser parser = new Parser(fileToPatch);
            parser.Execute(ref tokens);

            Console.WriteLine("\nDone! (debug: { tokens=" + tokens.Count + " })");
            Console.ReadLine();
            Environment.Exit(0);
        }

        static void HandleOptions(ref string[] args)
        {
            foreach (string arg in args)
            {
                switch(arg)
                {
                    case "--nobackup":
                    case "--nb":
                        MakeBackup = false;
                        break;

                    case "--debug":
                        WriteDebugBTs = true;
                        break;
                }
            }
        }
    }
}