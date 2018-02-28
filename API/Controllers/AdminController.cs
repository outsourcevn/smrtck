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
using System.IO;
using System.Configuration;
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
            Config.removeCookie("company_phone");
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
                Config.setCookie("company_phone", us.phone);
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
                    Config.setCookie("company_phone", us.phone);
                    Config.setCookie("company_code", us.code.ToString());
                    return RedirectToAction("CheckAll");
                }
                else { 
                    ViewBag.message = "Sai số điện thoại hoặc mật khẩu";
                    return RedirectToAction("Login", new { message = ViewBag.message });
                }
            }
        }
        [HttpPost]
        public ActionResult ResetEmail(string email)
        {
            try
            {
                if (db.customers.Any(o=>o.email==email)){
                    string mailuser = ConfigurationManager.AppSettings["mailuser"];
                    string mailpass = ConfigurationManager.AppSettings["mailpass"];//localhost:59340
                    string link = "http://api.smartcheck.vn/home/ConfirmReset2?email=" + email + "&code=" + db.customers.Where(o => o.email == email).OrderBy(o => o.id).FirstOrDefault().pass;//api.smartcheck.vn
                    if (Config.Sendmail(mailuser, mailpass, email, "Lấy lại mật khẩu của ứng dụng SmartCheck.Vn", "Ai đó đã dùng email này để yêu cầu lấy lại mật khấu, nếu là bạn xin xác nhận click vào đường link này để nhập lại mật khẩu " + link + ", nếu không phải là bạn xin bỏ qua email này.<br>http://smartcheck.vn"))
                    {
                        return RedirectToAction("ConfirmReset1", "Home", new { message = "Chúng tôi đã gửi mail đến địa chỉ email bạn cung cấp, vui lòng click vào link trong mail để đặt lại mật khẩu." });
                    }
                }else{
                    return RedirectToAction("Reset","Home", new { message = "Không tìm thấy email này trong dữ liệu, vui lòng điền email khác mà bạn dùng để đăng ký" });
                }
            }
            catch
            {
                    return RedirectToAction("Reset","Home", new { message = "Không tìm thấy email này trong dữ liệu, vui lòng điền email khác mà bạn dùng để đăng ký" });
            }
            return View();
        }
        [HttpPost]
        public ActionResult ResetEmailCompany(string email)
        {
            try
            {
                if (db.companies.Any(o => o.email == email))
                {
                    string mailuser = ConfigurationManager.AppSettings["mailuser"];
                    string mailpass = ConfigurationManager.AppSettings["mailpass"];//localhost:59340
                    string link = "http://api.smartcheck.vn/Admin/ConfirmReset2?email=" + email + "&code=" + db.companies.Where(o => o.email == email).OrderBy(o => o.id).FirstOrDefault().pass;//api.smartcheck.vn
                    if (Config.Sendmail(mailuser, mailpass, email, "Đổi hoặc Lấy lại mật khẩu của ứng dụng SmartCheck.Vn", "Ai đó đã dùng email này để yêu cầu lấy lại mật khấu, nếu là bạn xin xác nhận click vào đường link này để nhập lại mật khẩu " + link + ", nếu không phải là bạn xin bỏ qua email này.<br>http://smartcheck.vn"))
                    {
                        return RedirectToAction("ConfirmReset1", "Admin", new { message = "Chúng tôi đã gửi mail đến địa chỉ email bạn cung cấp, vui lòng click vào link trong mail để đặt lại mật khẩu." });
                    }
                }
                else
                {
                    return RedirectToAction("Reset", "Admin", new { message = "Không tìm thấy email này trong dữ liệu, vui lòng điền email khác mà bạn dùng để đăng ký" });
                }
            }
            catch
            {
                return RedirectToAction("Reset", "Admin", new { message = "Không tìm thấy email này trong dữ liệu, vui lòng điền email khác mà bạn dùng để đăng ký" });
            }
            return View();
        }
        public ActionResult ConfirmReset1(string message)
        {
            ViewBag.Message = message;

            return View();
        }
        public ActionResult ConfirmReset2(string email, string code)
        {

            if (!db.companies.Any(o => o.email == email && o.pass == code))
            {
                return RedirectToAction("Reset", "Admin", new { message = "Không tìm thấy email này trong dữ liệu, vui lòng điền email khác mà bạn dùng để đăng ký" });
            }
            ViewBag.code = code;
            ViewBag.email = email;
            return View();
        }
        [HttpPost]
        public ActionResult ConfirmReset3(string email, string code, string pass, string pass2)
        {
            if (!db.companies.Any(o => o.email == email && o.pass == code))
            {
                return RedirectToAction("Reset", "Admin", new { message = "Không tìm thấy email này trong dữ liệu, vui lòng điền email khác mà bạn dùng để đăng ký" });
            }
            MD5 md5Hash = MD5.Create();
            string hash = Config.GetMd5Hash(md5Hash, pass);
            db.Database.ExecuteSqlCommand("update company set pass=N'" + hash + "' where email=N'" + email + "' and pass=N'" + code + "'");
            return RedirectToAction("ConfirmReset4", "Admin", new { message = "Đổi mật khẩu thành công, xin dùng số điện thoại bạn đã đăng ký đăng nhập cùng mật khẩu mới này" });
        }
        public ActionResult ConfirmReset4(string message)
        {
            ViewBag.Message = message;
            return View();
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
        public ActionResult VoucherPoint(string k, int? page)
        {
            if (Config.getCookie("is_admin") != "1") return RedirectToAction("Login", "Admin", new { message = "Bạn không được cấp quyền truy cập chức năng này" });
            if (k == null) k = "";
            var ctm = db.voucher_points;
            var pageNumber = page ?? 1;
            var onePage = ctm.Where(o => o.name.Contains(k) || o.des.Contains(k)).OrderByDescending(f => f.id).ToPagedList(pageNumber, 20);
            ViewBag.onePage = onePage;
            ViewBag.k = k;
            return View();
        }
        public class ReportTotalItem1
        {
            
            public int code_company { get; set; }
            public string company { get; set; }
            public int schecked { get; set; }

        }
        public class ReportTotalItem2
        {
            
            public long user_id { get; set; }
            public string user_name { get; set; }
            public string user_email { get; set; }
            public string user_phone { get; set; }
            public int? schecked { get; set; }

        }
        public ActionResult ReportTotal(string k, int? page)
        {
            int code_company=0;
            string company = "";
            if (Config.getCookie("is_admin") == "") return RedirectToAction("Login", "Admin", new { message = "Bạn không được cấp quyền truy cập chức năng này" });
            //if (Config.getCookie("is_admin") != "1")
            //{
                code_company = int.Parse(Config.getCookie("company_code"));
                company = Config.getCookie("company_name");
            //}
            string query = "select user_id,user_name,user_email,user_phone,count(*) as schecked from checkall where user_email is not null and code_company=" + code_company + "  group by user_id,user_name,user_email,user_phone order by schecked desc";
            var p = db.Database.SqlQuery<ReportTotalItem2>(query);
            int pageSize = 10;
            int pageNumber = (page ?? 1);
            return View(p.ToPagedList(pageNumber, pageSize));
        }
        public string getReportTotal()
        {
            int? code_company = int.Parse(Config.getCookie("company_code"));
            string query = "select code_company,company,count(*) as schecked from checkall where code_company=" + code_company + "  group by code_company,company union select 0 as code_company,'A' as company, max(to_stt) as schecked from qrcode where code_company=" + code_company + "  group by code_company,company ";
            var p = db.Database.SqlQuery<ReportTotalItem1>(query).ToList();
            return "Công ty của bạn đã có số lượt khách quét là " + p[0].schecked + " trên tổng số " + p[1].schecked+" mã qr code";
        }
        public ActionResult ConfigPoint(int? page)
        {
            if (Config.getCookie("is_admin") != "1") return RedirectToAction("Login", "Admin", new { message = "Bạn không được cấp quyền truy cập chức năng này" });           
            var ctm = db.config_bonus_point;
            var pageNumber = page ?? 1;
            var onePage = ctm.OrderByDescending(f => f.id).ToPagedList(pageNumber, 20);
            ViewBag.onePage = onePage;
            return View();
        }
        public ActionResult Winning(string k, int? page)
        {
            if (Config.getCookie("is_admin") != "1") return RedirectToAction("Login", "Admin", new { message = "Bạn không được cấp quyền truy cập chức năng này" });
            if (k == null) k = "";
            var ctm = db.winnings;
            var pageNumber = page ?? 1;
            var onePage = ctm.Where(o => o.name.Contains(k)).OrderByDescending(f => f.id).ToPagedList(pageNumber, 20);
            ViewBag.onePage = onePage;
            ViewBag.k = k;
            return View();
        }
        public ActionResult Partner(string k,int? code_company, int? page)
        {
            if (Config.getCookie("is_admin") == "") return RedirectToAction("Login", "Admin", new { message = "Bạn không được cấp quyền truy cập chức năng này" });
            if (Config.getCookie("company_code") != "" && Config.getCookie("is_admin") != "1")
            {
                code_company = int.Parse(Config.getCookie("company_code"));
            }
            if (k == null) k = "";
            var ctm = db.partners;
            var pageNumber = page ?? 1;

            if (Config.getCookie("is_admin") == "1")
            {
                var onePage = ctm.Where(o => o.company.Contains(k) || o.name.Contains(k)).OrderByDescending(f => f.id).ToPagedList(pageNumber, 20);
                ViewBag.isadmin = "1";
                ViewBag.onePage = onePage;
            }
            else
            {
                var onePage = ctm.Where(o => (o.company.Contains(k) || o.name.Contains(k)) && o.code_company == code_company).OrderByDescending(f => f.id).ToPagedList(pageNumber, 20);
                ViewBag.isadmin = "0";
                ViewBag.onePage = onePage;
            }
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
                onePage = ctm.Where(o => o.code_company == code_company && o.id_partner == id_partner && o.from_stt >= ffrom && o.to_stt <= tto).OrderByDescending(f => f.id).ToPagedList(pageNumber, 20);
            }
            ViewBag.onePage = onePage;
            ViewBag.PageCount = onePage.PageCount;
            ViewBag.code_company = code_company;
            ViewBag.company = company;
            ViewBag.partner = partner;
            ViewBag.id_partner = id_partner;
            ViewBag.ffrom = ffrom;
            ViewBag.tto = tto;
            ViewBag.page = page==null?1:page;
            var nt= db.qrcode_log.OrderByDescending(o=>o.id).FirstOrDefault();
            ViewBag.notice = nt!=null?nt.actions:"";
            return View();
        }
        public int? getmaxsttfromcompany(int code_company)
        {
            try
            {
                var p = db.qrcodes.Where(o => o.code_company == code_company).OrderByDescending(o=>o.from_stt).FirstOrDefault();
                return p.to_stt+1;
            }
            catch
            {
                return 0;
            }
        }
        public ActionResult LogCompanyQrCode(DateTime? fdate, DateTime? tdate,string k,int? order, int? page)
        {
            if (Config.getCookie("is_admin") != "1") return RedirectToAction("Login", "Admin", new { message = "Bạn không được cấp quyền truy cập chức năng này" });
            if (k == null) k = "";
            var ctm = db.qrcode_log;
            var pageNumber = page ?? 1;
            var onePage = db.qrcode_log.Select(p => p); //ctm.OrderByDescending(f => f.id).ToPagedList(pageNumber, 20);
            //if (fdate == null) fdate = DateTime.Now.AddDays(-90);
            //if (tdate == null) tdate = DateTime.Now;
            if (fdate != null)
            {
                onePage = onePage.Where(o => o.date_time >= fdate);
            }
            if (tdate != null)
            {
                onePage = onePage.Where(o => o.date_time <= tdate);
            }
            if (k != "")
            {
                onePage = onePage.Where(o => o.actions.Contains(k));
            }
            if (order == null)
            {
                ViewBag.onePage = onePage.OrderByDescending(f => f.id).ToPagedList(pageNumber, 20);
            }
            if (order == 1)//date desc 
            {
                ViewBag.onePage = onePage.OrderByDescending(f => f.date_time).ToPagedList(pageNumber, 20);
            }
            if (order == 2)//date asc 
            {
                ViewBag.onePage = onePage.OrderBy(f => f.date_time).ToPagedList(pageNumber, 20);
            }
            //ViewBag.onePage = onePage;
            ViewBag.countall = ViewBag.onePage.Count;
            ViewBag.PageCount = ViewBag.onePage.PageCount;
            ViewBag.k = k;
            ViewBag.order = order;
            ViewBag.fdate = fdate;
            ViewBag.tdate = tdate;
            ViewBag.page = page == null ? 1 : page;
            return View();
        }
        public ActionResult CheckAll(int? code_company, string company, string partner, int? id_partner, DateTime? fdate, DateTime? tdate, string k,int? order, int? page)
        {
            if (Config.getCookie("is_admin") == "") return RedirectToAction("Login", "Admin", new { message = "Bạn không được cấp quyền truy cập chức năng này" });
            if (Config.getCookie("is_admin") != "1") { 
                code_company = int.Parse(Config.getCookie("company_code"));
                company = Config.getCookie("company_name");
            }
            if (k == null) k = "";
            var ctm = db.checkalls;
            var pageNumber = page ?? 1;
            var onePage = db.checkalls.Select(p => p); //ctm.OrderByDescending(f => f.id).ToPagedList(pageNumber, 20);
            if (fdate == null) fdate = DateTime.Now.AddDays(-90);
            if (tdate == null) tdate = DateTime.Now;
            //if (id_partner != null && code_company != null && id_partner != 0 && company != "")
            //{
            //    if (order == null) { 
            //        onePage = ctm.Where(o => o.code_company == code_company && o.id_partner == id_partner && o.date_time >= fdate && o.date_time <= tdate && o.province.Contains(provin)).OrderByDescending(f => f.id).ToPagedList(pageNumber, 20);
            //    }
            //    else
            //        if (order == 1)//sn, stt
            //        {
            //            onePage = ctm.Where(o => o.code_company == code_company && o.id_partner == id_partner && o.date_time >= fdate && o.date_time <= tdate && o.province.Contains(provin)).OrderBy(f => f.stt).ToPagedList(pageNumber, 20);
            //        } else
            //            if (order == 2)//sn, stt
            //            {
            //                onePage = ctm.Where(o => o.code_company == code_company && o.id_partner == id_partner && o.date_time >= fdate && o.date_time <= tdate && o.province.Contains(provin)).OrderByDescending(f => f.stt).ToPagedList(pageNumber, 20);
            //            }
                

            //}
            //else
            //{
            //    if (code_company != null && company != "")
            //    {
            //        onePage = ctm.Where(o => o.code_company == code_company && o.date_time >= fdate && o.date_time <= tdate && o.province.Contains(provin)).OrderByDescending(f => f.id).ToPagedList(pageNumber, 20);
            //    }
            //}
            if (code_company != null)
            {
                onePage = onePage.Where(o => o.code_company == code_company);
            }
            if (id_partner != null)
            {
                onePage = onePage.Where(o => o.id_partner == id_partner);
            }
            if (fdate != null)
            {
                onePage = onePage.Where(o => o.date_time >= fdate);
            }
            if (tdate != null)
            {
                onePage = onePage.Where(o => o.date_time <= tdate);
            }
            if (k != "")
            {
                onePage = onePage.Where(o => o.stt.ToString()==k || o.user_phone==k || o.user_email==k || o.guid==k);
            }
            if (order == null){
                ViewBag.onePage = onePage.OrderByDescending(f => f.id).ToPagedList(pageNumber, 20);
            }
            if (order == 1)//sn asc 
            {
                ViewBag.onePage = onePage.OrderBy(f => f.stt).ToPagedList(pageNumber, 20); 
            }
            if (order == 2)//sn desc 
            {
                ViewBag.onePage = onePage.OrderByDescending(f => f.stt).ToPagedList(pageNumber, 20);
            }
            if (order == 3)//date asc 
            {
                ViewBag.onePage = onePage.OrderBy(f => f.date_time).ToPagedList(pageNumber, 20);
            }
            if (order == 4)//date asc 
            {
                ViewBag.onePage = onePage.OrderByDescending(f => f.date_time).ToPagedList(pageNumber, 20);
            }
            //ViewBag.onePage = onePage;
            ViewBag.code_company = code_company;            
            ViewBag.id_partner = id_partner;
            ViewBag.company = company;
            ViewBag.partner = partner;
            ViewBag.fdate = fdate;
            ViewBag.tdate = tdate;
            ViewBag.countall = ViewBag.onePage.Count;
            ViewBag.is_admin = Config.getCookie("is_admin");
            ViewBag.k = k;
            ViewBag.order = order;
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
        public string UndoQrCode(string guid,int stt,string sguid)
        {
            try
            {
                if (stt == 0)
                {
                    if (db.checkalls.Any(o => o.guid == guid))
                    {
                        db.Database.ExecuteSqlCommand("delete from checkall where guid=N'" + guid + "'");
                        return "1";
                    }
                    else
                    {
                        return "0";
                    }
                }else
                {
                    if (db.checkalls.Any(o => o.guid == sguid))
                    {
                        db.Database.ExecuteSqlCommand("delete from checkall where guid=N'" + sguid + "' and stt="+ stt);
                        return "1";
                    }
                    else
                    {
                        return "0";
                    }
                }
                
            }
            catch
            {
                return "0";
            }
        }
        [HttpPost]
        public string generateQrCodeCompany(int code_company,string company,string partner,int id_partner, int ffrom, int tto)
        {
            try
            {
                if (db.qrcodes.Any(o => o.code_company == code_company && o.id_partner == id_partner && ((o.from_stt >= ffrom && o.from_stt <= tto)|| (o.to_stt >= ffrom && o.to_stt <= tto))))
                {
                    long? maxstt = db.qrcodes.Where(o => o.code_company == code_company && o.id_partner == id_partner).Max(o => o.to_stt);
                    return "Đã tồn tại khoảng thứ tự này, đề nghị chọn khoảng in khác, đã in đến số thứ tự " + maxstt;
                }
                //string info = db.config_app.Where(o => o.id==1).OrderBy(o => o.id).FirstOrDefault().text_in_qr_code;
                //if (db.config_app.Any(o => o.code_company == code_company))
                //{

                //    info = db.config_app.Where(o => o.code_company == code_company).OrderBy(o => o.id).FirstOrDefault().text_in_qr_code;
                //}
                
                DateTime fromtime = DateTime.Now;
                //for (long i = ffrom; i <= tto; i++)
                //{
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
                    qr.stt = ffrom;
                    qr.from_stt = ffrom;
                    qr.to_stt = tto;
                    db.qrcodes.Add(qr);
                    db.SaveChanges();
                //}
                qrcode_log ql = new qrcode_log();
                int totalminutes = (int)(DateTime.Now - fromtime).TotalMinutes;
                string notice = "Đã in qr code cho công ty " + company + ", nhà phân phối " + partner + ", từ số thứ tự " + ffrom + " đến số thứ tự " + tto + ",hoàn thành lúc " + DateTime.Now + ", hết " + totalminutes + " phút";
                ql.actions = notice;
                ql.user_name = Config.getCookie("user_name");
                ql.date_time = DateTime.Now;
                db.qrcode_log.Add(ql);
                db.SaveChanges();
                return notice;
            }
            catch(Exception ex)
            {
                return "Có lỗi xảy ra khi in "+ex.ToString();
            }
        }
        [HttpPost]
        public string cancelCompanyQrCode(int code_company, string company, string partner, int id_partner, long ffrom, long tto)
        {
            try
            {
                if (db.checkalls.Any(o => o.code_company == code_company && o.id_partner == id_partner && o.stt >= ffrom && o.stt <= tto))
                {
                    //long? maxstt = db.qrcodes.Where(o => o.code_company == code_company && o.id_partner == id_partner).Max(o => o.to_stt);
                    return "Không hủy được do trong dữ liệu quét của khách hàng đã tồn tại một thứ tự trong khoảng thứ tự ("+ffrom+"->"+tto+"), đề nghị chọn khoảng số thứ tự khác để hủy";
                }
              
                DateTime fromtime = DateTime.Now;
                db.Database.ExecuteSqlCommand("update qrcode set status=1 where code_company=" + code_company + " and id_partner=" + id_partner + " and from_stt>=" + ffrom + " and to_stt<=" + tto);
                qrcode_log ql = new qrcode_log();
                int totalminutes = (int)(DateTime.Now - fromtime).TotalMinutes;
                string notice = "Đã hủy các mã qr code cho công ty " + company + ", nhà phân phối " + partner + ", từ số thứ tự " + ffrom + " đến số thứ tự " + tto + ",hoàn thành lúc " + DateTime.Now + ", hết " + totalminutes + " phút";
                ql.actions = notice;
                db.qrcode_log.Add(ql);
                db.SaveChanges();
                return notice;
            }
            catch (Exception ex)
            {
                return "Có lỗi xảy ra khi in " + ex.ToString();
            }
        }
        [HttpPost]
        public string delCompanyQrCode(int code_company, string company, string partner, int id_partner, long ffrom, long tto)
        {
            try
            {
                if (db.checkalls.Any(o => o.code_company == code_company && o.id_partner == id_partner && o.stt >= ffrom && o.stt <= tto))
                {
                    //long? maxstt = db.qrcodes.Where(o => o.code_company == code_company && o.id_partner == id_partner).Max(o => o.stt);
                    return "Không xóa được do trong dữ liệu quét của khách hàng đã tồn tại một thứ tự trong khoảng thứ tự (" + ffrom + "->" + tto + "), đề nghị chọn khoảng số thứ tự khác để hủy";
                }

                DateTime fromtime = DateTime.Now;
                db.Database.ExecuteSqlCommand("delete from qrcode where code_company=" + code_company + " and id_partner=" + id_partner + " and from_stt>=" + ffrom + " and to_stt<=" + tto);
                qrcode_log ql = new qrcode_log();
                int totalminutes = (int)(DateTime.Now - fromtime).TotalMinutes;
                string notice = "Đã xóa các mã qr code cho công ty " + company + ", nhà phân phối " + partner + ", từ số thứ tự " + ffrom + " đến số thứ tự " + tto + ",hoàn thành lúc " + DateTime.Now + ", hết " + totalminutes + " phút";
                ql.actions = notice;
                db.qrcode_log.Add(ql);
                db.SaveChanges();
                return notice;
            }
            catch (Exception ex)
            {
                return "Có lỗi xảy ra khi in " + ex.ToString();
            }
        }
        [HttpPost]
        public string updateCompanyQrCode(int code_company, string company, string partner, int? id_partner, int? ffrom, int? tto)
        {
            try
            {
                //Tìm id cũ, do các stt nối tiếp nhau nên chỉ cập nhật ông nào cũ
                int? old_id_partner = db.qrcodes.Where(o => o.code_company == code_company && o.stt >= ffrom && o.stt <= tto).FirstOrDefault().id_partner;
                if (db.checkalls.Any(o => o.code_company == code_company && o.id_partner== old_id_partner && o.stt >= ffrom && o.stt <= tto))
                {
                    //long? maxstt = db.qrcodes.Where(o => o.code_company == code_company && o.id_partner == id_partner).Max(o => o.stt);
                    return "Không cập nhật được do trong dữ liệu quét của khách hàng đã tồn tại một thứ tự trong khoảng thứ tự (" + ffrom + "->" + tto + "), đề nghị chọn khoảng số thứ tự khác để hủy";
                }

                DateTime fromtime = DateTime.Now;
                int? cat = db.qrcodes.Count(o => o.code_company == code_company && o.id_partner == old_id_partner && o.from_stt==o.to_stt && o.stt >= ffrom && o.stt <= tto);
                if (cat > 1)
                {
                    db.Database.ExecuteSqlCommand("update qrcode set id_partner=" + id_partner + ",partner=N'" + partner + "' where code_company=" + code_company + " and id_partner="+ old_id_partner+" and from_stt=to_stt and stt >= "+ffrom +" and stt <="+ tto);
                }
                else
                {
                    cat = db.qrcodes.Count(o => o.code_company == code_company && o.id_partner == old_id_partner && o.from_stt != o.to_stt && o.from_stt<= tto && o.to_stt>= tto);
                    if (cat > 0)
                    {
                        //Chèn thêm 1 dòng mới
                        var tempcat = db.qrcodes.Where(o => o.code_company == code_company && o.id_partner == old_id_partner && o.from_stt < o.to_stt).FirstOrDefault();
                        string qrcodestr = tempcat.guid;
                        qrcode qr = new qrcode();
                        qr.code_company = code_company;
                        qr.company = company;
                        qr.date_id = tempcat.date_id;
                        qr.date_time = tempcat.date_time;
                        qr.from_stt = ffrom;
                        qr.guid = qrcodestr;
                        qr.id_partner = id_partner;
                        qr.partner = partner;
                        qr.qrcode1 = tempcat.qrcode1;
                        qr.qrcode2 = tempcat.qrcode2;
                        qr.sn = tempcat.sn;
                        qr.status = tempcat.status;
                        qr.stt = ffrom;
                        qr.to_stt = tto;
                        qr.winning_id = tempcat.winning_id;
                        db.qrcodes.Add(qr);
                        db.SaveChanges();
                        //Cắt dòng cũ đi
                        long ID = tempcat.id;
                        int? nextid = tto + 1;
                        db.Database.ExecuteSqlCommand("update qrcode set from_stt="+ nextid+ ",stt=" + nextid + " where id=" + ID);


                    }
                }
                qrcode_log ql = new qrcode_log();
                int totalminutes = (int)(DateTime.Now - fromtime).TotalMinutes;
                string notice = "Đã cập nhật các mã qr code cho công ty " + company + ", thành nhà phân phối " + partner + ", từ số thứ tự " + ffrom + " đến số thứ tự " + tto + ",hoàn thành lúc " + DateTime.Now + ", hết " + totalminutes + " phút";
                ql.actions = notice;
                db.qrcode_log.Add(ql);
                db.SaveChanges();
                return notice;
            }
            catch (Exception ex)
            {
                return "Có lỗi xảy ra khi in " + ex.ToString();
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
        [HttpPost,ValidateInput(false)]
        public string addUpdateVoucher(voucher_points cp)
        {
            return DBContext.addUpdateVoucher(cp);
        }
        [HttpPost, ValidateInput(false)]
        public string addUpdateWinning(winning cp)
        {
            return DBContext.addUpdateWinning(cp);
        }
        [HttpPost, ValidateInput(false)]
        public string addUpdateSplash(splash cp)
        {
            return DBContext.addUpdateSplash(cp);
        }
        [HttpPost]
        public string deleteSplash(int cpId)
        {
            return DBContext.deletesplash(cpId);
        }
        [HttpPost]
        public string deleteVoucher(int cpId)
        {
            return DBContext.deletevoucher(cpId);
        }
        [HttpPost]
        public string deleteWinning(int cpId)
        {
            return DBContext.deletewinning(cpId);
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
        [HttpGet]
        public string getTextInQrCode(int id_config_app, int code_company)
        {
            try
            {
                if (db.config_app.Any(o=>o.id== id_config_app))
                {
                    return db.config_app.Find(id_config_app).text_in_qr_code;
                }else
                {
                    return db.config_app.Where(o => o.code_company == code_company).FirstOrDefault().text_in_qr_code;
                }
            }
            catch
            {
                return "Chưa cấu hình sản phẩm";
            }
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
        public string getjsonproductcompany(int code_company)
        {
            var p = (from q in db.config_app where q.code_company == code_company select new { value = q.text_in_qr_code, id = q.id }).Distinct().ToList();
            return JsonConvert.SerializeObject(p);
        }
        public string confirmUpdateProduct(int code_company, int ffrom, int tto,int id_config_app)
        {
            try
            {
                db.Database.ExecuteSqlCommand("update qrcode set id_config_app=" + id_config_app + " where code_company=" + code_company + " and from_stt=" + ffrom + " and to_stt=" + tto);
                return "1";
            }
            catch
            {
                return "0";
            }
        }
        public string getfromandto(int? type, int code_company)
        {

            if (type == 0)
            {
                var p = (from q in db.qrcodes where q.code_company == code_company && q.to_stt>q.from_stt select new { value = q.from_stt, id = q.id }).Distinct().ToList();
                return JsonConvert.SerializeObject(p);
            }else
            {
                var p = (from q in db.qrcodes where q.code_company == code_company && q.to_stt > q.from_stt select new { value = q.to_stt, id = q.id }).Distinct().ToList();
                return JsonConvert.SerializeObject(p);
            }
            
        }
        public void exportCompanyQrCode(int code_company,string company,string partner,int id_partner,long ffrom,long tto)
        {
            
            try
            {

                var rs = db.qrcodes.Where(o => o.code_company == code_company && o.id_partner == id_partner && o.from_stt >= ffrom && o.to_stt <= tto).ToList();
                StringBuilder sb = new StringBuilder();
                //sb.Append("sn,qrcode\r\n");
                for (int i = 0; i < rs.Count;i++)
                {
                    if (rs[i].from_stt== rs[i].to_stt) { 
                        sb.Append("\""+rs[i].guid+"\",\""+rs[i].stt+"\"\r\n");
                    }
                    else{
                        sb.Append("\"" + rs[i].guid + "\",\"" + rs[i].from_stt + "\",\"" + rs[i].to_stt + "\"\r\n");
                    }
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
                //Response.Close();
                Response.End();

            }
            catch (Exception exmain)
            {
                return;
            }
        }
        public void exportCheckAll(int? code_company, string company, string partner, int? id_partner, DateTime? fdate, DateTime? tdate, int? order, string k,int? type)
        {

            try
            {
                if (k == null) k = "";
                if (type == 0)
                {

                    var rs = db.checkalls.Select(p => p);
                    //var rs = db.checkalls.Where(o => o.code_company == code_company && o.id_partner == id_partner && o.date_time >= fdate && o.date_time <= tdate).OrderByDescending(f => f.id).ToList();
                    //if (id_partner != null && code_company != null && id_partner != 0 && company != "")
                    //{
                    //    rs = db.checkalls.Where(o => o.code_company == code_company && o.id_partner == id_partner && o.date_time >= fdate && o.date_time <= tdate && o.province.Contains(provin)).OrderByDescending(f => f.id).ToList();

                    //}
                    //else
                    //{
                    //    if (code_company != null && company != "")
                    //    {
                    //        rs = db.checkalls.Where(o => o.code_company == code_company && o.date_time >= fdate && o.date_time <= tdate && o.province.Contains(provin)).OrderByDescending(f => f.id).ToList();
                    //    }
                    //}
                    if (code_company != null)
                    {
                        rs = rs.Where(o => o.code_company == code_company);
                    }
                    if (id_partner != null)
                    {
                        rs = rs.Where(o => o.id_partner == id_partner);
                    }
                    if (fdate != null)
                    {
                        rs = rs.Where(o => o.date_time >= fdate);
                    }
                    if (tdate != null)
                    {
                        rs = rs.Where(o => o.date_time <= tdate);
                    }
                    if (k != "")
                    {
                        rs = rs.Where(o => o.stt.ToString() == k || o.user_phone == k || o.user_email == k || o.guid == k);
                    }
                    if (order == null)
                    {
                        rs = rs.OrderByDescending(f => f.id);
                    }
                    if (order == 1)//sn asc 
                    {
                        rs = rs.OrderBy(f => f.stt);
                    }
                    if (order == 2)//sn desc 
                    {
                        rs = rs.OrderByDescending(f => f.stt);
                    }
                    if (order == 3)//date asc 
                    {
                        rs = rs.OrderBy(f => f.date_time);
                    }
                    if (order == 4)//date asc 
                    {
                        rs = rs.OrderByDescending(f => f.date_time);
                    }
                    var rss = rs.ToList();
                    StringBuilder sb = new StringBuilder();
                    //sb.Append("sn,qrcode\r\n");
                    sb.Append("<tr><th>Stt</th><th>Tên Công Ty</th><th>Nhà Phân Phối</th><th>Guid</th><th>Số thứ tự</th><th>Kích hoạt ngày</th><th>Email</th><th>Phone</th><th>Địa Chỉ</th><th>Tỉnh Thành</th><tr>");
                    for (int i = 0; i < rss.Count; i++)
                    {
                        //sb.Append("\"" + rs[i].company + "\",\"" + rs[i].partner + "\",\"" + rs[i].guid + "\",\"" + rs[i].date_time + "\",\"" + rs[i].address + "\",\"" + rs[i].province + "\"\r\n");
                        sb.Append("<tr><td>" + (i+1) + "</td><td>" + rss[i].company + "</td><td>" + rss[i].partner + "</td><td>" + rss[i].guid + "</td><td>" + rss[i].stt + "</td><td>" + rss[i].date_time + "</td><td>" + rss[i].user_email + "</td><td>" + rss[i].user_phone + "</td><td>" + rss[i].address + "</td><td>" + rss[i].province + "</td></tr>");
                    }
                    //Encoding csvEncoding = Encoding.Unicode;
                    Response.Clear();
                    Response.ClearContent();
                    Response.ClearHeaders();
                    Response.BufferOutput = true;                    
                    //Response.ContentType = "text/plain";
                    //Response.AddHeader("Content-Disposition", "attachment; filename=" + code_company + "_" + id_partner + "_"+ffrom+"_"+tto+".txt");
                    Response.ContentType = "application/vnd.ms-excel";//"text/csv";// 
                    //Response.ContentEncoding = csvEncoding;
                    Response.AddHeader("Content-Disposition", "attachment; filename=" + code_company + "_" + id_partner + "_" + fdate + "_" + tdate + ".xls");
                    //string allines = sb.ToString();
                    //UTF8Encoding utf8 = new UTF8Encoding();
                    //var preamble = utf8.GetPreamble();
                    //var data = utf8.GetBytes(allines);
                    //Response.BinaryWrite(data);
                    //Response.Write(htmlContent.ToString());
                    //byte[] buffer = Encoding.Unicode.GetBytes(sb.ToString());
                    //string convertedUtf8 = Encoding.Unicode.GetString(buffer);
                    //byte[] bytesUtf8 = Encoding.Unicode.GetBytes(convertedUtf8);
                    //Response.BinaryWrite(bytesUtf8);
                    Response.Write("<table cellspacing=0 cellpadding=0 border=\"1\">" + sb.ToString() + "</table>");
                    //Encoding encoding = Encoding.UTF8;
                    //var bytes = encoding.GetBytes(sb.ToString());
                    //MemoryStream stream = new MemoryStream(bytes);
                    //StreamReader reader = new StreamReader(stream);
                    //Response.Charset = encoding.BodyName;                    
                    //Response.ContentEncoding = Encoding.Unicode;
                    //Response.Output.Write(reader.ReadToEnd());
                    Response.Flush();
                    //Response.Close();
                    Response.End();
                }
                else
                {
                    string query = "SELECT company,partner,province,count(*) as count FROM [smartcheck].[dbo].[checkall] where company like N'" + company + "' and partner like N'" + partner + "' and date_time>=N'" + fdate + "' and date_time<=N'" + tdate + "' group by company,partner,province order by company,partner,province";

                    if (id_partner != null && code_company != null && id_partner != 0 && company != "")
                    {
                        query = "SELECT company,partner,province,count(*) as count FROM [smartcheck].[dbo].[checkall] where company like N'" + company + "' and partner like N'" + partner + "' and date_time>=N'" + fdate + "' and date_time<=N'" + tdate + "' group by company,partner,province order by company,partner,province";

                    }
                    else
                    {
                        if (code_company != null && company != "")
                        {
                            query = "SELECT company,partner,province,count(*) as count FROM [smartcheck].[dbo].[checkall] where company like N'" + company + "' and date_time>=N'" + fdate + "' and date_time<=N'" + tdate + "' group by company,partner,province order by company,partner,province";
                        }
                    }
                    var rs = db.Database.SqlQuery<itemCheckAll>(query).ToList();
                    StringBuilder sb = new StringBuilder();
                    //sb.Append("sn,qrcode\r\n");
                    sb.Append("<tr><th>Stt</th><th>Tên Công Ty</th><th>Nhà Phân Phối</th><th>Tỉnh Thành</th><th>Số Lượng</th><tr>");
                    for (int i = 0; i < rs.Count; i++)
                    {
                        //sb.Append("\"" + rs[i].company + "\",\"" + rs[i].partner + "\",\"" + rs[i].guid + "\",\"" + rs[i].date_time + "\",\"" + rs[i].address + "\",\"" + rs[i].province + "\"\r\n");
                        sb.Append("<tr><td>" +(i+1)+ "</td><td>" + rs[i].company + "</td><td>" + rs[i].partner + "</td><td>" + rs[i].province + "</td><td>" + rs[i].count + "</td></tr>");
                    }
                    //Encoding csvEncoding = Encoding.Unicode;
                    Response.Clear();
                    Response.ClearContent();
                    Response.ClearHeaders();
                    Response.BufferOutput = true;
                    //Response.ContentType = "text/plain";
                    //Response.AddHeader("Content-Disposition", "attachment; filename=" + code_company + "_" + id_partner + "_"+ffrom+"_"+tto+".txt");
                    Response.ContentType = "application/vnd.ms-excel";//"text/csv";// 
                    //Response.ContentEncoding = csvEncoding;
                    Response.AddHeader("Content-Disposition", "attachment; filename=" + code_company + "_" + id_partner + "_" + fdate + "_" + tdate + ".xls");
                    //string allines = sb.ToString();
                    //UTF8Encoding utf8 = new UTF8Encoding();
                    //var preamble = utf8.GetPreamble();
                    //var data = utf8.GetBytes(allines);
                    //Response.BinaryWrite(data);
                    //Response.Write(htmlContent.ToString());
                    //byte[] buffer = Encoding.Unicode.GetBytes(sb.ToString());
                    //string convertedUtf8 = Encoding.Unicode.GetString(buffer);
                    //byte[] bytesUtf8 = Encoding.Unicode.GetBytes(convertedUtf8);
                    //Response.BinaryWrite(bytesUtf8);
                    Response.Write("<table cellspacing=0 cellpadding=0 border=\"1\">" + sb.ToString() + "</table>");
                    //Encoding encoding = Encoding.UTF8;
                    //var bytes = encoding.GetBytes(sb.ToString());
                    //MemoryStream stream = new MemoryStream(bytes);
                    //StreamReader reader = new StreamReader(stream);
                    //Response.Charset = encoding.BodyName;                    
                    //Response.ContentEncoding = Encoding.Unicode;
                    //Response.Output.Write(reader.ReadToEnd());
                    Response.Flush();
                    //Response.Close();
                    Response.End();
                }

            }
            catch (Exception exmain)
            {
                return;
            }
        }
        public class itemCheckAll
        {
            public string company { get; set; }
            public string partner { get; set; }
            public string province { get; set; }
            public int count { get; set; }
        }
        [HttpPost]
        public string showCheckAll(string company, string partner, DateTime? fdate, DateTime? tdate,int? type)
        {
            /****** Script for SelectTopNRows command from SSMS  ******/
            string query = "SELECT company,partner,province,count(*) as count FROM [smartcheck].[dbo].[checkall] where company like N'" + company + "' and partner like N'" + partner + "' and date_time>=N'" + fdate + "' and date_time<=N'" + tdate + "' group by company,partner,province order by company,partner,province";
            if (type == 1)
            {
                if (partner != null && company != null && partner!="" && company!="")
                {
                    query = "SELECT company,partner,province,count(*) as count FROM [smartcheck].[dbo].[checkall] where company like N'" + company + "' and partner like N'" + partner + "' and date_time>=N'" + fdate + "' and date_time<=N'" + tdate + "' group by company,partner,province order by company,partner,province";

                }
                else
                {
                    if (company != null && company != "")
                    {
                        query = "SELECT company,partner,province,count(*) as count FROM [smartcheck].[dbo].[checkall] where company like N'" + company + "' and date_time>=N'" + fdate + "' and date_time<=N'" + tdate + "' group by company,partner,province order by company,partner,province";
                    }
                }
            }
            var p = db.Database.SqlQuery<itemCheckAll>(query).ToList();
            return JsonConvert.SerializeObject(p);
 
        }
        public ActionResult Reset(string message)
        {
            ViewBag.Message = message;

            return View();
        }
        [HttpPost]
        public string addUpdateConfig(config_bonus_point cp)
        {
            return DBContext.addUpdateConfig(cp);
        }
        public ActionResult uploadimg()
        {
            //if (Config.getCookie("logged") == "") return RedirectToAction("Login", "Account");
            if (Config.getCookie("is_admin") != "1") return Json(new { Message = "Error" }, JsonRequestBehavior.AllowGet);
            var fName = "";
            try
            {
                foreach (string fileName in Request.Files)
                {
                    HttpPostedFileBase file = Request.Files[fileName];
                    //if (!Config.IsImage(file)) return Json(new { Message = "/images/invalidimage.png" }, JsonRequestBehavior.AllowGet);
                    //Save file content goes here
                    if (file != null && file.ContentLength > 0)
                    {
                        var originalDirectory = new DirectoryInfo(string.Format("{0}images\\voucher", Server.MapPath(@"\")));
                        string strDay = DateTime.Now.ToString("yyyyMM");
                        string pathString = System.IO.Path.Combine(originalDirectory.ToString(), strDay);

                        var _fileName = Guid.NewGuid().ToString("N") + ".jpg";

                        bool isExists = System.IO.Directory.Exists(pathString);

                        if (!isExists)
                            System.IO.Directory.CreateDirectory(pathString);

                        var path = string.Format("{0}\\{1}", pathString, _fileName);
                        file.SaveAs(path);
                        //FileInfo f2 = new FileInfo(path);
                        //if (f2.Length > 100000)
                        //{
                        //    int percent = 50;
                        //    if (f2.Length > 1000000) percent = 10;
                        //    else
                        //        if (f2.Length > 500000) percent = 20;
                        //        else if (f2.Length > 300000) percent = 30;
                        //    ImageProcessor.ImageFactory iFF = new ImageProcessor.ImageFactory();
                        //    iFF.Load(path).Quality(percent).Save(path);
                        //}
                        fName = "/images/voucher/" + strDay + "/" + _fileName;
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return Json(new { Message = fName }, JsonRequestBehavior.AllowGet);
        }
        public string getFullDes(int id)
        {
            return db.voucher_points.Find(id).full_des;
        }
        public string getFullDesWinning(int id)
        {
            return db.winnings.Find(id).des;
        }
        public string getFullDesSplash(int id)
        {
            return db.splashes.Find(id).welcome_text;
        }
        public string getTotalPartnerQrCode(int? partner_id,int? code_company)
        {
            try
            {
                int? count = db.qrcodes.Count(o => o.code_company == code_company && o.id_partner == partner_id);
                if (count<=0)
                {
                    return "0";
                }else
                {
                    if (count <= 1)
                    {
                        var c2= db.qrcodes.Where(o => o.code_company == code_company && o.id_partner == partner_id).FirstOrDefault();                        
                        return c2.from_stt + " đến "+ c2.to_stt;
                    }
                    else
                    {
                        int? c1 = db.qrcodes.Count(o => o.code_company == code_company && o.id_partner == partner_id && o.from_stt == o.to_stt);
                        var c2 = db.qrcodes.Where(o => o.code_company == code_company && o.id_partner == partner_id && o.from_stt < o.to_stt).OrderBy(o=>o.from_stt).ToList();
                        string kq = "Có "+c1.ToString() + " tem guid lẻ<br>";
                        for(int i = 0; i < c2.Count; i++)
                        {
                            kq += "- Tem guid khối từ " +c2[i].from_stt.ToString() + " đến "+ c2[i].to_stt.ToString()+ "<br>";
                        }
                        return kq;
                    }
                }
            }
            catch
            {
                return "0";
            }
        }
        public ActionResult Splash(string k, int? page)
        {
            if (Config.getCookie("is_admin") != "1") return RedirectToAction("Login", "Admin", new { message = "Bạn không được cấp quyền truy cập chức năng này" });
            if (k == null) k = "";
            var ctm = db.splashes;
            var pageNumber = page ?? 1;
            var onePage = ctm.Where(o => o.welcome_text.Contains(k)).OrderByDescending(f => f.id).ToPagedList(pageNumber, 20);
            ViewBag.onePage = onePage;
            ViewBag.k = k;
            return View();
        }
    }
}
