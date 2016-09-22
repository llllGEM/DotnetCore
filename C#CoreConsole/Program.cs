using System;
using System.Threading;
using System.Threading.Tasks;
using ConsoleApplication.Functions.Randomizer;
using ConsoleApplication.Functions.Chat;
using System.Diagnostics;
using ConsoleApplication.Functions.Matrix;
using ConsoleApplication.Functions.TypingGame;

namespace ConsoleApplication
{       
    public class Program
    {
        public static ManualResetEvent GlobalResetEvent = new ManualResetEvent(false);
        public static CancellationTokenSource cts;

        public static void Main(string[] args)
        {
            ConsoleInit();
            TypingGame.Begin().Wait();
            //Task.Factory.StartNew(async() => await MainAsync(args), cts.Token).Wait();
            //GlobalResetEvent.WaitOne();
        }

        public static async Task MainAsync(string[] args) 
        {
            try{
                int nb;
                C.WL("Choose program:\n\n 1: SocketChat \n\n 2: HttpChat \n\n");
                C.WL("3: TypingGame \n\n 4: Matrix \n\n 5: Randomizer"); 
                if(args.Length > 0){ U.ParseInt(args[0], out nb); Console.Clear(); }
                else if( !U.ParseInt(C.Read(), out nb)) { await MainAsync(args); }
                switch(nb){
                        case 1: SocketChat.Begin(cts.Token);
                                break;
                        case 2: LauncHttpChatServer();
                                break;
                        case 3: TypingGame.Begin().Wait();
                                break;
                        case 4: Matrix.start(cts.Token);
                                break;
                        case 5: Randomizer.Randomize();
                                break;
                        default: SocketChat.Begin(cts.Token);
                                 break;
                }
            }
            catch( Exception ){}
            finally{ EndTasks(); }
        }

        public static void LauncHttpChatServer()
        {
            ProcessStartInfo psi = new ProcessStartInfo("dotnet");
            psi.WorkingDirectory = "../HttpChat";
            psi.Arguments = "run";
            Process p = Process.Start(psi);
            p.WaitForExit();
        }
        
        public static void ConsoleInit(){
            ProgressBar();
            C.WL(@"
              ______  __ __  ______                ______                       __   
             / ____/_/ // /_/ ____/___  ________  / ____/___  ____  _________  / /__ 
            / /   /_  _  __/ /   / __ \/ ___/ _ \/ /   / __ \/ __ \/ ___/ __ \/ / _ \
           / /___/_  _  __/ /___/ /_/ / /  /  __/ /___/ /_/ / / / (__  ) /_/ / /  __/
           \____/ /_//_/  \____/\____/_/   \___/\____/\____/_/ /_/____/\____/_/\___/ 
                                                                                    ");
            //Console.TreatControlCAsInput = true; //to suppress ctrl + C...
            cts = new CancellationTokenSource();
            Task.Factory.StartNew(() => {
                string[] icon = new string[4]{"| ", "/ ", "--", "\\ "};
                string[] dots = new string[4]{".   ", "..  ", "... ", "...."};
                string[] myNode = new string[10]{"⡇","⠇","⠏","⠛","⠹","⠸","⢸","⣰","⣤","⣆"};
                string[] node = new string[6]{"⠧","⠏","⠛","⠹","⠼","⠶"};
                string[] bars = new string[13]{"▏","▎","▍","▌","▊","▉","█","▇","▆","▅","▃","▂","▁"};
                string[] angles = new string[4]{"◣","◤","◥","◢"};
                string[] moons = new string[4]{"◐","◓","◑","◒"};
                string[] rectangles = new string[4]{"▙","▛","▜","▟"};
                SetConsoleTitle(myNode);
            }, cts.Token);

            Console.CancelKeyPress += (sender, e) => {
                //e.Cancel = true; 
                SocketChat.Listener?.Stop(); // stop listener if socketChat launched in case of control + C
                EndTasks();
                C.WL($"CTRL+C detected! Check the title\n");
                Console.Title = "Stopped by User";
            };
        }
        
        private static void SetConsoleTitle(string[] pattern)
        {
            string title = "";
            while (true){
                for(int i = 0; i< pattern.Length; i++){
                        title = "Running "+pattern[i];
                        Console.Title = title;
                        Thread.Sleep(50);
                }
            }
        }

        public static void ProgressBar(bool clear = true, ConsoleColor color = ConsoleColor.Green, string text = "")
        {
            Console.BackgroundColor = color;
            Console.ForegroundColor = ConsoleColor.Black;
            for(int i = 0; i <= Console.WindowWidth; i++){
               Console.Write(text.PadRight(i));
               Thread.Sleep(1);
               Console.SetCursorPosition(0,0);
            }
            Console.ResetColor();
            if(clear)Console.Clear();
        }

        public static void EndTasks()
        {
            try{ cts?.Cancel(false); }//cancel all tasks without ending process
            catch( Exception ){}
            finally{ cts?.Dispose(); }
            Console.ResetColor();Console.Clear();
        }
    }
}
