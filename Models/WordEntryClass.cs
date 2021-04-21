using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;


//update with
//dotnet ef database update
//migrate with
//dotnet ef migrations add <name>

namespace Synonyms.Models
{
    public class WordEntry
    {
        [Key]
        public int Id { get; set; }

        public string Word { get; set; }        

        public string SynOrAnt { get; set; }
    }

    public class TestClass
    {
        [Key]
        public int Id { get; set; }

        public string test { get; set; }
    }

}
