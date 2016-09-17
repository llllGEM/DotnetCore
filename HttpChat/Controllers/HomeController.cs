using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using HttpChat.Data;
using Microsoft.AspNetCore.Mvc;

namespace HttpChat.Controllers
{
    public class HomeController : Controller
    {
        public static bool OSX { get; set; } = false;

        public static string LocalIp { get; set; }

        private static HttpClient _Client;

        public static HttpClient Client { get { if(_Client == null) _Client = new HttpClient(); return _Client;} }

        public static List<string> ClientsIps {get; set;} = new List<string>(){LocalIp};

        public static List<string> PendingMessages { get; set; } = new List<string>();

        public HomeController()
        {
            CheckEnvironement();
        }

        public IActionResult Index()
        {
            var r = Request;
            return View();
        }
        
        [HttpPostAttribute]
        public async Task<IActionResult> Send(string username, string message)
        {
            var user = new HttpUser{
                Ip = LocalIp,
                Username = username,
                Message = message
            };
            var f = new FormUrlEncodedContent(user.ToDictionary());
            foreach(var clientIp in ClientsIps)
                await Client.PostAsync("http://localhost:5000/Home/Store/", f);
            return Json("Sent");            
        }

        [HttpPostAttribute]
        public void Store(HttpUser infos)
        {
            HttpContext.Session.SetObjectAsJson($"infos.Username", infos);
        }

        public IActionResult Receive()
        {
            
            return Json("");
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }

        public static void CheckEnvironement()
        {
            var SysName = Environment.GetEnvironmentVariable("_system_name");
            OSX = SysName == "OSX";
            LocalIp = OSX ? IPAddress.Parse("192.168.1.11").ToString() : GetLocalIPAsync().ToString();
        }      

        public async static Task<IPAddress> GetLocalIPAsync() 
        {
            IPHostEntry host = await Dns.GetHostEntryAsync(Dns.GetHostName());
            return host
                   .AddressList
                   .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
        }  

    }
}
