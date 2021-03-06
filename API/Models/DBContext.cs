﻿using System;
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
        public static string addUpdatepartner(partner cp)
        {
            try
            {
                using (var db = new smartcheckEntities())
                {
                    if (cp.id == 0)
                    {
                        db.partners.Add(cp);
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

        public static string deletepartner(int cpId)
        {
            try
            {
                using (var db = new smartcheckEntities())
                {
                    var cp = new partner() { id = cpId };
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
        //Company
        public static string addUpdatecompany(company cp)
        {
            try
            {
                using (var db = new smartcheckEntities())
                {
                    if (cp.id == 0)
                    {
                        db.companies.Add(cp);
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
        //Config Bonus
        public static string addUpdateConfig(config_bonus_point cp)
        {
            try
            {
                using (var db = new smartcheckEntities())
                {
                    if (cp.id == 0)
                    {
                        db.config_bonus_point.Add(cp);
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
        public static string deletecompany(int cpId)
        {
            try
            {
                using (var db = new smartcheckEntities())
                {
                    var cp = new company() { id = cpId };
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
        public static string deletevoucher(int cpId)
        {
            try
            {
                using (var db = new smartcheckEntities())
                {
                    var cp = new voucher_points() { id = cpId };
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
        public static string deletesplash(int cpId)
        {
            try
            {
                using (var db = new smartcheckEntities())
                {
                    var cp = new splash() { id = cpId };
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
        public static string deletewinning(int cpId)
        {
            try
            {
                using (var db = new smartcheckEntities())
                {
                    var cp = new winning() { id = cpId };
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
        //Config
        //Company
        public static string addUpdatecompanyConfig(config_app cp)
        {
            try
            {
                using (var db = new smartcheckEntities())
                {
                    if (cp.id == 0)
                    {
                        db.config_app.Add(cp);
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
        //Config
        //Voucher
        public static string addUpdateVoucher(voucher_points cp)
        {
            try
            {
                using (var db = new smartcheckEntities())
                {
                    if (cp.id == 0)
                    {
                        db.voucher_points.Add(cp);
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
        //Config
        //Winning
        public static string addUpdateWinning(winning cp)
        {
            try
            {
                using (var db = new smartcheckEntities())
                {
                    if (cp.id == 0)
                    {
                        db.winnings.Add(cp);
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
        //Config
        //Splash
        public static string addUpdateSplash(splash cp)
        {
            try
            {
                using (var db = new smartcheckEntities())
                {
                    if (cp.id == 0)
                    {
                        db.splashes.Add(cp);
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
        public static string deletecompanyConfig(int cpId)
        {
            try
            {
                using (var db = new smartcheckEntities())
                {
                    var cp = new config_app() { id = cpId };
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