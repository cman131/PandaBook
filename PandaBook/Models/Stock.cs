using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace PandaBook.Models
{
    public class Stock
    {
        public Stock(String name, String code, String source)
        {
            this.name = name;
            this.code = code;
            this.source = source;
        }
        public int StockId { get; set; }
        public string source { get; set; }
        public string code { get; set; }
        public string name { get; set; }
    }

    public class StockDBContext : DbContext
    {
        public DbSet<Stock> Stocks { get; set; }
    }
}