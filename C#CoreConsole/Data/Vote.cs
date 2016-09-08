using System.ComponentModel.DataAnnotations;

namespace ConsoleApplication.Data 
{
    public class Vote 
    {
        [Key]
        public int Id {get; set;}

        public int CampusId {get; set;}

        public string Name {get; set;}

        public string Exam {get; set;}

        public int Mark {get; set;}
    }
}