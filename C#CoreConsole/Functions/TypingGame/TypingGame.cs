using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

namespace ConsoleApplication.Functions.TypingGame
{
    public static class TypingGame
    {
        private static HttpClient _Client;
        public static HttpClient Client { get { if(_Client == null) _Client = new HttpClient(); return _Client; } }

        public static List<TimeSpan> Scores { get; set; } = new List<TimeSpan>();

        public static async Task Begin()
        {
            Console.Clear();
            C.WL(@"
            ___ _   _ ___  _ _  _ ____ ____ ____ _  _ ____ 
             |   \_/  |__] | |\ | | __ | __ |__| |\/| |___ 
             |    |   |    | | \| |__] |__] |  | |  | |___");
            Console.CursorVisible = false;
            while (true)
            {
                string randomWord = await Client.GetStringAsync("http://randomword.setgetgo.com/get.php"); 
                restart:
                Stopwatch watch = Stopwatch.StartNew();
                retry:
                Display.ResetCursor(randomWord);
                foreach(var c in randomWord)
                {
                    string key = Console.ReadKey(true).Key.ToString().ToLower();
                    if(key == "escape") { Display.Score(); goto restart; }
                    if(key == c.ToString().ToLower())
                        Display.ValidateChar(c.ToString());
                    else
                        goto retry;
                }
                watch.Stop();
                Scores.Add(watch.Elapsed);
            }
        } 
        
    }
}
