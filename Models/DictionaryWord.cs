using System;
using System.ComponentModel.DataAnnotations;

namespace Synonyms.Models
{
    public class DictionaryWord
    {
        [Key]
        public int Id { get; set; }

        public string Word { get; set; }
    }
}
