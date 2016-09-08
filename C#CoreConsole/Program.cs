using System;
using System.Threading;
using System.Threading.Tasks;
using ConsoleApplication.Functions;

namespace ConsoleApplication
{       
    public class Program
    {
        static ManualResetEvent resetEvent = new ManualResetEvent(false);
        public static CancellationTokenSource cts;

        public static void Main(string[] args)
        {
            ConsoleInit();
            Task.Factory.StartNew(async() => await MainAsync(args)).Wait();
            //resetEvent.WaitOne();
        }

        public static async Task MainAsync(string[] args) 
        {
            try{
                int nb;
                C.Display("Choose program:\n 1: Randomizer \n 2: GlobalChatServer \n 3: GlobalChatClient ");
                C.Display(" 4: PrivateChatServer \n 5: PrivateChatClient");    
                if(!U.ParseInt(C.Read(), out nb)) {await MainAsync(args);EndTasks();}
                else
                switch(nb){
                        case 1: Randomizer.Randomize(cts.Token);
                                break;
                        case 2: Chat.Public.ServerStartAsync().Wait();
                                break;
                        case 3: Chat.Public.ClientConnectAsync(cts.Token).Wait();
                                break;
                        case 4: Chat.Private.ServerStartAsync(cts.Token).Wait();
                                break;
                        case 5: Chat.Public.ClientConnectAsync(cts.Token, U.PrivatePort).Wait();
                                break;
                        default: Console.Clear();
                                 break;
                }
            }
            catch(Exception){}
            finally{EndTasks();}
        }
        
        public static void ConsoleInit(){
            //Console.TreatControlCAsInput = true; //to suppress ctrl + C...
            Console.Beep();
            cts = new CancellationTokenSource();
            Task.Factory.StartNew(() => {
                string bar = "Arretez de regarder le titre bande de cons";
                string[] icon = new string[4]{"| ", "/ ", "--", "\\ "};
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
                EndTasks();
                C.Display($"CTRL+C detected! Check the title\n");
                Console.Title = "Stopped by User";
            };
            ProgressBar();
            C.Display(@"
 ██████╗ ██╗ ██╗  ██████╗ ██████╗ ██████╗ ███████╗ ██████╗ ██████╗ ███╗   ██╗███████╗ ██████╗ ██╗     ███████╗
██╔════╝████████╗██╔════╝██╔═══██╗██╔══██╗██╔════╝██╔════╝██╔═══██╗████╗  ██║██╔════╝██╔═══██╗██║     ██╔════╝
██║     ╚██╔═██╔╝██║     ██║   ██║██████╔╝█████╗  ██║     ██║   ██║██╔██╗ ██║███████╗██║   ██║██║     █████╗  
██║     ████████╗██║     ██║   ██║██╔══██╗██╔══╝  ██║     ██║   ██║██║╚██╗██║╚════██║██║   ██║██║     ██╔══╝  
╚██████╗╚██╔═██╔╝╚██████╗╚██████╔╝██║  ██║███████╗╚██████╗╚██████╔╝██║ ╚████║███████║╚██████╔╝███████╗███████╗
 ╚═════╝ ╚═╝ ╚═╝  ╚═════╝ ╚═════╝ ╚═╝  ╚═╝╚══════╝ ╚═════╝ ╚═════╝ ╚═╝  ╚═══╝╚══════╝ ╚═════╝ ╚══════╝╚══════╝");
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
