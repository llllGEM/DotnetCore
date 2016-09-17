﻿using System;
using System.Threading;
using System.Threading.Tasks;
using ConsoleApplication.Functions.Randomizer;
using ConsoleApplication.Functions.Chat;
using System.Diagnostics;
using System.IO;

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
                C.WL("Choose program:\n\n 1: SocketChat \n\n 2: HttpChat \n\n 3: Randomizer");   
                if(!U.ParseInt(C.Read(), out nb)) {await MainAsync(args);EndTasks();}
                else
                switch(nb){
                        case 1: SocketChat.Begin(cts.Token);
                                break;
                        case 2: LauncHttpChatServer();
                                break;
                        case 3: Randomizer.Randomize();
                                break;
                        default: SocketChat.Begin(cts.Token);
                                 Console.Clear();
                                 break;
                }
            }
            catch(Exception){}
            finally{EndTasks();}
        }

        public static void LauncHttpChatServer()
        {
            ProcessStartInfo psi = new ProcessStartInfo("dotnet");
            psi.WorkingDirectory = "../Chat";
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
                //string bar = "Arretez de regarder le titre bande de cons";
                //string[] icon = new string[4]{"| ", "/ ", "--", "\\ "};
                //string[] dots = new string[4]{".   ", "..  ", "... ", "...."};
                string[] node = new string[10]{"⡇","⠇","⠏","⠛","⠹","⠸","⢸","⣰","⣤","⣆"};
                string title = "";
                while (true){
                    for(int i = 0; i< node.Length; i++){
                            //title += bar[i];
                            //title = icon[i].ToString();;
                            title = "Running "+node[i];
                            Console.Title = title;
                            Thread.Sleep(50);
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
            try{ cts?.Cancel(false); }//cancel all tasks without ending process
            catch(Exception){}
            finally{ cts?.Dispose(); }
            Console.ResetColor();Console.Clear();
        }
    }
}
