using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace PandaBook.Models
{
    public class User
    {
        public int ID { get; set; }
        public string first_Name { get; set; }
        public string last_Name { get; set; }
        public String[] favorite_stocks { get; set; }
    }
}