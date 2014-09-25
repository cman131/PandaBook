using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Collections;
using System.Net;
using System.Web.UI.DataVisualization.Charting;
using PandaBook.Models;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Web.Script.Serialization;
using System.Text;

namespace PandaBook.Controllers
{
    public class StockController : Controller
    {
        private static String[] tokens = new String[] { "uK8GHKPJykvk1xHxzYR9", "uK8GHKPJykvk1xHxzYR9" };
        private static Hashtable curQuotes = null;
        private static String lastUpdate = "";
        private static String[] curCodes = new String[] { };
        private static StockDBContext db = new StockDBContext();

        /**
         * Adds a stock to the database with given values and then reloads the instance of the database
         * 
         * @author Conor Wright
         * @param stock - a stock element with information for the new stock entry in the table
         */
        public static void addStock(Stock stock)
        {
            using (SqlConnection connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["StockDB"].ConnectionString))
            {
                try
                {
                    connection.Open();
                    SqlCommand cmd = new SqlCommand("INSERT INTO Stocks(Name, Code, Source) VALUES (@Name, @Code, @Source)");
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = connection;
                    cmd.Parameters.AddWithValue("@Name", stock.name.Replace('`',' '));
                    cmd.Parameters.AddWithValue("@Code", stock.code.Replace('`', ' '));
                    cmd.Parameters.AddWithValue("@Source", stock.source.Replace('`', ' '));
                    cmd.ExecuteNonQuery();
                    connection.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
            db = new StockDBContext();
        }

        /**
         * Sells given stock in the database and then reloads the instance of the database
         * 
         * @author Conor Wright
         * @param stock - a stock element to sell
         * @param shares - Quantity of stock to sell
         */
        public static String sellStock(int UserId, Stock stock, int shares)
        {
            int rar = -1;
            using (SqlConnection connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["StockDB"].ConnectionString))
            {
                try
                {
                    connection.Open();
                    SqlCommand cmd = new SqlCommand("SELECT SUM(Quantity) AS Totes from Transactions WHERE UserID=@User AND StockID=(SELECT TOP 1 Id FROM Stocks Where Code=@Stock);");
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = connection;
                    cmd.Parameters.AddWithValue("@User", UserId);
                    cmd.Parameters.AddWithValue("@Stock", stock.code.Replace('`', ' '));
                    object dat = cmd.ExecuteScalar();
                    int result = dat != null ? (int)dat : 0;
                    connection.Close();

                    if (result < shares)
                    {
                        connection.Open();
                        cmd = new SqlCommand("DELETE FROM Transactions WHERE UserID=@User AND StockID=(SELECT TOP 1 Id FROM Stocks WHERE Code=@Stock);");
                        cmd.CommandType = CommandType.Text;
                        cmd.Connection = connection;
                        cmd.Parameters.AddWithValue("@User", UserId);
                        cmd.Parameters.AddWithValue("@Stock", stock.code.Replace('`', ' '));
                        result = cmd.ExecuteNonQuery();
                        connection.Close();
                    }
                    else
                    {
                        int x = shares;
                        while (x > 0)
                        {
                            connection.Open();
                            cmd = new SqlCommand("UPDATE Transactions SET Quantity=Quantity-1 FROM Transactions WHERE Id = (SELECT Top 1 Id FROM Transactions Where UserId = @User AND StockId = (SELECT TOP 1 Id FROM Stocks WHERE Stocks.Code=@Stock) ORDER BY Date ASC);");
                            cmd.CommandType = CommandType.Text;
                            cmd.Connection = connection;
                            cmd.Parameters.AddWithValue("@User", UserId);
                            cmd.Parameters.AddWithValue("@Stock", stock.code.Replace('`', ' '));
                            result = cmd.ExecuteNonQuery();
                            connection.Close();
                            connection.Open();
                            cmd = new SqlCommand("DELETE FROM Transactions WHERE Quantity=0;");
                            cmd.CommandType = CommandType.Text;
                            cmd.Connection = connection;
                            result = cmd.ExecuteNonQuery();
                            connection.Close();
                            x = x - 1;
                            Debug.WriteLine(x + " " + (x > 0));
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Panda: " + e.ToString());
                }
            }
            db = new StockDBContext();
            return "Pandas: " + rar;
        }

        /**
         * Buys given stock in the database and then reloads the instance of the database
         * 
         * @author Conor Wright
         * @param stock - a stock element to Buy
         * @param shares - Quantity of stock to buy
         * @param date - When the stock was bought
         */
        public static String buyStock(int UserId, Stock stock, int shares, DateTime date)
        {
            int rar = -1;
            using (SqlConnection connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["StockDB"].ConnectionString))
            {
                try
                {
                    connection.Open();
                    SqlCommand cmd = new SqlCommand("SELECT COUNT(*) AS Totes FROM Transactions WHERE UserID=@User AND StockID=(SELECT TOP 1 Id FROM Stocks WHERE Stocks.Code=@Stock) AND Date=@Date;");
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = connection;
                    cmd.Parameters.AddWithValue("@User", UserId);
                    cmd.Parameters.AddWithValue("@Stock", stock.code.Replace('`', ' '));
                    cmd.Parameters.AddWithValue("@Date", date.ToString("yyyy-MM-dd"));
                    object dat = cmd.ExecuteScalar();
                    int result = dat != null ? (int)dat : 0;
                    connection.Close();

                    if (result == 0)
                    {
                        connection.Open();
                        cmd = new SqlCommand("INSERT INTO Transactions(UserID, StockID, Quantity, Date) VALUES(@User,(SELECT TOP 1 Id FROM Stocks WHERE Code=@Stock),@Quantity,@Date);");
                        cmd.CommandType = CommandType.Text;
                        cmd.Connection = connection;
                        cmd.Parameters.AddWithValue("@User", UserId);
                        cmd.Parameters.AddWithValue("@Stock", stock.code.Replace('`', ' '));
                        cmd.Parameters.AddWithValue("@Quantity", shares);
                        cmd.Parameters.AddWithValue("@Date", date.ToString("yyyy-MM-dd"));
                        result = cmd.ExecuteNonQuery();
                        connection.Close();
                    }
                    else
                    {
                        connection.Open();
                        cmd = new SqlCommand("UPDATE Transactions SET Quantity=@Quantity+Quantity FROM Transactions WHERE UserId = @User AND StockId = (SELECT TOP 1 Id FROM Stocks WHERE Stocks.Code=@Stock) AND Date=@Date;");
                        cmd.CommandType = CommandType.Text;
                        cmd.Connection = connection;
                        cmd.Parameters.AddWithValue("@User", UserId);
                        cmd.Parameters.AddWithValue("@Stock", stock.code.Replace('`', ' '));
                        cmd.Parameters.AddWithValue("@Quantity", shares);
                        cmd.Parameters.AddWithValue("@Date", date.ToString("yyyy-MM-dd"));
                        result = cmd.ExecuteNonQuery();
                        connection.Close();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Panda: "+e.ToString());
                }
            }
            db = new StockDBContext();
            return "Pandas: " + rar;
        }

        /**
         * Checks if current stock is favorited
         * 
         * @author Conor Wright
         * @param stock - a stock element to check favorite status of
         */
        public static Boolean isFavorite(int UserId, Stock stock)
        {
            Boolean ret = false;
            using (SqlConnection connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["StockDB"].ConnectionString))
            {
                try
                {
                    connection.Open();
                    SqlCommand cmd = new SqlCommand("SELECT COUNT(*) AS Totes FROM Notes WHERE UserID=@User AND StockID=(SELECT TOP 1 Id FROM Stocks WHERE Stocks.Code=@Stock);");
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = connection;
                    cmd.Parameters.AddWithValue("@User", UserId);
                    cmd.Parameters.AddWithValue("@Stock", stock.code.Replace('`', ' '));
                    object dat = cmd.ExecuteScalar();
                    int result = dat != null ? (int)dat : 0;
                    connection.Close();

                    if (result != 0)
                    {
                        connection.Open();
                        cmd = new SqlCommand("SELECT Favorite FROM Notes WHERE UserID=@User AND StockID=(SELECT TOP 1 Id FROM Stocks WHERE Stocks.Code=@Stock);");
                        cmd.CommandType = CommandType.Text;
                        cmd.Connection = connection;
                        cmd.Parameters.AddWithValue("@User", UserId);
                        cmd.Parameters.AddWithValue("@Stock", stock.code.Replace('`', ' '));
                        dat = cmd.ExecuteScalar();
                        connection.Close();
                        ret = dat != null ? (Boolean)dat : false;
                        Debug.WriteLine("Counted: "+ret+" "+result+" "+dat.ToString());
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Pandas\n\n\n\n" + e.ToString());
                }
            }
            return ret;
        }

        /**
         * Gets the specified stock's note
         * 
         * @author Conor Wright
         * @param stock - a stock element to get Note of
         */
        public static String getNote(int UserId, Stock stock)
        {
            String ret = "";
            using (SqlConnection connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["StockDB"].ConnectionString))
            {
                try
                {
                    connection.Open();
                    SqlCommand cmd = new SqlCommand("SELECT COUNT(*) AS Totes FROM Notes WHERE UserID=@User AND StockID=(SELECT TOP 1 Id FROM Stocks WHERE Stocks.Code=@Stock);");
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = connection;
                    cmd.Parameters.AddWithValue("@User", UserId);
                    cmd.Parameters.AddWithValue("@Stock", stock.code.Replace('`', ' '));
                    object dat = cmd.ExecuteScalar();
                    int result = dat != null ? (int)dat : 0;
                    connection.Close();

                    if (result != 0)
                    {
                        connection.Open();
                        cmd = new SqlCommand("SELECT Note FROM Notes WHERE UserID=@User AND StockID=(SELECT TOP 1 Id FROM Stocks WHERE Stocks.Code=@Stock);");
                        cmd.CommandType = CommandType.Text;
                        cmd.Connection = connection;
                        cmd.Parameters.AddWithValue("@User", UserId);
                        cmd.Parameters.AddWithValue("@Stock", stock.code.Replace('`', ' '));
                        dat = cmd.ExecuteScalar();
                        connection.Close();
                        ret = dat != null ? dat.ToString() : "";
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Pandas\n\n\n\n" + e.ToString());
                }
            }
            return ret;
        }

        /**
         * Favorites given stock in the database and then reloads the instance of the database
         * 
         * @author Conor Wright
         * @param stock - a stock element to favorite
         * @param isFavorite - whether to set the stock as favorited or not
         */
        public static string favoriteStock(int UserId, Stock stock, Boolean isFavorite)
        {
            string ret = "";
            using (SqlConnection connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["StockDB"].ConnectionString))
            {
                try
                {
                    int fav = isFavorite ? 1 : 0;
                    connection.Open();
                    SqlCommand cmd = new SqlCommand("SELECT COUNT(*) AS Totes FROM Notes WHERE UserID=@User AND StockID=(SELECT TOP 1 Id FROM Stocks WHERE Stocks.Code=@Stock);");
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = connection;
                    cmd.Parameters.AddWithValue("@User", UserId);
                    cmd.Parameters.AddWithValue("@Stock", stock.code.Replace('`', ' '));
                    object dat = cmd.ExecuteScalar();
                    int result = dat != null ? (int)dat : 0;
                    Debug.WriteLine("select: "+result+"="+dat.ToString()+"=");
                    connection.Close();
                    ret = "Select=" + result;

                    if(result==0){
                        connection.Open();
                        cmd = new SqlCommand("INSERT INTO Notes(UserID, StockID, Note, Favorite) VALUES(@User, (SELECT TOP 1 Id FROM Stocks WHERE Stocks.Code=@Stock),  '', @Favor);");
                        cmd.CommandType = CommandType.Text;
                        cmd.Connection = connection;
                        cmd.Parameters.AddWithValue("@User", UserId);
                        cmd.Parameters.AddWithValue("@Stock", stock.code.Replace('`', ' '));
                        cmd.Parameters.AddWithValue("@Favor", fav);
                        result = cmd.ExecuteNonQuery();
                        connection.Close();
                        ret = "Insert=" + result;
                    }
                    else
                    {
                        connection.Open();
                        cmd = new SqlCommand("UPDATE Notes SET Favorite=@Favor FROM Notes WHERE UserID=@User AND StockID=(SELECT TOP 1 Id FROM Stocks WHERE Stocks.Code=@Stock);");
                        cmd.CommandType = CommandType.Text;
                        cmd.Connection = connection;
                        cmd.Parameters.AddWithValue("@User", UserId);
                        cmd.Parameters.AddWithValue("@Stock", stock.code.Replace('`', ' '));
                        cmd.Parameters.AddWithValue("@Favor", fav);
                        result = cmd.ExecuteNonQuery();
                        connection.Close();
                        ret = "Update=" + result;
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Pandas\n\n\n\n"+e.ToString());
                }
            }
            Debug.WriteLine("Pandas\n\n\n\n"+ret+" code="+stock.code+"=");
            db = new StockDBContext();
            return "ret: "+ret;
        }

        [HttpPost]
        public ActionResult Upload(HttpPostedFileBase datafile)
        {
            try
            {
                StreamReader datStream = new StreamReader(datafile.InputStream);
                String file = "";
                String line = "";
                while ((line = datStream.ReadLine()) != null)
                {
                    file += line;
                }
                JavaScriptSerializer json = new JavaScriptSerializer();
                StringBuilder build = new StringBuilder();
                ArrayList listy = json.Deserialize<ArrayList>(build.ToString());
                using (SqlConnection connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["StockDB"].ConnectionString))
                {
                    try
                    {
                        connection.Open();
                        SqlCommand cmd = new SqlCommand("DELETE FROM Transactions WHERE UserID = @User;");
                        cmd.CommandType = CommandType.Text;
                        cmd.Connection = connection;
                        cmd.Parameters.AddWithValue("@User", UserController.getCurrentUser());
                        foreach (Dictionary<String, Object> dic in listy)
                        {
                            cmd = new SqlCommand("INSERT INTO Transactions(Id, UserID, StockID, Quantity, Date) VALUES(@Id, @User, @Stock, @Quantity, @Date)");
                            cmd.Parameters.AddWithValue("@Id", dic["Id"]);
                            cmd.Parameters.AddWithValue("@User", dic["UserID"]);
                            cmd.Parameters.AddWithValue("@Stock", dic["StockID"]);
                            cmd.Parameters.AddWithValue("@Quantity", dic["Quantity"]);
                            cmd.Parameters.AddWithValue("@Date", dic["Date"]);
                            cmd.ExecuteNonQuery();
                        }
                        connection.Close();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
            return View();
        }

        /**
         * Edits the note of given stock in the database and then reloads the instance of the database
         * 
         * @author Conor Wright
         * @param stock - a stock element to edit
         * @param note - the string to set the note as
         */
        public static void SetStockNote(int UserId, Stock stock, String note)
        {
            using (SqlConnection connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["StockDB"].ConnectionString))
            {
                try
                {
                    connection.Open();
                    SqlCommand cmd = new SqlCommand("SELECT Note,Favorite FROM Notes, Stocks, Users WHERE Users.ID = Notes.UserID AND Stocks.ID = Notes.StockID AND Users.UserID = @User AND Stocks.Code=@Stock;");
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = connection;
                    cmd.Parameters.AddWithValue("@User", UserId);
                    cmd.Parameters.AddWithValue("@Stock", stock.code.Replace('`', ' '));
                    int result = cmd.ExecuteNonQuery();
                    connection.Close();

                    if (result == 0)
                    {
                        connection.Open();
                        cmd = new SqlCommand("INSERT INTO Notes(UserID, StockID, Note, Favorite) VALUES(@User, (SELECT TOP 1 Id FROM Stocks WHERE Stocks.Code=@Stock),  '', @note);");
                        cmd.CommandType = CommandType.Text;
                        cmd.Connection = connection;
                        cmd.Parameters.AddWithValue("@User", UserId);
                        cmd.Parameters.AddWithValue("@Stock", stock.code.Replace('`', ' '));
                        cmd.Parameters.AddWithValue("@Note", note.Replace('`', ' '));
                        result = cmd.ExecuteNonQuery();
                        connection.Close();
                    }
                    else
                    {
                        connection.Open();
                        cmd = new SqlCommand("UPDATE Notes SET Note=@Note FROM Notes WHERE UserID = @User AND StockID = (SELECT TOP 1 Id FROM Stocks WHERE Stocks.Code=@Stock);");
                        cmd.CommandType = CommandType.Text;
                        cmd.Connection = connection;
                        cmd.Parameters.AddWithValue("@User", UserId);
                        cmd.Parameters.AddWithValue("@Stock", stock.code.Replace('`', ' '));
                        cmd.Parameters.AddWithValue("@Note", note.Replace('`', ' '));
                        result = cmd.ExecuteNonQuery();
                        connection.Close();
                    }
                    Debug.WriteLine("Notes: "+result);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
            db = new StockDBContext();
        }

        public static String retrieveMyData(int UserId)
        {
            string ret = "";
            using (SqlConnection connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["StockDB"].ConnectionString))
            {
                try
                {
                    connection.Open();
                    SqlCommand cmd = new SqlCommand("SELECT * FROM Transactions WHERE UserID = @User;");
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = connection;
                    cmd.Parameters.AddWithValue("@User", UserId);
                    SqlDataReader result = cmd.ExecuteReader();
                    ArrayList listy = new ArrayList();
                    while(result.Read()){
                        Dictionary<String, Object> dic = new Dictionary<String, Object>();
                        dic["Id"] = result.GetValue(0);
                        dic["UserID"] = result.GetValue(1);
                        dic["StockID"] = result.GetValue(2);
                        dic["Quantity"] = result.GetValue(3);
                        dic["Date"] = result.GetValue(4);
                        listy.Add(dic);
                    }
                    connection.Close();
                    JavaScriptSerializer json = new JavaScriptSerializer();
                    StringBuilder build = new StringBuilder();
                    json.Serialize(listy, build);
                    ret = build.ToString();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
            return ret;
        }

        public static int getSharesCount(int UserId, String code)
        {
            int result = 0;
            try {
                using (SqlConnection connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["StockDB"].ConnectionString))
                {
                    connection.Open();
                    SqlCommand cmd = new SqlCommand("SELECT SUM(Quantity) AS TotalShares FROM Transactions WHERE StockID=(SELECT TOP 1 Id FROM Stocks WHERE Code=@Stock) AND UserID=@User;");
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = connection;
                    cmd.Parameters.AddWithValue("@User", UserId);
                    cmd.Parameters.AddWithValue("@Stock", code.Replace('`', ' '));
                    object dat = cmd.ExecuteScalar();
                    result = dat!=null ? (int)dat : 0;
                    connection.Close();

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            return result;
        }

        /**
         * For ajax calls to favorite a stock
         * @author Conor Wright
         * @param favor - whether to favorite the stock or not
         * @param stock - the code for the stock to favorite
         */
        [HttpPost]
        public ActionResult FavoriteIt(String favor, String stock)
        {
            if (favor != null && stock != null)
            {
                Stock datStock = new Stock("fakeName", stock, "fakeSource");
                ViewBag.success = favoriteStock(UserController.getCurrentUser(), datStock, Boolean.Parse(favor));
            }
            return Json("chamara", JsonRequestBehavior.AllowGet);
        }

        /**
         * For ajax calls to buy a stock
         * @author Conor Wright
         * @param stock - the code for the stock to favorite
         * @param shares - the amount of shares to buy
         */
        [HttpPost]
        public ActionResult BuyIt(string code, string shares)
        {
            if (shares != null && Convert.ToInt32(shares) > 0)
            {
                try
                {
                    buyStock(UserController.getCurrentUser(), new Stock("datStock", code, "GOOG"), Convert.ToInt32(shares), DateTime.Now);
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Panda: " + e.ToString());
                }
            }
            return Json("chamara", JsonRequestBehavior.AllowGet);
        }

        /**
         * For ajax calls to buy a stock
         * @author Conor Wright
         * @param stock - the code for the stock to favorite
         * @param shares - the amount of shares to buy
         */
        [HttpPost]
        public ActionResult NoteIt(string code, string note)
        {
                try
                {
                    SetStockNote(UserController.getCurrentUser(), new Stock("datStock", code, "GOOG"), note);
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Panda: " + e.ToString());
                }
            return Json("chamara", JsonRequestBehavior.AllowGet);
        }

        /**
         * For ajax calls to sell a stock
         * @author Conor Wright
         * @param stock - the code for the stock to favorite
         * @param shares - the amount of shares to sell
         */
        [HttpPost]
        public ActionResult SellIt(string code, string shares)
        {
            if (shares != null && Convert.ToInt32(shares) > 0)
            {
                try
                {
                    sellStock(UserController.getCurrentUser(), new Stock("datStock", code, "GOOG"), Convert.ToInt32(shares));
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Panda: " + e.ToString());
                }
            }
            return Json("chamara", JsonRequestBehavior.AllowGet);
        }

        public ActionResult MyData()
        {
            ViewBag.data = retrieveMyData(UserController.getCurrentUser());
            return View();
        }

        public ActionResult Stock(String code)
        {
            ViewBag.note = getNote(UserController.getCurrentUser(), new Stock("nah", code, "nah"));
            ViewBag.Message = "Stock Time Son";
            ViewBag.code = code;
            ViewBag.listOStocks = generateHtmlStockList(PandaBook.Controllers.UserController.getOwnedStocks());

            ViewBag.shareCount = getSharesCount(UserController.getCurrentUser(), code);
            if (code != "" && code != null)
            {
                Hashtable hash = StockController.getDataSet(code);
                List<string> list = hash.Keys.Cast<string>().ToList();
                ViewBag.data = hash;
                list.Sort();
                list.Reverse();
                ViewBag.keys = list;
                ViewBag.isFavored = isFavorite(UserController.getCurrentUser(), new Stock("Nah", code, "Lies")) ? "rgb(255,0,0)" : "rgb(0,0,255)";
            }
            else
            {
                Hashtable hash = new Hashtable();
                ViewBag.data = hash;
                ViewBag.count = hash.Count;
                ViewBag.data = hash.Keys.Cast<string>().ToList();
            }
            return View();
        }

        public ActionResult StockChart(String code, String date1, String date2)
        {
            ViewBag.Message = "Stock Chart";
            Hashtable hsh = StockController.getDataSet(code);
            ViewBag.data = hsh;
            DateTime dt;
            if (( date1 != null && date1 != "" ) || ( date2 != null && date2 !="" ))
            {
                String[] keys = new String[hsh.Count];
                int count = 0;
                foreach (String key in hsh.Keys)
                {
                    keys[count] = key;
                    count += 1;
                }
                foreach (String key in keys)
                {
                    if (date1 != null && DateTime.TryParseExact(date1, "yyyy-MM-dd", null, DateTimeStyles.None, out dt) && date1.CompareTo(key) > 0)
                    {
                        hsh.Remove(key);
                    }
                    if (date2 != null && DateTime.TryParseExact(date2, "yyyy-MM-dd", null, DateTimeStyles.None, out dt) && date2.CompareTo(key) < 0)
                    {
                        hsh.Remove(key);
                    }
                }
                ViewBag.data = hsh;
            }
            return View();
        }

        public ActionResult StockData()
        {
            return View();
        }

        /**
         * Retrieves the stock codes from Quandl
         * 
         * @author Conor Wright
         * @return Hashtable - A hash table with stock names to codes
         */
        public static Hashtable getStockCodes()
        {
            Hashtable hash = new Hashtable();
            String url = "https://s3.amazonaws.com/quandl-static-content/Ticker+CSV%27s/Stock+Exchanges/Stocks.csv";
            HttpWebRequest req;
            HttpWebResponse resp = null;
            StreamReader file = null;
            try
            {
                req = (HttpWebRequest)WebRequest.Create(url);
                resp = (HttpWebResponse)req.GetResponse();
                String line = "";
                file = new StreamReader(resp.GetResponseStream());
                while ((line = file.ReadLine()) != null)
                {
                    String[] temp = line.Trim().Split(',');
                    if (!temp[0].Contains("Ticker"))
                    {
                        if (!(hash.ContainsKey(temp[2])))
                        {
                            hash.Add(temp[2], temp[0]);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            try
            {
                file.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            Console.WriteLine("Codes: " + hash.Count);
            return hash;
        }

        /**
         * Retrieves a set of data on all stocks
         * 
         * @author Conor Wright
         * 
         * @return Hashtable<String, double> - A mapping of code strings to the double of the most recent quote
         */
        public static Hashtable getCurrentQuotes(String[] codes)
        {
            if (curQuotes != null && DateTime.Now.ToString("yyyy/MM/dd").CompareTo(lastUpdate) <= 0 && curCodes.Equals(codes))
            {
                return curQuotes;
            }
            curCodes = codes;
            curQuotes = new Hashtable();
            String line;
            foreach (String code in codes)
            {
                HttpWebRequest req;
                HttpWebResponse resp = null;
                foreach (String source in new String[] { "GOOG", "YAHOO", "WIKI", "OFDP", "FRED", "DMDRN" })
                {
                    try
                    {
                        req = (HttpWebRequest)WebRequest.Create("http://www.quandl.com/api/v1/datasets/" + source + "/" + code.Trim() + ".csv?auth_token=" + tokens[0] + "&trim_start=" + DateTime.Now.AddDays(-2.00).ToString("yyyy-MM-dd"));
                        resp = (HttpWebResponse)req.GetResponse();
                        break;
                    }
                    catch (WebException e)
                    {
                        Console.WriteLine("\n\n\nPANDA: " + source + "~" + code + e.ToString());
                    }
                }
                if (resp == null)
                {
                    continue;
                }
                StreamReader file = new StreamReader(resp.GetResponseStream());
                while ((line = file.ReadLine()) != null)
                {
                    String[] temp = line.Trim().Split(',');
                    if (!temp[0].Contains("Date"))
                    {
                        try
                        {
                            curQuotes.Add(code, Double.Parse(temp[1]));
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.ToString());
                        }
                        break;
                    }
                }
                file.Close();
            }
            Console.WriteLine("Quotes: " + curQuotes.Count);
            lastUpdate = DateTime.Now.ToString("yyyy/MM/dd");
            return curQuotes;
        }

        /**
         * Retrieves a set of data on a specific stock from a specified source
         * 
         * @author Conor Wright
         * @param source - The source to get the data from (ex: "FRED")
         * @param dataSet - The identifier for the dataset to retrieve (Stock code)
         * 
         * @return Hashtable<String, double> - A mapping of date strings to the double of the quote for that date
         */
        public static Hashtable getDataSet(String dataSet)
        {
            Hashtable hash = new Hashtable();
            String line;
            HttpWebRequest req;
            HttpWebResponse resp = null;
            foreach (String source in new String[] { "GOOG", "YAHOO", "WIKI", "OFDP", "FRED", "DMDRN" })
            {
                try
                {
                    req = (HttpWebRequest)WebRequest.Create("http://www.quandl.com/api/v1/datasets/" + source + "/" + dataSet + ".csv?auth_token="+tokens[0]);
                    resp = (HttpWebResponse)req.GetResponse();
                    break;
                }
                catch (WebException e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
            if (resp == null)
            {
                return new Hashtable();
            }
            StreamReader file = new StreamReader(resp.GetResponseStream());
            try
            {
                while ((line = file.ReadLine()) != null)
                {
                    String[] temp = line.Trim().Split(',');
                    if (!temp[0].Contains("Date"))
                    {
                        try
                        {
                            hash.Add(temp[0], Double.Parse(temp[1]));
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.ToString());
                            throw new Exception("Quandl Data Exception: ~" + temp[0] + "~" + temp[1] + "~");
                        }
                    }

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            file.Close();
            return hash;
        }
        
        /**
         * Generates a string representing a list of stock links based on the given list of codes
         * 
         * @author Conor
         * @param listy - a list of stock codes
         * @return String - the resulting html string
         */
        private String generateHtmlStockList(String[] listy)
        {
            String ret = "";
            foreach (String code in listy)
            {
                ret += "<li class=\"radius\"><a href=\"./Stock?code="+code+"\">"+code+"</a></li>";
            }
            return ret;
        }
    }
}
