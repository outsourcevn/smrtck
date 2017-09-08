using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QRCoder;
using System.Drawing;
using API.Models;
using PagedList;
using Newtonsoft.Json;
using System.Security.Cryptography;
namespace API.Controllers
{
    
    public class AdminController : Controller
    {
        private smartcheckEntities db = new smartcheckEntities();
        // GET: Admin
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Login(string message)
        {
            ViewBag.message = message;
            return View();
        }
        [HttpPost]
        public ActionResult LogOff()
        {
            Config.removeCookie("user_id");
            Config.removeCookie("user_name");
            Config.removeCookie("user_email");
            return View();
        }
        [HttpPost]
        public ActionResult SubmitLogin(string phone, string pass)
        {
            MD5 md5Hash = MD5.Create();
            pass = Config.GetMd5Hash(md5Hash,pass);
            if (db.companies.Any(o => o.phone == phone && o.pass == pass && o.is_admin==1))
            {
                var us = db.companies.Where(o => o.phone == phone && o.pass == pass).FirstOrDefault();
                Config.setCookie("is_admin", "1");
                Config.setCookie("company_id", us.id.ToString());
                Config.setCookie("company_name", us.name);
                Config.setCookie("company_email", us.email);
                Config.setCookie("company_code", us.code.ToString());
                return RedirectToAction("Company");
            }
            else
            {
                if (db.companies.Any(o => o.phone == phone && o.pass == pass))
                {
                    var us = db.companies.Where(o => o.phone == phone && o.pass == pass).FirstOrDefault();
                    Config.setCookie("is_admin", "0");
                    Config.setCookie("company_id", us.id.ToString());
                    Config.setCookie("company_name", us.name);
                    Config.setCookie("company_email", us.email);
                    Config.setCookie("company_code", us.code.ToString());
                    return RedirectToAction("CompanyQrCode");
                }
                else { 
                    ViewBag.message = "Sai số điện thoại hoặc mật khẩu";
                    return RedirectToAction("Login", new { message = ViewBag.message });
                }
            }
        }
        public ActionResult Customer(string k, int? page)
        {
            if (Config.getCookie("is_admin") == "1") return RedirectToAction("Login", "Admin");
            if (k == null) k = "";
           
                var ctm = db.customers;
                var pageNumber = page ?? 1;
                var onePage = ctm.Where(o => o.phone.Contains(k) || o.email.Contains(k) || o.name.Contains(k)).OrderByDescending(f => f.id).ToPagedList(pageNumber, 20);
                ViewBag.onePage = onePage;
                ViewBag.k = k;
            return View();
            
        }
        public ActionResult Company(string k, int? page)
        {
            if (Config.getCookie("is_admin") == "1") return RedirectToAction("Login", "Admin");
            if (k == null) k = "";
            var ctm = db.companies;
            var pageNumber = page ?? 1;
            var onePage = ctm.Where(o => o.phone.Contains(k) || o.email.Contains(k) || o.name.Contains(k)).OrderByDescending(f => f.id).ToPagedList(pageNumber, 20);
            ViewBag.onePage = onePage;
            ViewBag.k = k;
            return View();
        }
        public ActionResult CompanyQrcodeConfig(string k, int? page)
        {
            if (Config.getCookie("is_admin") == "1") return RedirectToAction("Login", "Admin");
            if (k == null) k = "";
            var ctm = db.config_app;
            var pageNumber = page ?? 1;
            var onePage = ctm.Where(o => o.code_company.ToString().Contains(k) || o.company.Contains(k)).OrderByDescending(f => f.id).ToPagedList(pageNumber, 20);
            ViewBag.onePage = onePage;
            ViewBag.k = k;
            return View();
        }
        public ActionResult Generate()
        {
            return View();
        }
        [HttpPost]
        public string confirmAdmin(long id)
        {
            try { 
                db.Database.ExecuteSqlCommand("update customers set is_admin=1 where id=" + id);
                return "1";
            }
            catch
            {
                return "0";
            }
        }
        [HttpPost]
        public string generateCode(string content)
        {
            try
            {
                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                string code = Guid.NewGuid().ToString();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
                QRCode qrCode = new QRCode(qrCodeData);
                Bitmap qrCodeImage = qrCode.GetGraphic(60);
                Bitmap resized = new Bitmap(qrCodeImage, new Size(76, 76));
                var folder = Server.MapPath(@"/images\");
                resized.Save(folder + "/" + code + ".jpg");
                return "/images/" + code + ".jpg";
            }
            catch
            {
                return "";
            }
        }
        // GET: Admin/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Admin/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Admin/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Admin/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Admin/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Admin/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
        [HttpPost]
        public string addUpdateCustomer(customer cp)
        {
            MD5 md5Hash = MD5.Create();
            cp.pass = Config.GetMd5Hash(md5Hash, cp.pass);
            cp.date_time = DateTime.Now;            
            return DBContext.addUpdatecustomer(cp);
        }

        [HttpPost]
        public string deleteCustomer(int cpId)
        {
            return DBContext.deletecustomer(cpId);
        }
        [HttpPost]
        public string addUpdateCompany(company cp)
        {
            cp.date_time = DateTime.Now;
            return DBContext.addUpdatecompany(cp);
        }

        [HttpPost]
        public string deleteCompany(int cpId)
        {
            return DBContext.deletecompany(cpId);
        }
        [HttpPost]
        public string addUpdateCompanyConfig(config_app cp)
        {            
            return DBContext.addUpdatecompanyConfig(cp);
        }

        [HttpPost]
        public string deleteCompanyConfig(int cpId)
        {
            return DBContext.deletecompanyConfig(cpId);
        }
        [HttpPost]
        public bool checkDuplicateCode(int code)
        {
            return db.companies.Any(o => o.code == code);
        }
        [HttpPost]
        public bool checkDuplicateQrCode(int code)
        {
            return db.config_app.Any(o => o.code_company == code);
        }
        [HttpPost]
        public int? getMaxCompanyCode()
        {
            return db.companies.Max(o => o.code)+1;
        }
        [HttpGet]
        public string GetCompanyQrCodeInfo(int id)
        {
            return JsonConvert.SerializeObject(db.config_app.Where(o=>o.id==id).ToList());
        }
        public string getCompanyList(string k)
        {
            if (k == null) k = "";
            //return JsonConvert.SerializeObject(db.companies.Where(o => o.name.Contains(k)).OrderBy(o => o.name).ToList());
            var p = (from q in db.companies where q.name.Contains(k) select new { value = q.name, id = q.code }).Distinct().ToList();
            return JsonConvert.SerializeObject(p);
        }
    }
}
