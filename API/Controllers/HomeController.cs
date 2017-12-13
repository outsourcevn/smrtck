﻿using System;
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
        public string register(string name, string email, string phone, string pass, long? user_id,string identify,string address,string ref_phone,double? key)
        {
            Dictionary<string, string> field = new Dictionary<string, string>();
            try
            {
                if (key == null || !getKeyApi(key))
                {
                    field.Add("TotalMilliseconds", (DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds.ToString());
                    return Api("failed", field, "Bảo mật server, hàm api này không chạy được do ngày giờ ở máy bị sai số quá 10 phút hoặc chưa gửi key bảo mật!");
                }
                phone = phone.Trim();
                email = email.Trim();
                if (phone == "" || phone == null || pass == "" || pass == null)
                {
                    field.Add("user_id", "");
                    return Api("failed", field, "Số điện thoại hoặc pass phải khác rỗng!");
                }
                if ((user_id == 0 || user_id == null) && db.customers.Any(o => o.email == email || o.phone == phone))
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
                    field.Add("user_id", ct.id.ToString());
                    return Api("success", field, "Đăng ký thành công!");
                }
                else
                {
                    MD5 md5Hash = MD5.Create();
                    string hash = Config.GetMd5Hash(md5Hash, pass);
                    customer ct = db.customers.Find((long)user_id);
                    db.Entry(ct).State = EntityState.Modified;
                    ct.email = email;
                    ct.name = name;
                    ct.pass = hash;
                    ct.phone = phone;
                    ct.identify = identify;
                    ct.address = address;
                    ct.date_time = DateTime.Now;
                    db.SaveChanges();
                    field.Add("user_id", user_id.ToString());
                    return Api("success", field, "Cập nhật thành công!");
                }
            }catch(Exception ex){
                field.Add("user_id", "");
                return Api("error", field, "Cập nhật lỗi sql: " + ex.ToString());
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
                    field.Add("identify", us.identify);
                    field.Add("address", us.address);
                    field.Add("points", us.points.ToString());
                    return Api("success", field, "Thông tin khách hàng!");
                }
                else
                {
                    field.Add("user_id", "");
                    field.Add("user_name", "");
                    field.Add("user_email", "");
                    field.Add("user_phone", "");
                    field.Add("identify", "");
                    field.Add("address", "");
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
        //Hàm này trả về chi tiết trúng thưởng
        public string getWinningDetail(int id,int? os)
        {
            Dictionary<string, string> field = new Dictionary<string, string>();
            try
            {
                var p = db.winnings.Find(id);
                if (os == null || os == 1)
                {
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
                    return Api("success", field, "Danh sách chi tiết trúng thưởng");
                }
                else {
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
                    field.Add("des", "");//HttpUtility.HtmlEncode(p.des)
                    return Api("success", field, "Danh sách chi tiết trúng thưởng");
                }
            }
            catch (Exception ex)
            {
                field.Add("list", "[]");
                return Api("error", field, "Lỗi sql: " + ex.ToString());
            }
        }
        public ActionResult ViewWinningDetail(int id)
        {
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
        public string getHistoryCheckOfUser(long user_id)
        {
            Dictionary<string, string> field = new Dictionary<string, string>();
            try
            {
                var p = db.checkalls.Where(o => o.user_id == user_id).OrderByDescending(o => o.id).ToList();
                field.Add("list", JsonConvert.SerializeObject(p));
                return ApiArray("success", field, "Danh sách lịch sử quét của user này");
            }
            catch (Exception ex)
            {
                field.Add("list", "[]");
                return Api("error", field, "Lỗi sql: " + ex.ToString());
            }
        }
        //Hàm này trả về danh sách trúng của user
        public string getListWinningOfUser(long user_id)
        {
            Dictionary<string, string> field = new Dictionary<string, string>();
            try
            {
                var p = db.winning_log.Where(o => o.user_id == user_id).OrderByDescending(o => o.id).ToList();
                field.Add("list", JsonConvert.SerializeObject(p));
                return ApiArray("success", field, "Danh sách trúng thưởng của user này");
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
        public string check(string guid,long? user_id,double lon, double lat,string address, double? key,int? points,int? os)
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
                int? code_company = 0;
                string company = "";
                string partner = "";
                int? id_partner = 0;
                long? winning_id = 0;
                long? stt = 0;
                long NUMBER = 27;
                //Kiểm tra QR code mới hay cũ
                if (guid!=null && guid.Contains("-"))
                {
                    //Tách ra làm 2 guid
                    string[] tempGuid = guid.Split('-');
                    string guid1 = guid.Substring(0, guid.LastIndexOf("-"));
                    string guid2 = tempGuid[tempGuid.Length-1];
                    if (isNumber(guid2))
                    {
                        guid = guid1;
                        long.TryParse(guid2, out NUMBER);
                    }
                }
           
                //Kiểm tra xem guid này có config cho cty nào không?
                if (db.qrcodes.Any(o=>o.guid==guid && o.status==0) && ((NUMBER-27)%13==0)){
                    var rs = db.qrcodes.Where(o => o.guid == guid && o.status == 0).FirstOrDefault();
                    code_company = rs.code_company;
                    company = rs.company;
                    partner = db.partners.Find((int)rs.id_partner).name;//rs.partner;
                    id_partner = rs.id_partner;
                    stt = rs.stt;
                    //winning_id = rs.winning_id!=null?rs.winning_id:0;
                    if (db.config_app.Any(o => o.code_company == code_company))
                    {
                        var info = db.config_app.Where(o => o.code_company == code_company).FirstOrDefault();
                        label = info.text_in_qr_code;
                        active = info.text_in_active;
                        location = info.text_in_location;
                        point = info.text_in_point;
                    }
                    else
                    {
                        var info = db.config_app.Where(o => o.id == 1).FirstOrDefault();
                        label = info.text_in_qr_code;
                        active = info.text_in_active;
                        location = info.text_in_location;
                        point = info.text_in_point;
                    }
                }
                else
                {
                    field.Add("info", "Sản phẩm này không được cấp mã tem của An Hà");
                    field.Add("active", "Sản phẩm này không được kích hoạt");
                    field.Add("location", "Tại địa điểm " + address);                   
                    field.Add("point", "Sản phẩm này không được tính tích điểm");
                    field.Add("total", "...");
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
                if (db.checkalls.Any(o => o.guid == guid && o.stt==NUMBER))
                {
                    var cka = db.checkalls.Where(o => o.guid == guid).FirstOrDefault();
                    string dtfm = Config.formatDateTime(cka.date_time);
                    field.Add("info", label);
                    field.Add("active", "Sản phẩm này đã kích hoạt trước đó rồi. Vào thời gian " + dtfm);
                    field.Add("location", "Sản phẩm này đã báo địa điểm " + cka.address + " trước đó tại thời điểm " + dtfm);
                    int? count = db.customers.Find(user_id).points;
                    field.Add("point", "Sản phẩm này đã được tích điểm vào lúc " + dtfm + ", bạn không thể tích thêm điểm");
                    field.Add("total", count.ToString());
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
                    cka.id_partner = id_partner;
                    cka.partner = partner;
                    cka.stt = NUMBER;//stt;
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
                            var wnbn = db.winnings.Where(o => o.code_company == code_company && o.from_date <= wdt && o.to_date >= wdt && o.quantity > 0).OrderBy(o=>o.money).FirstOrDefault();
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
                                db.winning_log.Add(wl);
                                db.SaveChanges();
                                db.Database.ExecuteSqlCommand("update winning set quantity=quantity-1 where id=" + winning_id);
                            }
                        }
                    }
                    catch
                    {
                        winning_id = 0;
                    }
                    int? count = ctm.points;
                    label = label + ". Nhà phân phối " + partner;
                    field.Add("info", label);
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
                    field.Add("point", point);
                    field.Add("total", count.ToString());
                    field.Add("winning_id", winning_id.ToString());
                }
                return Api("success", field, "Gửi thông tin về server thành công!");
            }
            catch (Exception ex)
            {
                field.Clear();
                field.Add("info", "");
                field.Add("active","");
                field.Add("location", "");
                field.Add("point","");
                field.Add("total","");
                field.Add("winning_id", "0");
                return Api("error", field, "Cập nhật lỗi sql: " + ex.ToString());
            }
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

    }
}