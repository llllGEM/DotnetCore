using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using ConsoleApplication.Data;

namespace ConsoleApplication.Functions.Randomizer
{
    public static class VoteHandler
    {
        public static List<int> CampusIds = new List<int>();
        private static List<string> Names = new List<string>();
        private static List<int> Marks = new List<int>();
        private static List<Vote> Votes = new List<Vote>();
        private static string FilePath;
        public static bool FromFile = false;
        public static void GetFile()
        {
            Console.Clear();
            FromFile = true;
            C.WL("Copy paste your file (with CampusId + Name separated or not) into console then press Enter...");
            FilePath = C.Read().TrimEnd();
            StreamReader reader;
            try{
                reader = File.OpenText(@FilePath);
                if(reader != null) SetCampusIds(reader);
            }
            catch(Exception e){
                  C.WL(e.Message+"\n Press Enter To continue...");  
                  C.Read();
                  GetFile();
            }
        }

        public static void WriteVotesToFile() 
        {
            if(!SetVotes()){C.WL("No Data Readable");return;}
            var OutputStrings = new string[Votes.Count];
            foreach(var vote in Votes) OutputStrings[Votes.IndexOf(vote)] = vote.Name.PadRight(20)+vote.CampusId+" Exam: "+vote.Exam+" Note: "+vote.Mark;
            var session = new Session{ClassVotes = Votes,Date = DateTime.Now};
            using(var dbcontext = new EFCoreContext()){//sqlite storage for testing
                dbcontext.Sessions.Add(session);
                dbcontext.SaveChanges();
            }
            Program.ProgressBar();
            var newFile = FilePath.Replace(Path.GetFileName(FilePath), $"Session{session.Id}.txt");
            File.WriteAllLines(newFile, OutputStrings);
            C.WL("Done !");
            reset();
        }
        public static void GetMarks(int mark = -1, List<int> marks = null)
        {
            if(mark != -1)Marks.Add(mark);
            else if(marks != null) Marks.AddRange(marks);
        }
        
        public static void SetCampusIds(StreamReader ids)
        {
                string text;
                text = ids.ReadToEnd();
                Regex regexIds = new Regex(@"(\d\d\d\d\d\d)");
                Regex regexNames = new Regex(@"([A-Z]*[a-z]{3,}) {1,}([A-Z]*[a-z]{3,})");
                var tab =  regexIds.Split(text);
                foreach(var str in tab){
                    if(regexIds.Match(str).Success) CampusIds.Add(int.Parse(str));
                    if(regexNames.Match(str).Success) Names.Add(str.Replace(Environment.NewLine,"").Trim());
                }
        }
        
        public static bool SetVotes()
        {
            C.WL("Exam name:");
            var exam = C.Read();
            foreach(var id in CampusIds){
                Vote v = new Vote{ 
                    CampusId = id, 
                    Name = Names[CampusIds.IndexOf(id)],
                    Exam = exam, 
                    Mark = Marks[CampusIds.IndexOf(id)]
                };
                Votes.Add(v);
            }
            return Votes.Count > 0;
        }

        private static void reset()
        {
            CampusIds.Clear();
            Marks.Clear();
            Votes.Clear();
            FilePath = String.Empty;
            FromFile = false;
        }
    }
}
