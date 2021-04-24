using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Globalization;
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
            "Antonym",
            "Definition"
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
            ViewBag.TestDef = GetDefinition("quickly");

            return View();
        }

        public static string ToTitle(string s)
        {
            string rtnS = "";
            char[] sChar = s.ToCharArray();
            int i = 0;

            foreach (char c in sChar)
            {
                if (i == 0)
                {
                    rtnS += Convert.ToString(c).ToUpper();
                } else
                {
                    rtnS += Convert.ToString(c).ToLower();
                }

                i++;
            }

            return rtnS;

        }

        [HttpPost]
        [ValidateAntiForgeryToken]//built-in security
        public IActionResult Search(WordEntry obj)
        {
            /*
            _db.WordEntries.Add(obj);
            _db.SaveChanges();

            
            string SWord = obj.Word;
            Console.WriteLine(obj.SynOrAnt);

            

            ViewBag.StartWord = SWord;
            ViewBag.rtnWords = rtnWords;
            */
            string[] rtnWords = new string[] { };
            ViewBag.Other = "False";
            
            if (obj.Word != null)
            {
                if (obj.SynOrAnt == "Synonym")
                {
                    rtnWords = GetSynonyms(obj.Word.Trim());
                }
                else if (obj.SynOrAnt == "Antonym")
                {
                    rtnWords = GetAntonyms(obj.Word.Trim());
                }
                else
                {
                    rtnWords = GetDefinition(obj.Word.Trim());
                    ViewBag.Other = "True";
                }

                ViewBag.SearchWord = ToTitle(obj.Word.Trim());
            } else
            {
                ViewBag.SearchWord = "Please Enter Text";
            }                                  
            
            ViewBag.SearchType = obj.SynOrAnt + "s";

            ViewBag.RtnWords = rtnWords;
            ViewBag.SynList = synList;

            return View("Results");
        }
        
        public IActionResult Results()
        {
            ViewBag.SynList = synList;


            return View();
        }

        [HttpPost]
        public IActionResult Results(DictionaryWord obj)
        {
            ViewBag.DictionaryString = GetDefinition(obj.Word.Trim());            
            ViewBag.SearchWordDict = ToTitle(obj.Word.Trim());
            ViewBag.SynList = synList;
           
            return View("Dictionary");
        }
        public static string[] GetDefinition(string word)
        {
            var client = new RestClient($"https://twinword-word-graph-dictionary.p.rapidapi.com/definition/?entry={word}");
            var request = new RestRequest(Method.GET);
            request.AddHeader("x-rapidapi-key", "40fa536d21msh6eaac19e2c7d2fap140d60jsn5ca9a5f58d95");
            request.AddHeader("x-rapidapi-host", "twinword-word-graph-dictionary.p.rapidapi.com");
            IRestResponse response = client.Execute(request);
            string responseS = response.Content;

            Console.WriteLine($"response: {responseS}");
            List<string> nounsL = GetDefs(responseS);

            Console.WriteLine($"getDefsRtn: [{string.Join("::::", nounsL)}]");
            //responseS = Regex.Replace(responseS, @"[^a-zA-Z :]", "");
            //Console.WriteLine(responseS);

            //string[] list = responseS.Split("nou");

            //Console.WriteLine($"{string.Join(",", list)}");


            return nounsL.ToArray();
        }

        public static List<string> GetDefs(string s)
        {
            List<string> rtnL = new List<string>();

            char[] sAr = s.ToCharArray();
            int sArL = sAr.Length;
            double start = Double.PositiveInfinity;
            string rtnS = "";

            bool nouns = true;
            bool verbs = false;
            bool adverbs = false;
            bool adjectives = false;

            for (int x = 0; x < sArL; x++)
            {
                //nouns
                if (nouns)
                {
                    if (x < sArL - 5)
                    {
                        //checkForNoun
                        if ((sAr[x] == 'n' && sAr[x + 1] == 'o' && sAr[x + 2] == 'u') && (sAr[x + 3] != 'n'))
                        {
                            start = (int)x + 4;
                        }
                    }

                    if (s[x] == '\\')
                    {

                        if (rtnS != "")
                        {
                            rtnL.Add("Noun:");
                            rtnL.Add(rtnS);
                        }

                        rtnS = "";
                        start = Double.PositiveInfinity;
                    }

                    if (x > start)
                    {
                        rtnS += sAr[x];
                    }

                    //check verbs
                    if (sAr[x] == 'v' && sAr[x + 1] == 'e' && sAr[x + 2] == 'r' && sAr[x + 3] == 'b')
                    {
                        nouns = false;
                        verbs = true;
                        adjectives = false;
                        adverbs = false;
                        if (rtnS != "")
                        {
                            rtnS = rtnS.Substring(0, rtnS.Length - 4);
                            rtnL.Add("Noun:");
                            rtnL.Add(rtnS);
                            rtnS = "";
                            start = Double.PositiveInfinity;
                        }

                    }
                    //check adverbs
                    if ((sAr[x] == 'a' && sAr[x + 1] == 'd' && sAr[x + 2] == 'v' && sAr[x + 3] == 'e' && sAr[x + 3] == 'r' && sAr[x + 3] == 'b'))
                    {
                        nouns = false;
                        verbs = false;
                        adjectives = false;
                        adverbs = true;
                        if (rtnS != "")
                        {
                            rtnS = rtnS.Substring(0, rtnS.Length - 4);
                            rtnL.Add("Verb:");
                            rtnL.Add(rtnS);
                            rtnS = "";
                            start = Double.PositiveInfinity;
                        }

                    }
                    //check adjectives
                    if ((sAr[x] == 'a' && sAr[x + 1] == 'd' && sAr[x + 2] == 'j' && sAr[x + 3] == 'e' && sAr[x + 4] == 'c' && sAr[x + 5] == 't'))
                    {
                        nouns = false;
                        verbs = false;
                        adjectives = true;
                        adverbs = false;
                        if (rtnS != "")
                        {
                            rtnS = rtnS.Substring(0, rtnS.Length - 4);
                            rtnL.Add("Adjective:");
                            rtnL.Add(rtnS);
                            rtnS = "";
                            start = Double.PositiveInfinity;
                        }

                    }

                    if (x < sArL - 5)
                    {
                        if ((sAr[x] == 'v' && sAr[x + 1] == 'e' && sAr[x + 2] == 'r' && sAr[x + 3] == 'b'))
                        {
                            nouns = false;
                            verbs = true;
                            if (rtnS != "")
                            {
                                rtnS = rtnS.Substring(0, rtnS.Length - 4);
                                rtnL.Add("Noun:");
                                rtnL.Add(rtnS);
                                rtnS = "";
                                start = Double.PositiveInfinity;
                            }

                        }
                    }
                }

                //verbs
                if (verbs)
                {
                    if (x < sArL - 7)
                    {
                        if ((sAr[x] == 'v' && sAr[x + 1] == 'r' && sAr[x + 2] == 'b'))
                        {
                            start = (int)x + 4;
                        }

                        if (s[x] == '\\')
                        {

                            if (rtnS != "")
                            {
                                rtnL.Add("Verb:");
                                rtnL.Add(rtnS);
                            }

                            rtnS = "";
                            start = Double.PositiveInfinity;
                        }

                        if (x > start)
                        {
                            rtnS += sAr[x];
                        }
                        //check adverb
                        if (verbs && (sAr[x] == 'a' && sAr[x + 1] == 'd' && sAr[x + 2] == 'v' && sAr[x + 3] == 'e' && sAr[x + 4] == 'r' && sAr[x + 5] == 'b'))
                        {
                            if (rtnS != "")
                            {
                                rtnS = rtnS.Substring(0, rtnS.Length - 4);
                                rtnL.Add("Verb");
                                rtnL.Add(rtnS);
                                rtnS = "";
                                start = Double.PositiveInfinity;
                            }

                            nouns = false;
                            verbs = false;
                            adjectives = false;
                            adverbs = true;
                        }

                        //check adjectives
                        if ((sAr[x] == 'a' && sAr[x + 1] == 'd' && sAr[x + 2] == 'j' && sAr[x + 3] == 'e' && sAr[x + 4] == 'c' && sAr[x + 5] == 't'))
                        {
                            nouns = false;
                            verbs = false;
                            adjectives = true;
                            adverbs = false;
                            if (rtnS != "")
                            {
                                rtnS = rtnS.Substring(0, rtnS.Length - 4);
                                rtnL.Add("Adjective:");
                                rtnL.Add(rtnS);
                                rtnS = "";
                                start = Double.PositiveInfinity;
                            }

                        }
                    }
                }

                //adverbs
                if (adverbs)
                {
                    if (x < sArL - 4)
                    {
                        //find beginning of def
                        if (sAr[x] == 'a' && sAr[x + 1] == 'd' && sAr[x + 2] == 'v' && sAr[x + 3] == ')')
                        {
                            start = (int)x + 4;
                        }
                    }                    

                    //know where to end def
                    if (s[x] == '\\')
                    {

                        if (rtnS != "")
                        {
                            rtnL.Add("Adverb:");
                            rtnL.Add(rtnS);
                        }

                        rtnS = "";
                        start = Double.PositiveInfinity;
                    }

                    //build string to add to return list
                    if (x > start)
                    {
                        rtnS += sAr[x];
                    }

                    //check adjectives
                    if ((sAr[x] == 'a' && sAr[x + 1] == 'd' && sAr[x + 2] == 'j' && sAr[x + 3] == 'e' && sAr[x + 4] == 'c' && sAr[x + 5] == 't'))
                    {
                        nouns = false;
                        verbs = false;
                        adjectives = true;
                        adverbs = false;
                        if (rtnS != "")
                        {
                            rtnS = rtnS.Substring(0, rtnS.Length - 4);
                            rtnL.Add("Adjective:");
                            rtnL.Add(rtnS);
                            rtnS = "";
                            start = Double.PositiveInfinity;
                        }

                    }
                }

                //adjectives
                if (adjectives)
                {
                    if (x < sArL - 4)
                    {
                        //find beginning of def
                        if (sAr[x] == 'a' && sAr[x + 1] == 'd' && sAr[x + 2] == 'j' && sAr[x + 3] == ')')
                        {
                            start = (int)x + 4;
                        }
                    }                    

                    //know where to end def
                    if (s[x] == '\\')
                    {

                        if (rtnS != "")
                        {
                            rtnL.Add("Adjective:");
                            rtnL.Add(rtnS);
                        }

                        rtnS = "";
                        start = Double.PositiveInfinity;
                    }

                    //build string to add to return list
                    if (x > start)
                    {
                        rtnS += sAr[x];
                    }

                    //check for break
                    if (sAr[x] == 'i' && sAr[x + 1] == 'p' && sAr[x + 2] == 'a')
                    {
                        if (rtnS != "")
                        {
                            rtnL.Add("Adjective:");
                            rtnL.Add(rtnS.Substring(0, rtnS.Length - 1 - 4));
                        }

                        rtnS = "";
                        start = Double.PositiveInfinity;
                    }
                }
            }

            //check for final string
            if (rtnS != "")
            {
                if (nouns)
                {
                    rtnL.Add("Noun:");
                    rtnL.Add(rtnS);
                }
                if (verbs)
                {
                    rtnL.Add("Verb:");
                    rtnL.Add(rtnS);
                }
                if (adjectives)
                {
                    rtnL.Add("Adjective:");
                    rtnL.Add(rtnS);
                }
                if (adverbs)
                {
                    rtnL.Add("Adverb:");
                    rtnL.Add(rtnS);
                }
            }

            return rtnL;
        }

        [HttpGet]
        public IActionResult Dictionary(WordEntry obj)
        {
            ViewBag.DictionaryString = "";
            ViewBag.SynList = synList;
            if (obj.Word != "")
            {
                ViewBag.DictionaryString = GetDefinition(obj.Word.Trim());
            }

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
