using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ConsoleApplication.Data
{
    public class Session 
    {
        [Key]
        public int Id {get; set;}
        public virtual List<Vote> ClassVotes {get; set;}

        public DateTime Date {get; set;}
    }
}