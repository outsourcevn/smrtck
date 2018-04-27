using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using API.Models;
using System.Security.Cryptography;
using System.Data.Entity;
using System.Web.Script.Serialization;
using System.Net;
using System.Xml.Linq;
using System.Drawing;
using System.IO;
using Newtonsoft.Json.Linq;
using ImageProcessor;
using System.Text;

namespace API.Controllers
{
    public class HomeController : Controller
    {
        private smartcheckEntities db = new smartcheckEntities();
        public string Api(string status, Dictionary<string, string> field, string message)
        {
            string data = "[{";
            for (int i = 0; i < field.Count; i++)
            {
                data += "\"" + field.ElementAt(i).Key + "\":\"" + field.ElementAt(i).Value + "\",";
            }
            if (data.EndsWith(",")) data = data.Substring(0, data.Length - 1);
            data += "}]";
            string temp = "{\"status\":\"" + status + "\",\"data\":" + data + ",\"message\":\"" + message + "\"}";
            return temp;
        }
        public string ApiArray(string status, Dictionary<string, string> field, string message)
        {
            string data = "[{";
            for (int i = 0; i < field.Count; i++)
            {
                if (field.ElementAt(i).Value != null && field.ElementAt(i).Value.Contains("{"))
                {
                    data += "\"" + field.ElementAt(i).Key + "\":" + field.ElementAt(i).Value + ",";
                }
                else
                {
                    if (field.ElementAt(i).Value != null)
                    {
                        data += "\"" + field.ElementAt(i).Key + "\":\"" + field.ElementAt(i).Value + "\",";
                    }
                    else
                    {
                        data += "\"" + field.ElementAt(i).Key + "\":\"\",";
                    }
                }
            }
            if (data.EndsWith(",")) data = data.Substring(0, data.Length - 1);
            data += "}]";
            string temp = "{\"status\":\"" + status + "\",\"data\":" + data + ",\"message\":\"" + message + "\"}";
            return temp;
        }
        public bool getKeyApi(double? key)
        {
            try
            {

                double keytime1 = (DateTime.Now.AddMinutes(-43200) - new DateTime(1970, 1, 1)).TotalMilliseconds;
                double keytime2 = (DateTime.Now.AddMinutes(43200) - new DateTime(1970, 1, 1)).TotalMilliseconds;
                keytime1 = keytime1 * 13 + 27;
                keytime2 = keytime2 * 13 + 27;
                if (keytime1 <= key && key <= keytime2)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch { return false; }

        }
        //Thống nhất các API trả về cấu trúc JSON có dạng như sau "{\"status\":\"" + status + "\",\"data\":" + data + ",\"message\":\"" + message + "\"}";
        //Trong đó status báo là hàm trả về có thành công không: success, failed hoặc error là 3 trạng thái, data là các trường dữ liệu đi kèm và message là thông báo chi tiết
  
        
        //Hàm đăng ký user, gửi lên các đối số name là họ tên đầy đủ, email là thư điện tử, phone là số điện thoại, pass là mật khẩu mã hóa md5, user_id là id của user gửi kèm(với trường hợp cập nhật thông tin user)
        //key là khóa, nó có dạng Total Milli Seconds kể từ 1/1/1970 đến nay, client tính ra tổng số miliseconds này và *13+27, ví dụ tính ra là A, ta tính key=A*13+27 rồi gửi lên server, server kiểm tra hợp lệ thì mới cho vào
        //SERVER sẽ trả về mảng JSON có 3 trường dưới đây:
        //1. Trả về cấu trúc gồm status bao gồm 1 trong 3 trạng thái: success là báo đăng ký thành công(hoặc cập nhật), failed là báo thất bại, error là báo lỗi chi tiết sql nếu có
        //2. Trường data là các trường dữ liệu gừi kèm về, ví dụ nếu đăng ký thành công sẽ trả về id của user này để máy client lưu lại sau này còn biết gửi user_id lên server ở các API khác
        //3. Trường message là các thông báo cụ thể: Ví dụ đăng ký thành công nếu status là success, thông báo đã tồn tại email và số điện thoại này nếu status là failed, báo lỗi chi tiết sql nếu status là error
        //Lưu ý hàm này có thể dùng để cập nhật thông tin user, nếu user_id gửi lên là null hoặc =0 thì là thêm mới, còn nếu user_id gửi lên là số thì hàm này sẽ cập nhật thông tin user có id là user_id tương ứng với các đối số mới. Dùng khi màn hình đổi thông tin user nếu lúc đăng ký nhập sai.
        [HttpPost]
        public string register(string name, string email, string phone, string pass, long? user_id,string identify,string address,string ref_phone,string avatar,string profile_fb,DateTime? date_birth,double? key)
        {
            Dictionary<string, string> field = new Dictionary<string, string>();
            try
            {
                if (key == null || !getKeyApi(key))
                {
                    field.Add("TotalMilliseconds", (DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds.ToString());
                    return Api("failed", field, "Bảo mật server, hàm api này không chạy được do ngày giờ ở máy bị sai số quá 10 phút hoặc chưa gửi key bảo mật!");
                }
                if (phone == null) phone = "";
                if (email == null) email = "";
                phone = phone.Trim();
                email = email.Trim();
                //Nếu là đăng ký mới, và đăng ký này không từ FB
                if ((user_id == 0 || user_id == null) && (profile_fb==null || profile_fb=="") && (phone == "" || phone == null || pass == "" || pass == null))
                {
                    field.Add("user_id", "");
                    return Api("failed", field, "Số điện thoại hoặc pass phải khác rỗng!");
                }
                if ((user_id == 0 || user_id == null) && email!=null && email!="" && db.customers.Any(o => o.email == email || o.phone == phone))
                {
                    field.Add("user_id", "");
                    return Api("failed", field, "Đã tồn tại email hoặc số điện thoại này");
                }
                if (user_id == 0 || user_id == null)
                {
                    MD5 md5Hash = MD5.Create();
                    string hash = Config.GetMd5Hash(md5Hash, pass);
                    customer ct = new customer();
                    ct.email = email;
                    ct.name = name;
                    ct.pass = hash;
                    ct.phone = phone;
                    ct.date_time = DateTime.Now;
                    ct.points = 100;
                    ct.profile_fb = profile_fb;
                    db.customers.Add(ct);
                    db.SaveChanges();
                    //Cộng điểm cho người giới thiệu
                    if (ref_phone != null && ref_phone != "")
                    {
                        if (db.customers.Any(o => o.phone == ref_phone))
                        {
                            var bnu = db.customers.Where(o => o.phone == ref_phone).OrderBy(o => o.id).FirstOrDefault();
                            int? bnp = db.config_bonus_point.Find(1).ref_point;
                            db.Database.ExecuteSqlCommand("update customers set points=points+" + bnp + " where id=" + bnu.id);
                        }
                    }
                    if (avatar != "")
                    {
                        try
                        {
                            string fileName = ct.id.ToString() + ".jpg";
                            string file_name = Server.MapPath(@"\") + "\\images\\customer\\" + fileName;
                            save_file_from_url(file_name, avatar);
                            avatar = "/images/customer/" + fileName;
                            ImageProcessor.ImageFactory iFF = new ImageProcessor.ImageFactory();
                            iFF.Load(file_name).Quality(50).Save(file_name);
                            db.Database.ExecuteSqlCommand("update customers set avatar=N'" + avatar + "' where id=" + ct.id);
                        }
                        catch (Exception dlimage) { }
                    }
                    field.Add("name", name);
                    field.Add("phone", phone);
                    field.Add("avatar", avatar);
                    field.Add("user_id", ct.id.ToString());
                    return Api("success", field, "Đăng ký thành công!");
                }
                else
                {
                    //Nếu là đăng ký mới, và đăng ký này không từ FB
                    if (phone == "" || phone == null || pass == "" || pass == null)
                    {
                        field.Add("user_id", "");
                        return Api("failed", field, "Số điện thoại hoặc pass phải khác rỗng!");
                    }
                    MD5 md5Hash = MD5.Create();
                    string hash = Config.GetMd5Hash(md5Hash, pass);
                    customer ct = db.customers.Find((long)user_id);
                    db.Entry(ct).State = EntityState.Modified;
                    ct.email = email;
                    ct.name = name;
                    if (pass != null && pass.Trim() != "")
                    {
                        ct.pass = hash;
                    }
                    ct.phone = phone;
                    ct.identify = identify;
                    ct.address = address;
                    ct.date_birth = date_birth;
                    ct.date_time = DateTime.Now;
                    db.SaveChanges();
                    field.Add("name", name);
                    field.Add("phone", phone);
                    field.Add("avatar", ct.avatar);
                    field.Add("user_id", user_id.ToString());
                    return Api("success", field, "Cập nhật thành công!");
                }
            }catch(Exception ex){
                field.Add("user_id", "");
                return Api("error", field, "Cập nhật lỗi sql: " + ex.ToString());
            }
        }
        public string changePassOfUser(long? user_id,string old_pass, string new_pass,double? key)
        {
            Dictionary<string, string> field = new Dictionary<string, string>();
            try
            {
                if (key == null || !getKeyApi(key))
                {
                    field.Add("TotalMilliseconds", (DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds.ToString());
                    return Api("failed", field, "Bảo mật server, hàm api này không chạy được do ngày giờ ở máy bị sai số quá 10 phút hoặc chưa gửi key bảo mật!");
                }
                MD5 md5Hash = MD5.Create();
                string hash_old = Config.GetMd5Hash(md5Hash, old_pass);
                string hash_new = Config.GetMd5Hash(md5Hash, new_pass);

                if (user_id != null && user_id != 0 && db.customers.Any(o=>o.id==user_id && o.pass== hash_old))
                {
                    db.Database.ExecuteSqlCommand("update customers set pass=N'"+ hash_new + "' where id="+ user_id);
                    field.Add("user_id", user_id.ToString());
                    return Api("success", field, "Đổi pass thành công!");
                }else
                {
                    field.Add("user_id", "");
                    return Api("failed", field, "Đổi pass không thành công, không tồn tại user id hoặc pass cũ như vậy!");
                }
                
            }
            catch (Exception ex)
            {
                field.Add("user_id", "");
                return Api("error", field, "Cập nhật lỗi sql: " + ex.ToString());
            }
        }
        public string loginFb(string name, string email,string avatar,string phone,string profile_fb)
        {
            Dictionary<string, string> field = new Dictionary<string, string>();
            try
            {
                
                if (profile_fb!=null && profile_fb!="" && db.customers.Any(o=>o.profile_fb==profile_fb))
                {
                    var p = (from q in db.customers where q.profile_fb == profile_fb select q).OrderBy(o => o.id).FirstOrDefault();
                    if (avatar.Contains("https://"))
                    {
                        try
                        {
                            string fileName = p.id.ToString() + ".jpg";
                            string file_name = Server.MapPath(@"\") + "\\images\\customer\\" + fileName;
                            save_file_from_url(file_name, avatar);
                            avatar = "/images/customer/" + fileName;
                            ImageProcessor.ImageFactory iFF = new ImageProcessor.ImageFactory();
                            iFF.Load(file_name).Quality(50).Save(file_name);
                            db.Database.ExecuteSqlCommand("update customers set avatar=N'" + avatar + "' where id=" + p.id);
                        }
                        catch (Exception dlimage2) { }
                    }
                    //if (name!="" && name!="" && avatar)
                    db.Database.ExecuteSqlCommand("update customers set name=N'"+name+ "',avatar=N'" + avatar + "' where profile_fb=N'"+ profile_fb + "'");
                    field.Add("user_id", p.id.ToString());
                    return Api("success", field, "Đăng nhập thành công!");
                }
                else
                {
                    customer ctm = new customer();
                    ctm.address = "";
                    ctm.points = 100;
                    ctm.avatar = "";
                    ctm.date_time = DateTime.Now;
                    ctm.email = email;
                    ctm.name = name;
                    ctm.phone = phone;
                    ctm.profile_fb = profile_fb;
                    db.customers.Add(ctm);
                    db.SaveChanges();
                    if (avatar != "" && avatar.Contains("https://"))
                    {
                        try
                        {
                            string fileName = ctm.id.ToString() + ".jpg";
                            string file_name = Server.MapPath(@"\") + "\\images\\customer\\" + fileName;
                            save_file_from_url(file_name, avatar);
                            avatar = "/images/customer/" + fileName;
                            ImageProcessor.ImageFactory iFF = new ImageProcessor.ImageFactory();
                            iFF.Load(file_name).Quality(50).Save(file_name);
                            db.Database.ExecuteSqlCommand("update customers set avatar=N'" + avatar + "' where id=" + ctm.id);
                        }
                        catch (Exception dlimage2) { }
                    }
                    
                    field.Add("user_id", ctm.id.ToString());
                    return Api("success", field, "Đăng nhập thành công!");
                }
            }
            catch(Exception ex){
                field.Add("user_id", "");
                return Api("error", field, "Cập nhật lỗi sql: " + ex.ToString());
            }
        }
        public string loginFbCheck(string profile_fb)
        {
            Dictionary<string, string> field = new Dictionary<string, string>();
            try
            {

                if (profile_fb != null && profile_fb != "" && db.customers.Any(o => o.profile_fb == profile_fb))
                {
                    var p = (from q in db.customers where q.profile_fb == profile_fb select q).OrderBy(o => o.id).FirstOrDefault();
                    field.Add("user_id", p.id.ToString());
                    return Api("success", field, "Đăng nhập thành công!");
                }
                else
                {
                    
                    field.Add("user_id", "");
                    return Api("failed", field, "Đăng nhập thất bại!");
                }
            }
            catch (Exception ex)
            {
                field.Add("user_id", "");
                return Api("error", field, "Cập nhật lỗi sql: " + ex.ToString());
            }
        }
        public static void save_file_from_url(string file_name, string url)
        {
            byte[] content;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            WebResponse response = request.GetResponse();

            Stream stream = response.GetResponseStream();

            using (BinaryReader br = new BinaryReader(stream))
            {
                content = br.ReadBytes(500000);
                br.Close();
            }
            response.Close();

            FileStream fs = new FileStream(file_name, FileMode.Create);
            BinaryWriter bw = new BinaryWriter(fs);
            try
            {
                bw.Write(content);
            }
            finally
            {
                fs.Close();
                bw.Close();
            }

        }
        //Hàm này login bằng số phone và mật khẩu, gửi kèm key bảo mật
        //Trả về là user_id của user này nếu đăng nhập thành công, nếu không báo sai pass và mật khẩu kèm theo user_id là rỗng
        [HttpPost]
        public string login(string phone, string pass, double? key)
        {
            Dictionary<string, string> field = new Dictionary<string, string>();
            try
            {
                MD5 md5Hash = MD5.Create();
                string hash = Config.GetMd5Hash(md5Hash, pass);
                if (db.customers.Any(o => o.phone == phone && o.pass == hash))
                {
                    string user_id = db.customers.Where(o => o.phone == phone && o.pass == hash).FirstOrDefault().id.ToString();
                    field.Add("user_id", user_id);
                    return Api("success", field, "Đăng nhập thành công!");
                }
                else
                {
                    field.Add("user_id", "");
                    return Api("failed", field, "Đăng nhập không thành công, sai số điện thoại hoặc mật khẩu!");
                }
            }
            catch (Exception ex)
            {
                field.Add("user_id", "");
                return Api("error", field, "Lỗi sql: " + ex.ToString());
            }
        }
        
        public string getInfoUser(long user_id)
        {
            Dictionary<string, string> field = new Dictionary<string, string>();
            try
            {
                var us = db.customers.Find(user_id);
                if (us!=null)
                {
                    field.Add("user_id", user_id.ToString());
                    field.Add("user_name", us.name);
                    field.Add("user_email", us.email);
                    field.Add("user_phone", us.phone);
                    field.Add("date_birth", us.date_birth.ToString());
                    field.Add("identify", us.identify);
                    field.Add("address", us.address);
                    field.Add("avatar",us.avatar);
                    field.Add("points", us.points.ToString());
                    return Api("success", field, "Thông tin khách hàng!");
                }
                else
                {
                    field.Add("user_id", "");
                    field.Add("user_name", "");
                    field.Add("user_email", "");
                    field.Add("user_phone", "");
                    field.Add("date_birth", "");
                    field.Add("identify", "");
                    field.Add("address", "");
                    field.Add("avatar", "");
                    return Api("failed", field, "Không tìm thấy user này!");
                }
            }
            catch (Exception ex)
            {
                field.Add("user_id", "");
                return Api("error", field, "Lỗi sql: " + ex.ToString());
            }
        }
        //Hàm này trả về danh sách các voucher sắp xếp giảm dần theo id, mới nhất lên đầu
        public string getListVoucher()
        {
            Dictionary<string, string> field = new Dictionary<string, string>();
            try
            {
                DateTime dtn=DateTime.Now;
                var p = (from q in db.voucher_points where q.from_date <= dtn && q.to_date >= dtn && q.quantity>0 select new { id = q.id, name = q.name, image = q.image, point = q.price }).OrderByDescending(o => o.id).ToList();
                field.Add("list", JsonConvert.SerializeObject(p));
                return ApiArray("success", field, "Danh sách các voucher");
            }
            catch (Exception ex)
            {
                field.Add("list", "[]");
                return Api("error", field, "Lỗi sql: " + ex.ToString());
            }
        }
        //Hàm này trả về danh sách các màn chào hỏi Splash lúc mới vào app
        public string getListSplash()
        {
            Dictionary<string, string> field = new Dictionary<string, string>();
            try
            {
                DateTime dtn = DateTime.Now;
                var p = (from q in db.splashes select q).OrderByDescending(o => o.id).ToList();
                for(int i = 0; i < p.Count; i++)
                {
                    p[i].welcome_text = p[i].welcome_text.Replace("\"", "\\\"");
                }
                field.Add("list", JsonConvert.SerializeObject(p));
                return ApiArray("success", field, "Danh sách các màn hình chào splash");
            }
            catch (Exception ex)
            {
                field.Add("list", "[]");
                return Api("error", field, "Lỗi sql: " + ex.ToString());
            }
        }
        //Hàm này trả về chi tiết voucher
        public string getListVoucherDetail(int id,int? os)
        {
            Dictionary<string, string> field = new Dictionary<string, string>();
            try
            {

                var p = db.voucher_points.Find(id);
                if (os == null || os == 1)
                {
                    field.Add("name", p.name);
                    field.Add("image", p.image);
                    field.Add("point", p.price.ToString());
                    field.Add("quantity", p.quantity.ToString());
                    field.Add("from_date", String.Format("{0:yyyy/MM/dd hh:mm tt}", p.from_date.Value));
                    field.Add("to_date", String.Format("{0:yyyy/MM/dd hh:mm tt}", p.to_date.Value));
                    field.Add("big_image", p.big_image);
                    field.Add("image1", p.image1);
                    field.Add("image2", p.image2);
                    field.Add("image3", p.image3);
                    field.Add("full_des", p.full_des.Replace("\"", "\\\""));//HttpUtility.HtmlEncode(System.Security.SecurityElement.Escape(p.full_des))
                    return Api("success", field, "Danh sách chi tiết voucher");
                }
                else
                {
                    field.Add("name", p.name);
                    field.Add("image", p.image);
                    field.Add("point", p.price.ToString());
                    field.Add("quantity", p.quantity.ToString());
                    field.Add("from_date", String.Format("{0:yyyy/MM/dd hh:mm tt}", p.from_date.Value));
                    field.Add("to_date", String.Format("{0:yyyy/MM/dd hh:mm tt}", p.to_date.Value));
                    field.Add("big_image", p.big_image);
                    field.Add("image1", p.image1);
                    field.Add("image2", p.image2);
                    field.Add("image3", p.image3);
                    field.Add("full_des", "");
                    return Api("success", field, "Danh sách chi tiết voucher");
                }
            }
            catch (Exception ex)
            {
                field.Add("list", "[]");
                return Api("error", field, "Lỗi sql: " + ex.ToString());
            }
        }
        public ActionResult ViewListVoucherDetail(int id)
        {
            var p = db.voucher_points.Find(id);
            return View(p);
        }
        public string updateCpnPtrWn()
        {
            var p = (from q in db.winning_log select q).ToList();
            for(int i = 0; i < p.Count; i++)
            {
                var item = p[i];
                int? scode_company = 0;
                long spartner = 0;
                string spartner_name = "";
                try
                {
                    scode_company = db.companies.Where(o => o.name == item.company).OrderBy(o => o.id).FirstOrDefault().code;
                }catch
                {

                }
                try
                {
                    spartner = db.partners.Where(o => o.code_company == scode_company && o.id == item.id_partner).OrderBy(o => o.id).FirstOrDefault().id;
                    spartner_name = db.partners.Where(o => o.code_company == item.code_company && o.id==item.id_partner).OrderBy(o => o.id).FirstOrDefault().name;
                }
                catch
                {

                }                
                db.Database.ExecuteSqlCommand("update winning_log set code_company=" + scode_company + ",id_partner=" + spartner + ",partner=N'"+ spartner_name + "' where id="+item.id);
            }
            return "updated";
        }
        //Hàm này trả về chi tiết trúng thưởng
        public string getWinningDetail(int id,int? sn,int? os)
        {
            Dictionary<string, string> field = new Dictionary<string, string>();
            string product_name = "";
            string company_address = "";
            string company_phone = "";
            string company_email = "";
            string company_web = "";
            string partner = "";
            int? is_received = 0;
            try
            {
                //var pwl = db.winning_log.Find(id);

                DateTime? date_time = null;
                var p = db.winnings.Find(id);
                try
                {
                    var pwl = db.winning_log.Where(o => o.winning_id == id && o.code_company == p.code_company && o.sn == sn).OrderBy(o => o.id).FirstOrDefault();
                    if (pwl != null)
                    {
                        product_name = pwl.product_name.Replace("\"", "\\\"");
                        date_time=pwl.date_time;
                        partner = pwl.partner;
                        is_received = pwl.is_received;


                    }
                    else
                    {
                        product_name = "";
                    }
                    var cpn = db.companies.Where(o => o.code == pwl.code_company).OrderBy(o => o.id).FirstOrDefault();
                    company_address = cpn.address;
                    company_email = cpn.email_contact;
                    company_phone = cpn.phone_contact;
                    company_web = cpn.web;
                }
                catch
                {
                    product_name = "";
                }

                if (os == null || os == 1)
                {
                    
                    field.Add("company", p.company);
                    field.Add("partner", partner);
                    field.Add("is_received", is_received.ToString());
                    if (sn == null) sn = 0;
                    field.Add("sn", sn.ToString());
                    field.Add("name", p.name);
                    field.Add("image", p.image);
                    field.Add("money", p.money.ToString());
                    field.Add("quantity", p.quantity.ToString());
                    field.Add("from_date", p.from_date.ToString());
                    field.Add("to_date", p.to_date.ToString());
                    field.Add("big_image", p.big_image);
                    field.Add("image1", p.image1);
                    field.Add("image2", p.image2);
                    field.Add("image3", p.image3);
                    field.Add("des", p.des.Replace("\"", "\\\""));//HttpUtility.HtmlEncode(p.des)
                    field.Add("product_name", product_name);
                    field.Add("date_time", date_time.ToString());
                    field.Add("company_phone", company_phone);
                    field.Add("company_email", company_email);
                    field.Add("company_address", company_address);
                    field.Add("company_web", company_web);
                    return Api("success", field, "Danh sách chi tiết trúng thưởng");
                }
                else {
                    field.Add("company", p.company);
                    field.Add("partner", partner);
                    field.Add("is_received", is_received.ToString());
                    if (sn == null) sn = 0;
                    field.Add("sn", sn.ToString());
                    field.Add("name", p.name);
                    field.Add("image", p.image);
                    field.Add("money", p.money.ToString());
                    field.Add("quantity", p.quantity.ToString());
                    field.Add("from_date", p.from_date.ToString());
                    field.Add("to_date", p.to_date.ToString());
                    field.Add("big_image", p.big_image);
                    field.Add("image1", p.image1);
                    field.Add("image2", p.image2);
                    field.Add("image3", p.image3);
                    field.Add("des", "");
                    field.Add("product_name", product_name);
                    field.Add("date_time", date_time.ToString());
                    field.Add("company_phone", company_phone);
                    field.Add("company_email", company_email);
                    field.Add("company_address", company_address);
                    field.Add("company_web", company_web);
                    //HttpUtility.HtmlEncode(p.des)
                    return Api("success", field, "Danh sách chi tiết trúng thưởng");
                }
            }
            catch (Exception ex)
            {
                field.Add("list", "[]");
                return Api("error", field, "Lỗi sql: " + ex.ToString());
            }
        }
        public string delWinningDetail(int id)
        {
            Dictionary<string, string> field = new Dictionary<string, string>();
            try
            {

                    db.Database.ExecuteSqlCommand("delete from winning_log where id=" + id);
                    field.Add("id", id.ToString());                    
                    return Api("success", field, "Xóa trúng thưởng này thành công");
                
            }
            catch (Exception ex)
            {
                field.Add("list", "[]");
                return Api("error", field, "Lỗi sql: " + ex.ToString());
            }
        }
        public string testJson()
        {
            Dictionary<string, string> field = new Dictionary<string, string>();
            try
            {

                var p = db.config_app.Find(29);
                field.Add("product_info", convertToAscii(p.product_info));
                field.Add("waranty_text", convertToAscii(p.waranty_text));
                field.Add("buy_more", convertToAscii(p.buy_more));
                return Api("success", field, "test json");

            }
            catch (Exception ex)
            {
                field.Add("list", "[]");
                return Api("error", field, "Lỗi sql: " + ex.ToString());
            }
        }
        //Hàm này trả về chi tiết liên hệ nơi bán
        public string getBuyMoreDetail(int id)
        {
            Dictionary<string, string> field = new Dictionary<string, string>();
            try
            {
                var p = db.checkalls.Find(id);
                long? stt = p.stt;
                int? code_company = p.code_company;
                int? config_id = 0;
                if (!db.company_configapp_qrcode_link.Any(o=>o.code_company==code_company && o.from_sn<=stt && o.to_sn>=stt))
                {
                    var q = db.qrcodes.Where(o => o.code_company == code_company && o.from_stt <= stt && o.to_stt >= stt).OrderBy(o => o.id).FirstOrDefault();                    
                    if (q != null)
                    {
                        config_id = q.id_config_app;
                    }
                }else
                {
                    var q = db.company_configapp_qrcode_link.Where(o => o.code_company == code_company && o.from_sn <= stt && o.to_sn >= stt).OrderBy(o=>o.id).FirstOrDefault();
                    if (q != null)
                    {
                        config_id = q.id_config_app;
                    }
                }
                var cid = db.config_app.Find(config_id);

                if (cid != null)
                {
                    field.Add("buy_more", cid.buy_more.Replace("\"", "\\\""));
                }else
                {
                    field.Add("buy_more","");
                }
                return Api("success", field, "Chi tiết liên hệ nơi bán mua thêm");
                
            }
            catch (Exception ex)
            {
                field.Add("buy_more", "");
                return Api("error", field, "Lỗi sql: " + ex.ToString());
            }
        }
        //Hàm này trả về chi tiết liên hệ nơi bảo hành
        public string getWarantyMoreDetail(int id)
        {
            Dictionary<string, string> field = new Dictionary<string, string>();
            try
            {
                var p = db.checkalls.Find(id);
                long? stt = p.stt;
                int? code_company = p.code_company;
                int? config_id = 0;
                if (!db.company_configapp_qrcode_link.Any(o => o.code_company == code_company && o.from_sn <= stt && o.to_sn >= stt))
                {
                    var q = db.qrcodes.Where(o => o.code_company == code_company && o.from_stt <= stt && o.to_stt >= stt).OrderBy(o => o.id).FirstOrDefault();
                    if (q != null)
                    {
                        config_id = q.id_config_app;
                    }
                }
                else
                {
                    var q = db.company_configapp_qrcode_link.Where(o => o.code_company == code_company && o.from_sn <= stt && o.to_sn >= stt).OrderBy(o => o.id).FirstOrDefault();
                    if (q != null)
                    {
                        config_id = q.id_config_app;
                    }
                }
                var cid = db.config_app.Find(config_id);

                if (cid != null)
                {
                    field.Add("waranty_text", cid.waranty_text.Replace("\"", "\\\""));
                }
                else
                {
                    field.Add("waranty_text", "");
                }
                return Api("success", field, "Chi tiết liên hệ nơi bảo hành");

            }
            catch (Exception ex)
            {
                field.Add("waranty_text", "");
                return Api("error", field, "Lỗi sql: " + ex.ToString());
            }
        }
        public ActionResult ViewWinningDetail(long id)
        {
            //var pwl = db.winning_log.Find(id);
            var p = db.winnings.Find(id);
            return View(p);
        }
        //Hàm này trả về chi tiết code của voucher có id là id và của user có id là user_id
        public string getVoucherCode(long user_id, long voucher_id)
        {
            Dictionary<string, string> field = new Dictionary<string, string>();
            try
            {
                var p = db.voucher_log.Where(o => o.voucher_id == voucher_id && o.user_id==user_id).OrderByDescending(o => o.id).FirstOrDefault();
                field.Add("code", p.code.ToString());
                return Api("success", field, "code của user_id với voucher_id vừa mua");
            }
            catch (Exception ex)
            {
                field.Add("list", "[]");
                return Api("error", field, "Lỗi sql: " + ex.ToString());
            }
        }
        //Hàm này trả về danh sách các voucher mà user này đã đổi thành công để xem lại
        public string getVoucherOfUser(long user_id)
        {
            Dictionary<string, string> field = new Dictionary<string, string>();
            try
            {
                //var p = db.voucher_log.Where(o => o.user_id == user_id).OrderByDescending(o => o.id).ToList();
                var p=(from q in db.voucher_log where q.user_id==user_id join q2 in db.voucher_points on q.voucher_id equals q2.id into ps
                            from q2 in ps.DefaultIfEmpty()
                           select new {
                               id=q.id,
                               user_id=q.user_id,
                               user_name=q.user_name,
                               user_email=q.user_email,
                               user_phone=q.user_phone,
                               voucher_id=q.voucher_id,
                               voucher_name=q.voucher_name,
                               voucher_image=q2.image,
                               points=q.points,
                               date_time=q.date_time,
                               lon=q.lon,
                               lat=q.lat,
                               address=q.address,
                               code=q.code
                           }).OrderByDescending(o => o.id).ToList();
                field.Add("list", JsonConvert.SerializeObject(p));
                return ApiArray("success", field, "Danh sách lịch sử đổi điểm voucher của user này");
            }
            catch (Exception ex)
            {
                field.Add("list", "[]");
                return Api("error", field, "Lỗi sql: " + ex.ToString());
            }
        }
        //Hàm này trả về danh sách lịch sử quét của user
        public string getHistoryCheckOfUser(long? user_id,string keyword)
        {
            Dictionary<string, string> field = new Dictionary<string, string>();
            try
            {
                if (keyword == null) keyword = "";
                var p = db.checkalls.Where(o => o.user_id == user_id).Where(o=>o.waranty_text.Contains(keyword) || o.waranty_name.Contains(keyword) || o.product_text.Contains(keyword) && o.waranty_phone.Contains(keyword) || o.address.Contains(keyword) || o.partner.Contains(keyword)).OrderByDescending(o => o.id).Take(100).ToList();
                //var p=from q in db.checkalls where (q.user_id== user_id) && (q.waranty_text.Contains(keyword) || q.waranty_phone.Contains(keyword) || q.address.Contains(keyword) || q.partner.Contains(keyword) select new { id=q.id, address = q.address, code_company = q.code_company, company = q.company, date_time = q.date_time, guid = q.guid, id_config_app = q.id_config_app, id_partner = q.id_partner,lat=q.lat,lon=q.lon,os=q.os, partner = q.partner, product_text = q.product_text, province = q.province, status = q.status, stt = q.stt, user_email = q.user_email, user_id = q.user_id, user_name = q.user_name, user_phone = q.user_phone,
                //    waranty_address = q.waranty_address,
                //    waranty_link_web = q.waranty_link_web,
                //    waranty_name = q.waranty_name,
                //    waranty_phone = q.waranty_phone,
                //    waranty_text = q.waranty_text,
                //    waranty_year = q.waranty_year                    
                //});
                field.Add("list", JsonConvert.SerializeObject(p));
                return ApiArray("success", field, "Danh sách lịch sử quét của user này");
            }
            catch (Exception ex)
            {
                field.Add("list", "[]");
                return Api("error", field, "Lỗi sql: " + ex.ToString());
            }
        }
        //Hàm này trả về danh sách nhà phân phối của dòng có id trong hàm getHistoryCheckOfUser trả về
        //Họ ấn vào đó để sửa thông tin bảo hành, mình hiện ra ngoài Họ tên, Email và Số điện thoại...để họ cập nhật lại thì các em load thêm 1 list nhà phân phối do api này trả về
        public string getListPartnerOfUser(long id)
        {
            Dictionary<string, string> field = new Dictionary<string, string>();
            try
            {
                int? code_company = db.checkalls.Find(id).code_company;
                var p = db.partners.Where(o => o.code_company==code_company).OrderBy(o => o.name).Take(100).ToList();
                field.Add("list", JsonConvert.SerializeObject(p));
                return ApiArray("success", field, "Danh sách Nhà phân phối");
            }
            catch (Exception ex)
            {
                field.Add("list", "[]");
                return Api("error", field, "Lỗi sql: " + ex.ToString());
            }
        }
        //Hàm này cập nhật trạng thái là đã xóa lịch sử quét của id trên list danh sách Bảo Hành(Lịch sử quét)
        [HttpPost]
        public string delHistoryCheckOfUser(int id, double? key)
        {
            Dictionary<string, string> field = new Dictionary<string, string>();
            if (key == null || !getKeyApi(key))
            {
                field.Add("TotalMilliseconds", (DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds.ToString());
                return Api("failed", field, "Ngày giờ ở máy bị sai số quá 10 phút nên ứng dụng không chạy được, Bạn cần chỉnh lại thời gian ở điện thoại!");
            }
            try
            {
                customer_bonus_log cbl = new customer_bonus_log();
                db.Database.ExecuteSqlCommand("update checkall set status=1 where id=" + id);
                field.Add("status", "1");
                return Api("success", field, "Xóa thành công!");
            }
            catch (Exception ex)
            {
                field.Add("status", "");
                return Api("error", field, "Lỗi sql: " + ex.ToString());
            }

        }
        //Cập nhật lại thông tin của người cần bảo hành
        [HttpPost]
        public string updateHistoryCheckOfUser(int id, string waranty_name,string waranty_phone,string waranty_address,int? id_partner,string partner, double? key)
        {
            Dictionary<string, string> field = new Dictionary<string, string>();
            if (key == null || !getKeyApi(key))
            {
                field.Add("TotalMilliseconds", (DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds.ToString());
                return Api("failed", field, "Ngày giờ ở máy bị sai số quá 10 phút nên ứng dụng không chạy được, Bạn cần chỉnh lại thời gian ở điện thoại!");
            }
            try
            {
                //customer_bonus_log cbl = new customer_bonus_log();
                db.Database.ExecuteSqlCommand("update checkall set id_partner="+ id_partner + ", partner=N'" + partner + "', waranty_name=N'" + waranty_name + "',waranty_phone=N'" + waranty_phone + "', waranty_address=N'" + waranty_address + "' where id=" + id);
                field.Add("waranty_phone", waranty_phone);
                return Api("success", field, "Cập nhật thông tin bảo hành thành công!");
            }
            catch (Exception ex)
            {
                field.Add("status", "");
                return Api("error", field, "Lỗi sql: " + ex.ToString());
            }

        }
        //Hàm này trả về danh sách trúng của user
        public string getListWinningOfUser(long user_id,string keyword)
        {
            Dictionary<string, string> field = new Dictionary<string, string>();
            try
            {
                if (keyword == null) keyword = "";
                var p = db.winning_log.Where(o => o.user_id == user_id && (o.product_name.Contains(keyword) || o.partner.Contains(keyword) || o.winning_name.Contains(keyword) || o.company.Contains(keyword))).OrderByDescending(o => o.id).ToList();
                field.Add("list", JsonConvert.SerializeObject(p));
                return ApiArray("success", field, "Danh sách trúng thưởng của user này");
            }
            catch (Exception ex)
            {
                field.Add("list", "[]");
                return Api("error", field, "Lỗi sql: " + ex.ToString());
            }
        }
        // Hàm này trả về các khách hàng quét mới nhất từ thời gian fromdate đến thời gian todate
        public string getListCustomer(int company_code,DateTime fromdate, DateTime todate,string key)
        {
            MD5 md5Hash = MD5.Create();
            int newPass = company_code * 2 + 1;
            string hash = Config.GetMd5Hash(md5Hash, newPass.ToString());            
            Dictionary<string, string> field = new Dictionary<string, string>();
            if (hash != key)
            {
                field.Add("list", "[]");
                return Api("error", field, "Bạn gửi sai khóa");
            }
            try
            {
                var p = (from q in db.checkalls where q.code_company == company_code && q.date_time >= fromdate && q.date_time <= todate
                         select new
                         {
                             id=q.id,
                             TenDoanhNghiep=q.company,
                             TenNhaPhanPhoi=q.partner,
                             MaSanPham=q.guid,
                             SoThuTu=q.stt,
                             KichHoatNgay=q.date_time,
                             Email=q.user_email,
                             Phone=q.user_phone,
                             DiaDiem=q.address,
                             TinhThanh=q.province,
                             AnhQrCode1=q.guid,
                             AnhQrCode2=q.stt*13+27

                         }
                         ).OrderByDescending(o => o.id).ToList();
                //field.Add("list", JsonConvert.SerializeObject(p));
                return JsonConvert.SerializeObject(p);// ApiArray("success", field, "Danh sách khách hàng quét qr code");
            }
            catch (Exception ex)
            {
                field.Add("list", "[]");
                return Api("error", field, "Lỗi sql: " + ex.ToString());
            }
        }

        //Hàm này ghi lại nhật ký đổi điểm và trả về total điểm hiện tại của user sau khi đổi điểm, mã code voucher này để khi đến chỗ sử dụng voucher như rạp chiếu phim, nhà hàng...
        //Thì đưa code này ra , sẽ có 1 hàm là lấy mã code voucher này khi đến nhà hàng đưa cho thu ngân, họ sẽ giảm giá dựa vào code khách cung cấp
        //Code này do client app lưu ở đâu đó hoặc khi đến nơi bật 3g gọi đến 1 hàm api khác để lấy dựa trên voucher_id này
        //Hàm này cũng trừ điểm của user đi số điểm tương ứng là points
        [HttpPost]
        public string transferVoucher(long user_id, long voucher_id, int points, double lon, double lat, string address, double? key)
        {
            Dictionary<string, string> field = new Dictionary<string, string>();
            try
            {
                if (key == null || !getKeyApi(key))
                {
                    field.Add("TotalMilliseconds", (DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds.ToString());
                    return Api("failed", field, "Ngày giờ ở máy bị sai số quá 10 phút nên ứng dụng không chạy được, Bạn cần chỉnh lại thời gian ở điện thoại!");
                }

                if (db.customers.Find(user_id).points < points)
                {
                    field.Add("total", "");
                    return Api("failed", field, "Không đủ điểm để đổi voucher này");
                }
                long? codeVc = user_id * 1000 + voucher_id + Config.datetimeid();
                db.Database.ExecuteSqlCommand("update customers set points=points-" + points + " where id=" + user_id);
                var us = db.customers.Find(user_id);
                voucher_log vl=new voucher_log();
                vl.address = address;
                vl.code = codeVc;
                vl.date_time = DateTime.Now;
                vl.lat = lat;
                vl.lon = lon;
                vl.points = points;
                vl.user_email = us.email;
                vl.user_id = user_id;
                vl.user_name = us.name;
                vl.user_phone = us.phone;
                vl.voucher_id = voucher_id;
                vl.voucher_name = db.voucher_points.Find(voucher_id).name;
                db.voucher_log.Add(vl);
                db.SaveChanges();
                db.Database.ExecuteSqlCommand("update voucher_points set quantity=quantity-1 where id=" + voucher_id);
                field.Add("total", us.points.ToString());
                field.Add("code", codeVc.ToString());
                return Api("success", field, "User đã mua voucher này thành công và trả về code sử dụng!");
            }
            catch (Exception ex)
            {
                field.Add("total", "");   
                return Api("error", field, "Lỗi sql: " + ex.ToString());
            }
        }

        [HttpPost]
        //Hàm này trả về định nghĩa các điểm thưởng
        public string getConfigPoints()
        {
            Dictionary<string, string> field = new Dictionary<string, string>();
            try
            {
                var p = db.config_bonus_point.Find(1);
                field.Add("check_point", p.check_point.ToString());
                field.Add("share_point", p.share_point.ToString());
                field.Add("ref_point", p.ref_point.ToString());
                field.Add("time_point", p.time_point.ToString());
                return Api("success", field, "Định nghĩa các điểm thưởng!");
            }
            catch (Exception ex)
            {
                field.Add("check_point", "");
                field.Add("share_point", "");
                field.Add("ref_point", "");
                field.Add("time_point", "");
                return Api("error", field, "Lỗi sql: " + ex.ToString());
            }
        }
        [HttpPost]
        //Hàm này trả về định nghĩa các điểm thưởng
        public string bonusPoints(long user_id, int type, int points)
        {
            Dictionary<string, string> field = new Dictionary<string, string>();
            try
            {
                customer_bonus_log cbl = new customer_bonus_log();
                db.Database.ExecuteSqlCommand("update customers set points=points+" + points + " where id=" + user_id);
                var user = db.customers.Find(user_id);
                if (type == 1)
                {
                    cbl.actions = "Share ứng dụng lên facebook";
                }
                else
                {
                    cbl.actions = "Được thưởng điểm theo tháng";
                }
                cbl.date_time = DateTime.Now;
                cbl.points = points;
                cbl.user_email = user.email;
                cbl.user_id = user_id;
                cbl.user_name = user.name;
                cbl.user_phone = user.phone;
                db.customer_bonus_log.Add(cbl);
                db.SaveChanges();
                field.Add("total", user.points.ToString());               
                return Api("success", field, "Cập nhật hành động thành công, điểm hiện tại là!");
            }
            catch (Exception ex)
            {
                field.Add("total", "");    
                return Api("error", field, "Lỗi sql: " + ex.ToString());
            }
        }
        public bool isNumber(string val)
        {
            try
            {
                int num;
                if (int.TryParse(val, out num))
                {
                    return true;
                }
                else return false;
            }
            catch(Exception ex)
            {
                return false;
            }
        }
        //Hàm này làm 3 việc và trả về 3 trường tương ứng là active,location và point (gửi kèm cả trường total nữa)
        //1. Kích hoạt sản phẩm với mã sản phẩm là guid, nếu sản phẩm này đã kích hoạt 1 lần rồi cũng báo đã kích hoạt
        //2. Báo địa điểm với mã sản phẩm là guid, nếu đã báo địa điểm rồi cũng báo là đã báo
        //3. Báo tích điểm với mã sản phẩm là guid, nếu đã tích điểm rồi cũng báo, hiện số điểm sau khi tích điểm, gửi kèm cả trường total là số điểm hiện thời sau khi tích điểm
        //Nếu lỗi sql trả về rỗng các trường và lỗi chi tiết
        [HttpPost]
        public string check(string guid, long? user_id, double lon, double lat, string address, double? key, int? points, int? os)
        {
            Dictionary<string, string> field = new Dictionary<string, string>();
            try
            {
                if (key == null || !getKeyApi(key))
                {
                    field.Add("TotalMilliseconds", (DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds.ToString());
                    return Api("failed", field, "Ngày giờ ở máy bị sai số quá 10 phút nên ứng dụng không chạy được, Bạn cần chỉnh lại thời gian ở điện thoại!");
                }
                string label = "";
                string active = "";
                string location = "";
                string point = "";
                string image = "";
                int? code_company = 0;
                string company = "";
                string company_address = "";
                string company_phone = "";
                string company_email = "";
                string company_web = "";
                string partner = "";
                int? id_partner = 0;
                long? winning_id = 0;
                long? winning_sn = 0;
                int? w_from_stt = 0;
                int? w_to_stt = 0;
                long? stt = 0;
                long NUMBER = 27;//Mặc định qr code cũ thì stt là 0 vì chỉ có GUID
                int? waranty_year = 1;
                string waranty_text = "";
                string waranty_link_web = "";
                string product_info="";
                string product_code = "";
                string product_date = "";
                string buy_more = "";
                int? id_config_app = 0;
                //Kiểm tra QR code mới hay cũ
                if (guid != null && guid.Contains("-"))
                {
                    //Tách ra làm 2 guid
                    string[] tempGuid = guid.Split('-');
                    string guid1 = guid.Substring(0, guid.LastIndexOf("-"));
                    string guid2 = tempGuid[tempGuid.Length - 1];
                    if (isNumber(guid2))
                    {
                        guid = guid1;
                        long.TryParse(guid2, out NUMBER);
                    }
                }

                //Kiểm tra xem guid này có config cho cty nào không?
                if (db.qrcodes.Any(o => o.guid == guid && o.status == 0) && ((NUMBER - 27) % 13 == 0))
                {
                    long temp_stt = (NUMBER - 27) / 13;
                    //if (temp_stt == 0) temp_stt = 1;
                    var rs = db.qrcodes.Where(o => o.guid == guid && o.status == 0).OrderBy(o => o.from_stt).FirstOrDefault();
                    if (temp_stt > 0)
                    {
                        rs = db.qrcodes.Where(o => o.guid == guid && o.status == 0 && o.from_stt <= temp_stt && o.to_stt >= temp_stt).OrderBy(o => o.from_stt).FirstOrDefault();
                    }
                    code_company = rs.code_company;
                    company = rs.company;
                    partner = db.partners.Find((int)rs.id_partner).name;//rs.partner;
                    id_partner = rs.id_partner;

                    var cpnif = db.companies.Where(o => o.code==rs.code_company).FirstOrDefault();
                    company_address = cpnif.address;
                    company_phone = cpnif.phone_contact;
                    company_email = cpnif.email_contact;
                    company_web = cpnif.web;
                    stt = rs.stt;
                    winning_id = rs.winning_id;
                    w_from_stt = rs.w_from_stt;
                    w_to_stt = rs.w_to_stt;
                    id_config_app = rs.id_config_app;
                    if ((NUMBER - 27) / 13 != 0) stt = (NUMBER - 27) / 13;
                    //winning_id = rs.winning_id!=null?rs.winning_id:0;
                    if (db.config_app.Any(o => o.code_company == code_company))
                    {
                        if (db.company_configapp_qrcode_link.Any(o => o.code_company == code_company && o.from_sn<= stt && o.to_sn>=stt))
                        {
                            var fcfapp = db.company_configapp_qrcode_link.Where(o => o.code_company == code_company && o.from_sn <= stt && o.to_sn >= stt).OrderBy(o=>o.id).FirstOrDefault();
                            int fcfapp_id_config_app = (int)fcfapp.id_config_app;
                            var info = db.config_app.Where(o => o.id==fcfapp_id_config_app).OrderBy(o => o.id).FirstOrDefault();
                            //if (id_config_app != null)
                            //{
                            //    info = db.config_app.Where(o => o.id == id_config_app).FirstOrDefault();
                            //}
                            image = info.image;
                            label = info.text_in_qr_code;
                            active = "Kích hoạt thành công, Sản phẩm được kích hoạt bảo hành vào thời điểm {NGAYTHANG}";//info.text_in_active;
                            location = "Sản phẩm được quét tại địa điểm {DIADIEM}";// info.text_in_location;
                            point = "Bạn được tích số điểm mới là {DIEM}";//info.text_in_point;
                            waranty_year = info.waranty_year;
                            waranty_text = info.waranty_text;
                            waranty_link_web = info.waranty_link_web;
                            buy_more = info.buy_more;
                            product_info = info.product_info;
                            product_code = info.product_code;
                            product_date = info.product_date;
                            id_config_app = fcfapp.id_config_app;                            
                        }
                        else
                        {
                            var info = db.config_app.Where(o => o.code_company == code_company).OrderBy(o => o.id).FirstOrDefault();
                            //if (id_config_app != null)
                            //{
                            //    info = db.config_app.Where(o => o.id == id_config_app).FirstOrDefault();
                            //}
                            image = info.image;
                            label = info.text_in_qr_code;
                            active = "Kích hoạt thành công, Sản phẩm được kích hoạt bảo hành vào thời điểm {NGAYTHANG}";//info.text_in_active;
                            location = "Sản phẩm được quét tại địa điểm {DIADIEM}";// info.text_in_location;
                            point = "Bạn được tích số điểm mới là {DIEM}";//info.text_in_point;
                            waranty_year = info.waranty_year;
                            waranty_text = info.waranty_text;
                            waranty_link_web = info.waranty_link_web;
                            buy_more = info.buy_more;
                            product_info = info.product_info;
                            product_code = info.product_code;
                            product_date = info.product_date;
                            id_config_app = info.id;
                        }
                    }
                    else
                    {
                        var info = db.config_app.Where(o => o.id == 1).FirstOrDefault();
                        image = info.image;
                        label = info.text_in_qr_code;
                        active = info.text_in_active;
                        location = info.text_in_location;
                        point = info.text_in_point;
                        waranty_year = info.waranty_year;
                        waranty_text = info.waranty_text;
                        waranty_link_web = info.waranty_link_web;
                        buy_more = info.buy_more;
                        product_info = info.product_info;
                        product_code = info.product_code;
                        product_date = info.product_date;
                        id_config_app = info.id;
                    }
                }
                else
                {
                    field.Add("info", "Sản phẩm này không được cấp mã tem của An Hà");
                    field.Add("active", "Sản phẩm này không được kích hoạt");
                    field.Add("location", "Tại địa điểm " + address);
                    field.Add("point", "Sản phẩm này không được tính tích điểm");
                    field.Add("user_id_scaned", "");
                    field.Add("image", "");
                    field.Add("waranty_year", "");
                    field.Add("waranty_text", "");
                    field.Add("waranty_link_web", "");
                    field.Add("winning_id", "0");
                    field.Add("winning_sn", "0");
                    field.Add("product_info", "");
                    field.Add("product_code", "");
                    field.Add("product_date", "");
                    field.Add("buy_more", "");
                    field.Add("total", "...");
                    field.Add("company", "");
                    field.Add("company_phone", "");
                    field.Add("company_email", "");
                    field.Add("company_web", "");
                    field.Add("company_address", "");
                    field.Add("partner", "");
                    return Api("success", field, "Gửi thông tin về server thành công!");
                    //var info = db.config_app.Where(o => o.id==1).FirstOrDefault();
                    //label = info.text_in_qr_code;
                    //active = info.text_in_active;
                    //location = info.text_in_location;
                    //point = info.text_in_point;
                }
                DateTime dtn = DateTime.Now;
                active = active.Replace("{NGAYTHANG}", Config.formatDateTime(dtn));
                location = location.Replace("{DIADIEM}", address);
                NUMBER = (NUMBER - 27) / 13;
                // Kết hợp duy nhất trong 1 bảng
                if (db.checkalls.Any(o => o.guid == guid && (o.stt == NUMBER || o.stt == stt)))// || o.stt== stt
                {
                    var cka = db.checkalls.Where(o => o.guid == guid && (o.stt == NUMBER || o.stt == stt)).FirstOrDefault();
                    string dtfm = Config.formatDateTime(cka.date_time);
                    if (product_info != null && product_info != "") product_info = product_info.Replace("\"", "\\\"");
                    if (buy_more != null && buy_more != "") buy_more = buy_more.Replace("\"", "\\\"");
                    if (waranty_text != null && waranty_text != "") waranty_text = waranty_text.Replace("\"", "\\\"");
                    if (os == 0)
                    {
                        product_info = cka.id.ToString();
                        buy_more = cka.id.ToString();
                        waranty_text = cka.id.ToString();
                    }

                    field.Add("info", label);
                    field.Add("image", image);
                    field.Add("active", "Sản phẩm này đã kích hoạt trước đó rồi. Vào thời gian " + dtfm);
                    field.Add("location", "Sản phẩm này đã báo địa điểm " + cka.address + " trước đó tại thời điểm " + dtfm);
                    field.Add("user_id_scaned", cka.user_id.ToString());
                    field.Add("user_phone_scaned", cka.user_phone);
                    field.Add("user_location_scaned", cka.address);
                    field.Add("user_date_scaned", cka.date_time.ToString());
                    int? count = db.customers.Find(user_id).points;
                    field.Add("point", "Sản phẩm này đã được tích điểm vào lúc " + dtfm + ", bạn không thể tích thêm điểm");
                    field.Add("total", count.ToString());
                    field.Add("waranty_year", waranty_year.ToString());
                    field.Add("waranty_text", waranty_text);
                    field.Add("waranty_link_web", waranty_link_web);
                    field.Add("winning_id", "0");
                    field.Add("winning_sn", "0");
                    field.Add("product_info", product_info);
                    field.Add("product_code", product_code);
                    field.Add("product_date", product_date);
                    field.Add("buy_more", buy_more);
                    field.Add("company", company);
                    field.Add("company_phone", company_phone);
                    field.Add("company_email", company_email);
                    field.Add("company_address", company_address);
                    field.Add("company_web", company_web);
                    field.Add("partner", partner);
                }
                else
                {
                    if (points==null) {
                        try{
                            points=db.config_bonus_point.Find(1).check_point;
                        }catch{
                            points=1;
                        }
                    }
                    //Quét thành công
                    db.Database.ExecuteSqlCommand("update customers set points=points+" + points + " where id=" + user_id);                    
                    customer ctm = db.customers.Find(user_id);
                    checkall cka = new checkall();
                    cka.address = address;
                    cka.date_time = dtn;
                    cka.guid = guid;
                    cka.lat = lat;
                    cka.lon = lon;
                    cka.os = os;                    
                    cka.points = points;
                    cka.user_id = user_id;
                    cka.user_email = ctm.email;
                    cka.user_name = ctm.name;
                    cka.user_phone = ctm.phone;
                    cka.province = Config.getProvince(address);
                    cka.company = company;
                    cka.code_company = code_company;
                    cka.product_text = label;
                    cka.id_partner = id_partner;
                    cka.image = image;
                    cka.partner = partner;
                    cka.waranty_link_web = waranty_link_web;
                    cka.waranty_text = waranty_text;
                    cka.waranty_year = waranty_year;
                    cka.id_config_app= id_config_app;
                    if (NUMBER != 0)
                    {
                        cka.stt = NUMBER;//stt;
                    }else
                    {
                        cka.stt = stt;//stt;
                    }
                    winning_sn = cka.stt;
                    db.checkalls.Add(cka);
                    db.SaveChanges();
                    //Ghi nhật ký                                 
                    customer_bonus_log cbl = new customer_bonus_log();   
                    cbl.actions = "Được thưởng điểm khi quét qr code "+guid;                   
                    cbl.date_time = DateTime.Now;
                    cbl.points = points;
                    cbl.user_id = user_id;
                    cbl.user_email = ctm.email;
                    cbl.user_name = ctm.name;
                    cbl.user_phone = ctm.phone;                   
                    db.customer_bonus_log.Add(cbl);
                    db.SaveChanges();
                    //Quyết định có trúng thưởng hay không
                    try
                    {
                        DateTime wdt=DateTime.Now;
                        if (db.winnings.Any(o => o.code_company == code_company && o.from_date <= wdt && o.to_date>=wdt && o.quantity>0))
                        {
                            //Nếu có trúng thưởng cho số sn này thì lấy, còn không thì mặc định
                            long? win_sn = 0;
                            if (NUMBER != 0)
                            {
                                win_sn = NUMBER;//stt;
                            }
                            else
                            {
                                win_sn = stt;//stt;
                            }
                            //winning_sn = win_sn;
                            //Có những trúng thưởng cho những khoảng id đặc biệt
                            if (winning_id != null && win_sn >= w_from_stt && win_sn <= w_to_stt && db.winnings.Any(o => o.id == winning_id && o.quantity>0))
                            {
                                var wnbn = db.winnings.Find(winning_id);
                                if (wnbn != null)
                                {
                                    winning_id = wnbn.id;
                                    winning_log wl = new winning_log();
                                    wl.address = address;
                                    wl.date_time = DateTime.Now;
                                    wl.lat = lat;
                                    wl.lon = lon;
                                    wl.money = wnbn.money;
                                    wl.qrcode = guid;
                                    wl.user_id = user_id;
                                    wl.user_email = ctm.email;
                                    wl.user_name = ctm.name;
                                    wl.user_phone = ctm.phone;
                                    wl.winning_id = winning_id;
                                    wl.winning_name = wnbn.name;
                                    if (NUMBER != 0)
                                    {
                                        wl.sn = NUMBER;//stt;
                                    }
                                    else
                                    {
                                        wl.sn = stt;//stt;
                                    }
                                    wl.company = company;
                                    wl.partner = partner;
                                    wl.id_partner = id_partner;
                                    wl.code_company = code_company;                           
                                    wl.product_name = label;
                                    wl.is_received = 0;
                                    db.winning_log.Add(wl);
                                    db.SaveChanges();
                                    db.Database.ExecuteSqlCommand("update winning set quantity=quantity-1 where id=" + winning_id);
                                }
                            }
                            else {
                                //Mặc định trúng thưởng
                                var wnbn = db.winnings.Where(o => o.code_company == code_company && o.from_date <= wdt && o.to_date >= wdt && o.quantity > 0).OrderBy(o => o.money).FirstOrDefault();
                                if (wnbn != null)
                                {
                                    winning_id = wnbn.id;
                                    winning_log wl = new winning_log();
                                    wl.address = address;
                                    wl.date_time = DateTime.Now;
                                    wl.lat = lat;
                                    wl.lon = lon;
                                    wl.money = wnbn.money;
                                    wl.qrcode = guid;
                                    wl.user_id = user_id;
                                    wl.user_email = ctm.email;
                                    wl.user_name = ctm.name;
                                    wl.user_phone = ctm.phone;
                                    wl.winning_id = winning_id;
                                    wl.winning_name = wnbn.name;
                                    if (NUMBER != 0)
                                    {
                                        wl.sn = NUMBER;//stt;
                                    }
                                    else
                                    {
                                        wl.sn = stt;//stt;
                                    }
                                    wl.company = company;
                                    wl.partner = partner;
                                    wl.id_partner = id_partner;
                                    wl.code_company = code_company;
                                    wl.product_name = label;
                                    wl.is_received = 0;
                                    db.winning_log.Add(wl);
                                    db.SaveChanges();
                                    db.Database.ExecuteSqlCommand("update winning set quantity=quantity-1 where id=" + winning_id);
                                }
                            }
                        }
                    }
                    catch
                    {
                        winning_id = null;
                    }
                    int? count = ctm.points;
                    //label = label;// + ". Nhà phân phối " + partner;
                    field.Add("info", label);
                    field.Add("image", image);
                    field.Add("active", active);
                    if (address == null || address == "")
                    {
                        address = RetrieveFormatedAddress(lat, lon);
                        if (address != null && address != "")
                            field.Add("location", location);
                        else
                            field.Add("location", location + " - (Địa chỉ chưa lấy được)");
                    }
                    point = point.Replace("{DIEM}", count.ToString());
                    if (product_info!=null && product_info!="") product_info = product_info.Replace("\"", "\\\"");
                    if (buy_more != null && buy_more != "") buy_more = buy_more.Replace("\"", "\\\"");
                    if (waranty_text != null && waranty_text != "") waranty_text = waranty_text.Replace("\"", "\\\"");
                    if (os == 0)
                    {
                        product_info = cka.id.ToString();// convertToAscii(product_info);
                        buy_more = cka.id.ToString();// convertToAscii(buy_more);
                        waranty_text = cka.id.ToString();// convertToAscii(waranty_text);
                    }
                    field.Add("point", point);
                    field.Add("user_id_scaned", "");
                    field.Add("total", count.ToString());
                    field.Add("winning_id", winning_id.ToString());
                    field.Add("winning_sn", winning_sn.ToString());
                    field.Add("waranty_year", waranty_year.ToString());
                    field.Add("waranty_text", waranty_text);
                    field.Add("waranty_link_web", waranty_link_web);
                    field.Add("product_info", product_info);
                    field.Add("product_code", product_code);
                    field.Add("product_date", product_date);
                    field.Add("buy_more", buy_more);
                    field.Add("company", company);
                    field.Add("company_phone", company_phone);
                    field.Add("company_email", company_email);
                    field.Add("company_address", company_address);
                    field.Add("company_web", company_web);
                    field.Add("partner", partner);
                    
                }
                return Api("success", field, "Gửi thông tin về server thành công!");
            }
            catch (Exception ex)
            {
                field.Clear();
                field.Add("info", "");
                field.Add("active","");
                field.Add("image", "");
                field.Add("location", "");
                field.Add("point","");
                field.Add("total","");
                field.Add("winning_id", "0");
                field.Add("winning_sn", "0");
                field.Add("user_id_scaned", "");
                field.Add("waranty_year", "");
                field.Add("waranty_text", "");
                field.Add("waranty_link_web", "");
                field.Add("product_info", "");
                field.Add("product_code", "");
                field.Add("product_date", "");
                field.Add("buy_more", "");
                field.Add("company", "");
                field.Add("company_phone", "");
                field.Add("company_email", "");
                field.Add("company_address", "");
                field.Add("company_web", "");
                field.Add("partner", "");
                return Api("error", field, "Cập nhật lỗi sql: " + ex.ToString());
            }
        }
        public ActionResult viewBuyMoreAfterCheck(long id)
        {
            var p = db.checkalls.Find(id);
            try { 
                int? id_config_app = db.company_configapp_qrcode_link.Where(o => o.code_company == p.code_company && o.from_sn <= p.stt && o.to_sn >= p.stt).OrderBy(o => o.id).FirstOrDefault().id_config_app;
                if (id_config_app != null)
                {
                    var p2 = db.config_app.Find(id_config_app);
                    ViewBag.buy_more = p2.buy_more;
                    return View();
                }else
                {
                    var p2 = db.config_app.Where(o => o.code_company == p.code_company).OrderBy(o => o.id).FirstOrDefault();
                    ViewBag.buy_more = p2.buy_more;
                    return View();
                }
            }
            catch
            {
                ViewBag.buy_more = "";
            }
            return View();        
        }
        public ActionResult viewWarantyTextAfterCheck(long id)
        {
            var p = db.checkalls.Find(id);
            try
            {
                int? id_config_app = db.company_configapp_qrcode_link.Where(o => o.code_company == p.code_company && o.from_sn <= p.stt && o.to_sn >= p.stt).OrderBy(o => o.id).FirstOrDefault().id_config_app;
                if (id_config_app != null)
                {
                    var p2 = db.config_app.Find(id_config_app);
                    ViewBag.waranty_text = p2.waranty_text;
                    return View();
                }
                else
                {
                    var p2 = db.config_app.Where(o => o.code_company == p.code_company).OrderBy(o => o.id).FirstOrDefault();
                    ViewBag.waranty_text = p2.waranty_text;
                    return View();
                }
            }
            catch
            {
                ViewBag.waranty_text = "";
            }
            return View();
        }
        public ActionResult viewProductInfoAfterCheck(long id)
        {
            var p = db.checkalls.Find(id);
            try
            {
                int? id_config_app = db.company_configapp_qrcode_link.Where(o => o.code_company == p.code_company && o.from_sn <= p.stt && o.to_sn >= p.stt).OrderBy(o => o.id).FirstOrDefault().id_config_app;
                if (id_config_app != null)
                {
                    var p2 = db.config_app.Find(id_config_app);
                    ViewBag.product_info = p2.product_info;
                    return View();
                }
                else
                {
                    var p2 = db.config_app.Where(o => o.code_company == p.code_company).OrderBy(o => o.id).FirstOrDefault();
                    ViewBag.product_info = p2.product_info;
                    return View();
                }
            }
            catch
            {
                ViewBag.product_info = "";
            }
            return View();
        }
        public string getProductInfoOfUserCheck(long id)
        {
            Dictionary<string, string> field = new Dictionary<string, string>();
            var p = db.checkalls.Find(id);
            try
            {
                int? id_config_app = db.company_configapp_qrcode_link.Where(o => o.code_company == p.code_company && o.from_sn <= p.stt && o.to_sn >= p.stt).OrderBy(o => o.id).FirstOrDefault().id_config_app;
                if (id_config_app != null)
                {
                    var p2 = db.config_app.Find(id_config_app);
                    field.Add("product_info", p2.product_info.Replace("\"", "\\\""));                    
                    return Api("success", field, "Thông tin sản phẩm");
                }
                else
                {
                    var p2 = db.config_app.Where(o => o.code_company == p.code_company).OrderBy(o => o.id).FirstOrDefault();
                    field.Add("product_info", p2.product_info.Replace("\"", "\\\""));
                    return Api("success", field, "Thông tin sản phẩm");
                }
            }
            catch(Exception ex)
            {
                field.Add("product_info", "");
                return Api("error", field, "lỗi sql: " + ex.ToString());
            }
        }
        //Id của dòng bảo hành, xem list bảo hành, click vào dòng nào đó thì ra chi tiết, anh sẽ trả về đủ các trường cho anh em cần hiện
        public string getDetailCheckOfUser(long id,int? os)
        {
            Dictionary<string, string> field = new Dictionary<string, string>();
            string product_info = "0";
            string product_code = "";
            string product_date = "";
            string buy_more = "0";
            string waranty_text = "0";
            string is_modifiable = "0";
            string company_address = "";
            string company_phone = "";
            string company_email = "";
            string company_web = "";
            var p = db.checkalls.Find(id);
            //Lấy ra vài thông tin
            try
            {
                long? stt = p.stt;
                int? code_company = p.code_company;
                int? config_id = 0;
                if (!db.company_configapp_qrcode_link.Any(o => o.code_company == code_company && o.from_sn <= stt && o.to_sn >= stt))
                {
                    var q = db.qrcodes.Where(o => o.code_company == code_company && o.from_stt <= stt && o.to_stt >= stt).OrderBy(o => o.id).FirstOrDefault();
                    if (q != null)
                    {
                        config_id = q.id_config_app;
                    }
                }
                else
                {
                    var q = db.company_configapp_qrcode_link.Where(o => o.code_company == code_company && o.from_sn <= stt && o.to_sn >= stt).OrderBy(o => o.id).FirstOrDefault();
                    if (q != null)
                    {
                        config_id = q.id_config_app;
                    }
                }
                var cid = db.config_app.Find(config_id);

                if (cid != null)
                {
                    buy_more = cid.buy_more.Replace("\"", "\\\"");
                    product_info=cid.product_info.Replace("\"", "\\\"");
                    waranty_text= cid.waranty_text.Replace("\"", "\\\"");
                    product_code = cid.product_code;
                    product_date = cid.product_date;
                }
                var cpnif = db.companies.Where(o => o.code == code_company).FirstOrDefault();
                company_address = cpnif.address;
                company_phone = cpnif.phone_contact;
                company_email = cpnif.email_contact;
                company_web = cpnif.web;
                is_modifiable = cpnif.modifiable.ToString();

            }
            catch
            {

            }
            try
            {
                
                field.Add("product_text", p.product_text);
                field.Add("image", p.image);
                field.Add("address", p.address);
                field.Add("user_name", p.user_name);
                field.Add("user_email", p.user_email);
                field.Add("user_phone", p.user_phone);               
                field.Add("date_time", p.date_time.ToString());
                if (os == 0)
                {
                    product_info = p.id.ToString();// convertToAscii(product_info);
                    buy_more = p.id.ToString();// convertToAscii(buy_more);
                    waranty_text = p.id.ToString();// convertToAscii(waranty_text);
                }
                field.Add("waranty_year", p.waranty_year.ToString());
                field.Add("waranty_text", waranty_text);
                field.Add("waranty_link_web", p.waranty_link_web);
                field.Add("product_info", product_info);
                field.Add("product_code", product_code);
                field.Add("sn", p.stt.ToString());
                field.Add("product_date", product_date);
                field.Add("buy_more", buy_more);
                field.Add("company", p.company);
                field.Add("is_modifiable", is_modifiable);
                field.Add("company_address", company_address);
                field.Add("company_phone", company_phone);
                field.Add("company_email", company_email);
                field.Add("company_web", company_web);
                field.Add("partner", p.partner);
                return Api("success", field, "Thông tin sản phẩm đã quét");
            }
            catch (Exception ex)
            {
                field.Add("product_text", "");
                return Api("error", field, "lỗi sql: " + ex.ToString());
            }
        }
        //Id này là chi tiết của dòng đã quét
        public ActionResult viewProductInfoOfUserCheckIOS(long id)
        {
            var p = db.checkalls.Find(id);
            try
            {
                int? id_config_app = db.company_configapp_qrcode_link.Where(o => o.code_company == p.code_company && o.from_sn <= p.stt && o.to_sn >= p.stt).OrderBy(o => o.id).FirstOrDefault().id_config_app;
                if (id_config_app != null)
                {
                    var p2 = db.config_app.Find(id_config_app);
                    ViewBag.product_info = p2.product_info;
                    return View();
                }
                else
                {
                    var p2 = db.config_app.Where(o => o.code_company == p.code_company).OrderBy(o => o.id).FirstOrDefault();
                    ViewBag.product_info = p2.product_info;
                    return View();
                }
            }
            catch
            {
                return View();
            }
        }
        public string convertToAscii(string unc)
        {
            //string unicodeString = "<p>Xin chào các bạn nhé</p><table style=\"table\"></table>";
            var jsonSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            return jsonSerializer.Serialize(unc);
            //return System.Text.Encoding.Unicode.GetBytes(unicodeString.ToString()).Aggregate("", (agg, val) => agg + val.ToString("X2"));
            //string unicodeString = "<p>Xin chào các bạn nhé</p><table style=\"table\"></table>";

            //// Create two different encodings.
            //Encoding ascii = Encoding.;
            //Encoding unicode = Encoding.Unicode;

            //// Convert the string into a byte array.
            //byte[] unicodeBytes = unicode.GetBytes(unicodeString);

            //// Perform the conversion from one encoding to the other.
            //byte[] asciiBytes = Encoding.Convert(unicode, ascii, unicodeBytes);

            //// Convert the new byte[] into a char[] and then into a string.
            //char[] asciiChars = new char[ascii.GetCharCount(asciiBytes, 0, asciiBytes.Length)];
            //ascii.GetChars(asciiBytes, 0, asciiBytes.Length, asciiChars, 0);
            //string asciiString = new string(asciiChars);
            //return asciiString;

        }
        public string RetrieveFormatedAddress(double lat, double lon)
        {
            return "";
            //string requestUri = "https://maps.googleapis.com/maps/api/geocode/json?latlng="+lat+","+lon+"&key=AIzaSyDLPSKQ4QV4xGiQjnZDUecx-UEr3D0QePY";

            //using (WebClient wc = new WebClient())
            //{
            //    string result = wc.DownloadString(requestUri);
            //    //var xmlElm = XElement.Parse(result);
            //    //var status = (from elm in xmlElm.Descendants()
            //    //              where
            //    //                  elm.Name == "status"
            //    //              select elm).FirstOrDefault();
            //    var adr = JsonConvert.DeserializeObject(result);
            //    //if (status.Value.ToLower() == "ok")
            //    //{
            //    //    var res = (from elm in xmlElm.Descendants()
            //    //               where
            //    //                   elm.Name == "formatted_address"
            //    //               select elm).FirstOrDefault();
            //    //    requestUri = res.Value;
            //    //}
            //    //else requestUri = "";
            //    return "";
            //}
            //return "";
        }
        public ActionResult Index(string code)
        {
            
            return View();
        }
        public string getInfoUserCode(long? code)
        {
            try {
                if (db.voucher_log.Any(o => o.code == code)) { 
                    var iff=db.voucher_log.Where(o=>o.code==code).FirstOrDefault();
                    return "Xác nhận khách hàng có số điện thoại " + iff.user_phone + ", quét code này tại thời gian " + String.Format("{0:dd/MM/yyyy hh:mm tt}", iff.date_time.Value) + ", tại địa chỉ " + iff.address;
                }
                else return "Không tìm thấy khách hàng này!";
            }
            catch
            {
                return "";
            }
        }
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }
        public ActionResult Reset(string message)
        {
            ViewBag.Message = message;

            return View();
        }
        public ActionResult ConfirmReset1(string message)
        {
            ViewBag.Message = message;

            return View();
        }
        public ActionResult ConfirmReset2(string email,string code)
        {

            if (!db.customers.Any(o => o.email==email && o.pass == code))
            {
                return RedirectToAction("Reset", "Home", new { message = "Không tìm thấy email này trong dữ liệu, vui lòng điền email khác mà bạn dùng để đăng ký" });
            }
            ViewBag.code = code;
            ViewBag.email = email;            
            return View();
        }
        [HttpPost]
        public ActionResult ConfirmReset3(string email,string code, string pass, string pass2)
        {
            if (!db.customers.Any(o => o.email==email && o.pass == code))
            {
                return RedirectToAction("Reset", "Home", new { message = "Không tìm thấy email này trong dữ liệu, vui lòng điền email khác mà bạn dùng để đăng ký" });
            }
             MD5 md5Hash = MD5.Create();
             string hash = Config.GetMd5Hash(md5Hash, pass);
             db.Database.ExecuteSqlCommand("update customers set pass=N'" + hash + "' where email=N'" + email + "' and pass=N'" + code + "'");
             return RedirectToAction("ConfirmReset4", "Home", new { message = "Đổi mật khẩu thành công, xin dùng số điện thoại bạn đã đăng ký đăng nhập cùng mật khẩu mới này" });
        }
        public ActionResult ConfirmReset4(string message)
        {
            ViewBag.Message = message;
            return View();
        }
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        //Upload avatar cho user có id là user_id cập nhật ảnh, gửi lên ảnh dạng base64, server tự decode lại
        //trả về đường dẫn ảnh trong trường image nếu thành công, trả về rỗng nếu lỗi
        //[HttpPost]
        //public string updateImageUser(Dictionary<string, string> sjson)//int user_id, 
        //{
        //    Dictionary<string, string> field = new Dictionary<string, string>();
        //    string s_user_id = "-1";
        //    string s_base64 = "";
        //    dynamic stuff1 = JsonConvert.SerializeObject(sjson);
        //    s_user_id = stuff1[0].user_id;
        //    s_base64= stuff1[0].base64;
        //    int user_id = int.Parse(s_user_id);
        //    try
        //    {
        //        String path = Server.MapPath("~//images//customer"); //Path

        //        //Check if directory exist
        //        if (!System.IO.Directory.Exists(path))
        //        {
        //            System.IO.Directory.CreateDirectory(path); //Create directory if it doesn't exist
        //        }

        //        string imageName = Guid.NewGuid().ToString() + ".jpg";

        //        //set the image path
        //        string imgPath = Path.Combine(path, imageName);

        //        byte[] imageBytes = Convert.FromBase64String(s_base64);
        //        System.IO.File.WriteAllBytes(imgPath, imageBytes);
        //        db.Database.ExecuteSqlCommand("update customers set avatar=N'/Images/customer/" + imageName + "' where id=" + user_id);
        //        var usi = db.customers.Find(user_id);
        //        field.Add("image", "/Images/customer/" + imageName);
        //        field.Add("user_name", usi.name);
        //        field.Add("user_email", usi.email);
        //        field.Add("user_phone", usi.phone);
        //        field.Add("identify", usi.identify);
        //        field.Add("address", usi.address);
        //        field.Add("avatar", usi.avatar);
        //        field.Add("points", usi.points.ToString());
        //        return Api("success", field, "Tải ảnh avatar lên thành công!");
        //        //return "/Images/customer/"+ imageName;
        //    }
        //    catch
        //    {
        //        field.Add("image", "");
        //        return Api("error", field, "Không tải được ảnh lên");
        //    }
            
        //}
        //Upload avatar cho user có id là user_id cập nhật ảnh, gửi lên ảnh dạng base64, server tự decode lại
        //trả về đường dẫn ảnh trong trường image nếu thành công, trả về rỗng nếu lỗi
        [HttpPost]
        public string updateImageUser(int user_id,string base64)//int user_id, 
        {
            Dictionary<string, string> field = new Dictionary<string, string>();
            try
            {
                String path = Server.MapPath("~//images//customer"); //Path

                //Check if directory exist
                if (!System.IO.Directory.Exists(path))
                {
                    System.IO.Directory.CreateDirectory(path); //Create directory if it doesn't exist
                }

                string imageName = Guid.NewGuid().ToString() + ".jpg";

                //set the image path
                string imgPath = Path.Combine(path, imageName);

                byte[] imageBytes = Convert.FromBase64String(base64);
                System.IO.File.WriteAllBytes(imgPath, imageBytes);
                db.Database.ExecuteSqlCommand("update customers set avatar=N'/Images/customer/" + imageName + "' where id=" + user_id);
                var usi = db.customers.Find(user_id);
                field.Add("image", "/Images/customer/" + imageName);
                field.Add("user_name", usi.name);
                field.Add("user_email", usi.email);
                field.Add("user_phone", usi.phone);
                field.Add("identify", usi.identify);
                field.Add("address", usi.address);
                field.Add("avatar", usi.avatar);
                field.Add("points", usi.points.ToString());
                return Api("success", field, "Tải ảnh avatar lên thành công!");
                //return "/Images/customer/"+ imageName;
            }
            catch(Exception ex)
            {
                field.Add("image", "");
                return Api("error", field, ex.ToString());
            }

        }
    }
}