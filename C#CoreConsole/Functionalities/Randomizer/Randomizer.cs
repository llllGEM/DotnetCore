
using System;
using System.Linq;

namespace ConsoleApplication.Functions.Randomizer
{
    public static class Randomizer
    {
        public static void Randomize()
        {
            Console.Clear();
            int a, b;
            C.WL("Entrez le premier nombre");
            if (!U.ParseInt(C.Read(), out a)) a = 0;
            C.WL("Entrez le Deuxieme nombre");
            if (!U.ParseInt(C.Read(), out b)) b = 20;
            C.WL("Nombre Random Compris entre [" + a + " et " + b + "] ");
            long i = 1, j;
            C.WL("Données depuis un fichier ? y/n");
            if(C.Key().Key == ConsoleKey.Y) VoteHandler.GetFile(); //if file then
            string iteration = VoteHandler.FromFile ? VoteHandler.CampusIds.Count().ToString() : AskIteration();
            if (U.ParseLong(iteration, out j)){          
                while (i != j + 1){
                    int r = new Random().Next(a, b + 1);
                    C.WL("Note Random " + i++ + " => " + r);
                    if(VoteHandler.FromFile)VoteHandler.GetMarks(r);                       
                } 
                if(VoteHandler.FromFile)VoteHandler.WriteVotesToFile();
                C.Read();
            }
            else{
                while(true){
                    C.WL("Note Random " + i++ + " => " + new Random().Next(a, b + 1));
                    if(C.Key().Key == ConsoleKey.Escape) break;// echap to escape
                } 
            }
            C.WL("Relaunch? y/n");
            if(C.Key().Key == ConsoleKey.Y){
                Console.Clear();
                Randomizer.Randomize(); 
                return;
            }
        }

        private static string AskIteration()
        {
            Console.Clear();
            C.WL("Nombre d'iteration :");
            return C.Read();
        }
    }

}