using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RestSharp;
using Synonyms.Models;
using Synonyms.Data;
using System.Text.RegularExpressions;

//handles the user requests and acts as an interface between the models and the views
//Get, Post request
namespace Synonyms.Controllers
{
    public class HomeController : Controller
    {        

        public string[] synList = new string[]
        {
            "Synonym",
            "Antonym"
        };

        public static string[] GetSynonyms(string word)
        {
            var client = new RestClient($"https://languagetools.p.rapidapi.com/synonyms/{word}");
            var request = new RestRequest(Method.GET);
            request.AddHeader("x-rapidapi-key", "40fa536d21msh6eaac19e2c7d2fap140d60jsn5ca9a5f58d95");
            request.AddHeader("x-rapidapi-host", "languagetools.p.rapidapi.com");
            IRestResponse response = client.Execute(request);

            string firstString = Regex.Replace(response.Content, @"[^a-zA-Z ,]", "");

            string[] rtnArray = firstString.Split(",");

            rtnArray[0] = rtnArray[0].Replace("synonyms", "");

            return rtnArray;
        }

        public static string[] GetAntonyms(string word)
        {
            var client = new RestClient($"https://languagetools.p.rapidapi.com/antonyms/{word}");
            var request = new RestRequest(Method.GET);
            request.AddHeader("x-rapidapi-key", "40fa536d21msh6eaac19e2c7d2fap140d60jsn5ca9a5f58d95");
            request.AddHeader("x-rapidapi-host", "languagetools.p.rapidapi.com");
            IRestResponse response = client.Execute(request);

            string firstString = Regex.Replace(response.Content, @"[^a-zA-Z ,]", "");

            string[] rtnArray = firstString.Split(",");

            rtnArray[0] = rtnArray[0].Replace("antonyms", "");

            return rtnArray;
        }

        //private readonly ILogger<HomeController> _logger;

        /*
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }
        */

        private readonly SynonymDbContext _db;

        public HomeController(SynonymDbContext db)
        {            
            _db = db;
        }

        public IActionResult Search()
        {
            ViewBag.SynList = synList;
            ViewBag.RtnWords = new string[] { };

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]//built-in security
        public IActionResult Search(WordEntry obj)
        {
            
            _db.WordEntries.Add(obj);
            _db.SaveChanges();

            /*
            string SWord = obj.Word;
            Console.WriteLine(obj.SynOrAnt);

            

            ViewBag.StartWord = SWord;
            ViewBag.rtnWords = rtnWords;
            */
            string[] rtnWords = new string[] { };

            if (obj.SynOrAnt == "Synonym")
            {
                rtnWords = GetSynonyms(obj.Word);
            } else
            {
                rtnWords = GetAntonyms(obj.Word);
            }            

            ViewBag.SearchWord = obj.SynOrAnt + "s";
            ViewBag.RtnWords = rtnWords;
            ViewBag.SynList = synList;

            return View("Results");
        }

        public IActionResult Results()
        {
            ViewBag.SynList = synList;

            return View();
        }

        public IActionResult Index()
        {
            
            //clearDatabase here

            //may not need to pass in list
            //probably needs to be redirect
            return View();
        }
        
        public IActionResult Privacy()
        {
            return View();
        }       

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
