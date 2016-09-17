using System.Collections.Generic;

namespace HttpChat.Data 
{
    public class HttpUser
    {
        public string Ip {get; set;}

        public string Username {get; set ;}

        public string Message {get; set;}

        public Dictionary<string,string> ToDictionary()
       {
           return new Dictionary<string,string>{
                {"Ip", this.Ip},            
                {"Username", this.Username},            
                {"Message", this.Message},            
           };
       } 
    }
}