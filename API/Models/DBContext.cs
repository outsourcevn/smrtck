using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace API.Models
{
    public class DBContext
    {
        public static string addUpdatecustomer(customer cp)
        {
            try
            {
                using (var db = new smartcheckEntities())
                {
                    if (cp.id == 0)
                    {
                        db.customers.Add(cp);
                    }
                    else
                    {
                        db.Entry(cp).State = EntityState.Modified;
                    }
                    db.SaveChanges();
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                return "Thất bại: " + ex.Message;
            }
        }

        public static string deletecustomer(int cpId)
        {
            try
            {
                using (var db = new smartcheckEntities())
                {
                    var cp = new customer() { id = cpId };
                    db.Entry(cp).State = EntityState.Deleted;
                    db.SaveChanges();
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                return "Thất bại: " + ex.Message;
            }
        }
    }
}