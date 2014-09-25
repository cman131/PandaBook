using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace PandaBook.Models
{
    public class Calendar
    {
        public int ID { get; set; }
        public string Title { get; set; }
    }

    public class CalendarDBContext : DbContext
    {
        public DbSet<Calendar> Calendars { get; set; }
    }
}