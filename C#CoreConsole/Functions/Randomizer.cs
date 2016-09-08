
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApplication.Functions
{
    public static class Randomizer
    {
        public static void Randomize(CancellationToken ct)
        {
            var t = Task.Factory.StartNew(() => {
                Console.Clear();
                int a, b;
                C.Display("Entrez le premier nombre");
                if (!U.ParseInt(C.Read(), out a)) a = 0;
                C.Display("Entrez le Deuxieme nombre");
                if (!U.ParseInt(C.Read(), out b)) b = 20;
                C.Display("Nombre Random Compris entre [" + a + " et " + b + "] ");
                long i = 1, j;
                C.Display("Données depuis un fichier ? y/n");
                if(C.Key().Key == ConsoleKey.Y) VoteHandler.GetFile(); //if file then
                string iteration = VoteHandler.FromFile ? VoteHandler.CampusIds.Count().ToString() : AskIteration();
                if (U.ParseLong(iteration, out j)){          
                    while (i != j + 1){
                        int r = new Random().Next(a, b + 1);
                        C.Display("Note Random " + i++ + " => " + r);
                        if(VoteHandler.FromFile)VoteHandler.GetMarks(r);                       
                    } 
                    if(VoteHandler.FromFile)VoteHandler.WriteVotesToFile();
                    C.Read();
                }
                else{
                    while(true){
                        C.Display("Note Random " + i++ + " => " + new Random().Next(a, b + 1));
                        if(C.Key().Key == ConsoleKey.Escape) break;// echap to escape
                    } 
                }
            }, ct);
            t.Wait();
            if(t.IsCompleted){
                Console.Clear();
                C.Display("Task finished");
            }
            C.Display("Relaunch? y/n");
            if(C.Key().Key == ConsoleKey.Y){
                Console.Clear();
                Randomizer.Randomize(ct); 
                return;
            }
        }

        private static string AskIteration()
        {
            Console.Clear();
            C.Display("Nombre d'iteration :");
            return C.Read();
        }
    }

}