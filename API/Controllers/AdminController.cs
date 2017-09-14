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
using System.Text;
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
            Config.removeCookie("is_admin");
            Config.removeCookie("company_id");
            Config.removeCookie("company_name");
            Config.removeCookie("company_email");
            Config.removeCookie("company_code");
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
                    return RedirectToAction("CheckAll");
                }
                else { 
                    ViewBag.message = "Sai số điện thoại hoặc mật khẩu";
                    return RedirectToAction("Login", new { message = ViewBag.message });
                }
            }
        }
        public ActionResult Customer(string k, int? page)
        {
            if (Config.getCookie("is_admin") != "1") return RedirectToAction("Login", "Admin", new { message = "Bạn không được cấp quyền truy cập chức năng này" });
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
            if (Config.getCookie("is_admin") != "1") return RedirectToAction("Login", "Admin", new { message = "Bạn không được cấp quyền truy cập chức năng này" });
            if (k == null) k = "";
            var ctm = db.companies;
            var pageNumber = page ?? 1;
            var onePage = ctm.Where(o => o.phone.Contains(k) || o.email.Contains(k) || o.name.Contains(k)).OrderByDescending(f => f.id).ToPagedList(pageNumber, 20);
            ViewBag.onePage = onePage;
            ViewBag.k = k;
            return View();
        }
        public ActionResult Partner(string k,int? code_company, int? page)
        {
            if (Config.getCookie("is_admin") != "1") return RedirectToAction("Login", "Admin", new { message = "Bạn không được cấp quyền truy cập chức năng này" });
            if (Config.getCookie("company_code") != "" && Config.getCookie("is_admin") != "1")
            {
                code_company = int.Parse(Config.getCookie("company_code"));
            }
            if (k == null) k = "";
            var ctm = db.partners;
            var pageNumber = page ?? 1;
            var onePage = ctm.Where(o => o.company.Contains(k) || o.name.Contains(k)).OrderByDescending(f => f.id).ToPagedList(pageNumber, 20);
            ViewBag.onePage = onePage;
            ViewBag.k = k;
            ViewBag.company = Config.getCookie("company_name");
            ViewBag.code_company = Config.getCookie("company_code");
            return View();
        }
        public ActionResult CompanyQrcodeConfig(string k, int? page)
        {
            if (Config.getCookie("is_admin") != "1") return RedirectToAction("Login", "Admin", new { message = "Bạn không được cấp quyền truy cập chức năng này" });
            if (k == null) k = "";
            var ctm = db.config_app;
            var pageNumber = page ?? 1;
            var onePage = ctm.Where(o => o.code_company.ToString().Contains(k) || o.company.Contains(k)).OrderByDescending(f => f.id).ToPagedList(pageNumber, 20);
            ViewBag.onePage = onePage;
            ViewBag.k = k;
            return View();
        }
        public ActionResult CompanyQrCode(int? code_company,string company,string partner,int? id_partner,long? ffrom,long? tto, int? page)
        {
            if (Config.getCookie("is_admin") != "1") return RedirectToAction("Login", "Admin", new { message = "Bạn không được cấp quyền truy cập chức năng này" });
           
            var ctm = db.qrcodes;
            var pageNumber = page ?? 1;            
            var onePage = ctm.OrderByDescending(f => f.id).ToPagedList(pageNumber, 20);
            if (code_company != null)
            {
                onePage = ctm.Where(o => o.code_company == code_company && o.id_partner == id_partner && o.stt >= ffrom && o.stt <= tto).OrderByDescending(f => f.id).ToPagedList(pageNumber, 20);
            }
            ViewBag.onePage = onePage;
            ViewBag.code_company = code_company;
            ViewBag.company = company;
            ViewBag.partner = partner;
            ViewBag.id_partner = id_partner;
            ViewBag.ffrom = ffrom;
            ViewBag.tto = tto;
            var nt= db.qrcode_log.OrderByDescending(o=>o.id).FirstOrDefault();
            ViewBag.notice = nt!=null?nt.actions:"";
            return View();
        }
        public ActionResult CheckAll(int? code_company, string company, string partner, int? id_partner, DateTime? fdate, DateTime? tdate, string provin,int? type, int? page)
        {
            if (Config.getCookie("is_admin") == "") return RedirectToAction("Login", "Admin", new { message = "Bạn không được cấp quyền truy cập chức năng này" });
            if (Config.getCookie("is_admin") != "1") { 
                code_company = int.Parse(Config.getCookie("company_code"));
            }
            if (provin == null) provin = "";
            var ctm = db.checkalls;
            var pageNumber = page ?? 1;
            var onePage = ctm.OrderByDescending(f => f.id).ToPagedList(pageNumber, 20);
            if (id_partner != null)
            {
                onePage = ctm.Where(o => o.code_company == code_company && o.id_partner == id_partner && o.date_time >= fdate && o.date_time <= tdate && o.province.Contains(provin)).OrderByDescending(f => f.id).ToPagedList(pageNumber, 20);
                
            }
            ViewBag.onePage = onePage;
            ViewBag.code_company = code_company;            
            ViewBag.id_partner = id_partner;
            ViewBag.company = company;
            ViewBag.partner = partner;
            ViewBag.fdate = fdate;
            ViewBag.tdate = tdate;
            ViewBag.countall = onePage.Count;
            ViewBag.is_admin = Config.getCookie("is_admin");
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
        public string confirmAdminCompany(long id,int val)
        {
            try
            {
                db.Database.ExecuteSqlCommand("update company set is_admin=" + val + " where id=" + id);
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
                string guid = Guid.NewGuid().ToString(); 
                //string code = content;// Guid.NewGuid().ToString();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
                QRCode qrCode = new QRCode(qrCodeData);
                Bitmap qrCodeImage = qrCode.GetGraphic(60);
                Bitmap resized = new Bitmap(qrCodeImage, new Size(330, 330));
                var folder = Server.MapPath(@"/images\");
                resized.Save(folder + "/" + guid + ".jpg");
                return "/images/" + guid + ".jpg";
            }
            catch
            {
                return "";
            }
        }
        [HttpPost]
        public string UndoQrCode(string guid)
        {
            try
            {
                db.Database.ExecuteSqlCommand("delete from checkall where guid=N'"+guid+"'");
                return "1";
            }
            catch
            {
                return "0";
            }
        }
        [HttpPost]
        public string generateQrCodeCompany(int code_company,string company,string partner,int id_partner,long ffrom,long tto)
        {
            try
            {
                if (db.qrcodes.Any(o => o.code_company == code_company && o.id_partner == id_partner && o.stt >= ffrom && o.stt <= tto))
                {
                    long? maxstt=db.qrcodes.Where(o => o.code_company == code_company && o.id_partner == id_partner).Max(o=>o.stt);
                    return "Đã tồn tại khoảng thứ tự này, đề nghị chọn khoảng in khác, đã in đến số thứ tự " + maxstt;
                }
                string info = db.config_app.Where(o => o.id==1).OrderBy(o => o.id).FirstOrDefault().text_in_qr_code;
                if (db.config_app.Any(o => o.code_company == code_company))
                {

                    info = db.config_app.Where(o => o.code_company == code_company).OrderBy(o => o.id).FirstOrDefault().text_in_qr_code;
                }
                
                DateTime fromtime = DateTime.Now;
                for (long i = ffrom; i <= tto; i++)
                {
                    string guid=Guid.NewGuid().ToString();                    
                    qrcode qr = new qrcode();
                    qr.code_company = code_company;
                    qr.company = company;
                    qr.date_id = Config.datetimeid();
                    qr.date_time = DateTime.Now;
                    qr.guid = guid;
                    qr.id_partner = id_partner;
                    qr.partner = partner;
                    qr.qrcode2 = guid;// info.Replace("{GUID}", guid);
                    qr.status = 0;
                    qr.stt = i;
                    db.qrcodes.Add(qr);
                    db.SaveChanges();
                }
                qrcode_log ql = new qrcode_log();
                int totalminutes = (int)(DateTime.Now - fromtime).TotalMinutes;
                string notice = "Đã in qr code cho công ty " + company + ", nhà phân phối " + partner + ", từ số thứ tự " + ffrom + " đến số thứ tự " + tto + ",hoàn thành lúc " + DateTime.Now + ", hết " + totalminutes + " phút";
                ql.actions = notice;
                db.qrcode_log.Add(ql);
                db.SaveChanges();
                return notice;
            }
            catch(Exception ex)
            {
                return "Có lỗi xảy ra khi in "+ex.ToString();
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
        public string addUpdatePartner(partner cp)
        {
            cp.date_time = DateTime.Now;
            return DBContext.addUpdatepartner(cp);
        }

        [HttpPost]
        public string deletePartner(int cpId)
        {
            return DBContext.deletepartner(cpId);
        }
        [HttpPost]
        public string addUpdateCompany(company cp)
        {
            MD5 md5Hash = MD5.Create();
            cp.pass = Config.GetMd5Hash(md5Hash, cp.pass);
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
        public string getPartnerList(string k,int code_company)
        {
            if (k == null) k = "";
            //return JsonConvert.SerializeObject(db.companies.Where(o => o.name.Contains(k)).OrderBy(o => o.name).ToList());
            var p = (from q in db.partners where q.name.Contains(k) && q.code_company==code_company select new { value = q.name, id = q.id }).Distinct().ToList();
            return JsonConvert.SerializeObject(p);
        }
        public void exportCompanyQrCode(int code_company,string company,string partner,int id_partner,long ffrom,long tto)
        {
            
            try
            {

                var rs = db.qrcodes.Where(o => o.code_company == code_company && o.id_partner == id_partner && o.stt >= ffrom && o.stt <= tto).ToList();
                StringBuilder sb = new StringBuilder();
                //sb.Append("sn,qrcode\r\n");
                for (int i = 0; i < rs.Count;i++)
                {
                    sb.Append("\""+rs[i].guid+"\",\""+rs[i].stt+"\"\r\n");
                }
                Response.ClearContent();
                Response.ClearHeaders();
                Response.BufferOutput = true;
                //Response.ContentType = "text/plain";
                //Response.AddHeader("Content-Disposition", "attachment; filename=" + code_company + "_" + id_partner + "_"+ffrom+"_"+tto+".txt");
                Response.ContentType = "application/vnd.ms-excel";
                Response.AddHeader("Content-Disposition", "attachment; filename=" + code_company + "_" + id_partner + "_"+ffrom+"_"+tto+".csv");
                Response.Write(sb.ToString());
                //Response.Write(htmlContent.ToString());
                Response.Flush();
                Response.Close();
                Response.End();

            }
            catch (Exception exmain)
            {
                return;
            }

        }
    }
}
