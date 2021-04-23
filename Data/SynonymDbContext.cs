using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Synonyms.Models;

namespace Synonyms.Data
{
    public class SynonymDbContext : DbContext
    {
        //this is all configuration syntax that must be followed in order to use the DbContextOptions class
        //which i guess was inherited from the EntityFrameworkCore library
        public SynonymDbContext(DbContextOptions<SynonymDbContext> options): base(options)
        {

        }

        //this is how you can make migrations to the database (final step)
        public DbSet<WordEntry> WordEntries { get; set; }
        public DbSet<DictionaryWord> DictionaryWords { get; set; }

    }
}
