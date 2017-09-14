using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace API
{
    public class Config
    {
        public static string GetMd5Hash(MD5 md5Hash, string input)
        {

            // Convert the input string to a byte array and compute the hash. 
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes 
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data  
            // and format each one as a hexadecimal string. 
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string. 
            return sBuilder.ToString();
        }
        public static void setCookie(string field, string value)
        {
            HttpCookie MyCookie = new HttpCookie(field);
            MyCookie.Value = value;
            MyCookie.Expires = DateTime.Now.AddDays(365);
            HttpContext.Current.Response.Cookies.Add(MyCookie);
            //Response.Cookies.Add(MyCookie);           
        }
        public static string getCookie(string v)
        {
            try
            {
                return HttpContext.Current.Request.Cookies[v].Value.ToString();
            }
            catch (Exception ex)
            {
                return "";
            }
        }
        public static void removeCookie(string field)
        {
            HttpCookie MyCookie = new HttpCookie(field);
            MyCookie.Value = "1";
            MyCookie.Expires = DateTime.Now.AddDays(-1);
            HttpContext.Current.Response.Cookies.Add(MyCookie);
            //Response.Cookies.Add(MyCookie);           
        }
        public static string formatDateTime(DateTime? val)
        {
            try
            {
                return String.Format("{0:dd/MM/yyyy hh:mm tt}", val);
            }
            catch
            {
                return val.ToString();
            }
        }
        public static string getProvince(string val)
        {
            if (val == null || val.Trim() == "") return val;
            try
            {
                string[] pr = val.Split(',');
                return pr[pr.Length - 2].Trim();

            }
            catch
            {
                return "";
            }
        }
        public static int datetimeid()
        {
            DateTime d1;
            try
            {

                d1 = DateTime.Now;//.ToUniversalTime();
                string rs = d1.Year.ToString() + d1.Month.ToString("00") + d1.Day.ToString("00");
                return int.Parse(rs);

            }
            catch (Exception ex)
            {
                d1 = DateTime.Now;//.ToUniversalTime();
                string rs = d1.Year.ToString() + d1.Month.ToString("00") + d1.Day.ToString("00");
                return int.Parse(rs);
            }
        }
    }
}