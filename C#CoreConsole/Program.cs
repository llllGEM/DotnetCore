using System;
using System.Threading;
using System.Threading.Tasks;
using ConsoleApplication.Functions;
using ConsoleApplication.Functions.Chat;

namespace ConsoleApplication
{       
    public class Program
    {
        public static ManualResetEvent GlobalResetEvent = new ManualResetEvent(false);
        public static CancellationTokenSource cts;

        public static void Main(string[] args)
        {
            ConsoleInit();
            Task.Factory.StartNew(async() => await MainAsync(args), cts.Token).Wait();
            //GlobalResetEvent.WaitOne();
        }

        public static async Task MainAsync(string[] args) 
        {
            try{
                int nb;
                C.WL("Choose program:\n\n 1: Randomizer \n\n 2: SocketChat");   
                if(!U.ParseInt(C.Read(), out nb)) {await MainAsync(args);EndTasks();}
                else
                switch(nb){
                        case 1: Randomizer.Randomize();
                                break;
                        case 2: SocketChat.Begin(cts.Token);
                                break;
                        default: Console.Clear();
                                 break;
                }
            }
            catch(Exception){}
            finally{EndTasks();}
        }
        
        public static void ConsoleInit(){
            ProgressBar();
            C.WL(@"
 ██████╗ ██╗ ██╗  ██████╗ ██████╗ ██████╗ ███████╗ ██████╗ ██████╗ ███╗   ██╗███████╗ ██████╗ ██╗     ███████╗
██╔════╝████████╗██╔════╝██╔═══██╗██╔══██╗██╔════╝██╔════╝██╔═══██╗████╗  ██║██╔════╝██╔═══██╗██║     ██╔════╝
██║     ╚██╔═██╔╝██║     ██║   ██║██████╔╝█████╗  ██║     ██║   ██║██╔██╗ ██║███████╗██║   ██║██║     █████╗  
██║     ████████╗██║     ██║   ██║██╔══██╗██╔══╝  ██║     ██║   ██║██║╚██╗██║╚════██║██║   ██║██║     ██╔══╝  
╚██████╗╚██╔═██╔╝╚██████╗╚██████╔╝██║  ██║███████╗╚██████╗╚██████╔╝██║ ╚████║███████║╚██████╔╝███████╗███████╗
 ╚═════╝ ╚═╝ ╚═╝  ╚═════╝ ╚═════╝ ╚═╝  ╚═╝╚══════╝ ╚═════╝ ╚═════╝ ╚═╝  ╚═══╝╚══════╝ ╚═════╝ ╚══════╝╚══════╝");
            //Console.TreatControlCAsInput = true; //to suppress ctrl + C...
            Console.Beep();
            cts = new CancellationTokenSource();
            Task.Factory.StartNew(() => {
                //string bar = "Arretez de regarder le titre bande de cons";
                //string[] icon = new string[4]{"| ", "/ ", "--", "\\ "};
                string[] dots = new string[4]{".   ", "..  ", "... ", "...."};
                string title = "";
                while (true){
                    for(int i = 0; i< dots.Length; i++){
                            //title += bar[i];
                            //title = icon[i].ToString();;
                            title = "Running "+dots[i];
                            Console.Title = title;
                            Thread.Sleep(150);
                    }
                    //title="";
            }}, cts.Token);
            Console.CancelKeyPress += (sender, e) => {
                //e.Cancel = true; 
                SocketChat.Listener?.Stop(); // stop listener if socketChat launched in case of control + C
                EndTasks();
                C.WL($"CTRL+C detected! Check the title\n");
                Console.Title = "Stopped by User";
            };
        }
        
        public static void ProgressBar(bool clear = true, ConsoleColor color = ConsoleColor.Green, string text = "")
        {
            Console.BackgroundColor = color;
            Console.ForegroundColor = ConsoleColor.Black;
            for(int i = 0; i <= Console.WindowWidth; i++){
               Console.Write(text.PadRight(i));
               Thread.Sleep(5);
               Console.SetCursorPosition(0,0);
            }
            Console.ResetColor();
            if(clear)Console.Clear();
        }

        public static void EndTasks()
        {
            try{cts?.Cancel(false);}//cancel all tasks without ending process
            catch(Exception){}
            finally{cts?.Dispose();}
            Console.ResetColor();Console.Clear();
        }
    }
}
