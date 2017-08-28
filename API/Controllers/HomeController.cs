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
        public string register(string name, string email, string phone, string pass, long? user_id, double? key)
        {
            Dictionary<string, string> field = new Dictionary<string, string>();
            try
            {
                if (key == null || !getKeyApi(key))
                {
                    field.Add("TotalMilliseconds", (DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds.ToString());
                    return Api("failed", field, "Bảo mật server, hàm api này không chạy được do ngày giờ ở máy bị sai số quá 10 phút hoặc chưa gửi key bảo mật!");
                }
                if (phone == "" || phone == null || pass == "" || pass == null)
                {
                    field.Add("user_id", "");
                    return Api("failed", field, "Số điện thoại hoặc pass phải khác rỗng!");
                }
                if (db.customers.Any(o => o.email == email || o.phone == phone))
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
                    db.customers.Add(ct);
                    db.SaveChanges();
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
        //Hàm này làm 3 việc và trả về 3 trường tương ứng là active,location và point (gửi kèm cả trường total nữa)
        //1. Kích hoạt sản phẩm với mã sản phẩm là guid, nếu sản phẩm này đã kích hoạt 1 lần rồi cũng báo đã kích hoạt
        //2. Báo địa điểm với mã sản phẩm là guid, nếu đã báo địa điểm rồi cũng báo là đã báo
        //3. Báo tích điểm với mã sản phẩm là guid, nếu đã tích điểm rồi cũng báo, hiện số điểm sau khi tích điểm, gửi kèm cả trường total là số điểm hiện thời sau khi tích điểm
        //Nếu lỗi sql trả về rỗng các trường và lỗi chi tiết
        [HttpPost]
        public string check(string guid,long? user_id,double lon, double lat,string address, double? key)
        {
            Dictionary<string, string> field = new Dictionary<string, string>();
            try
            {
                if (key == null || !getKeyApi(key))
                {
                    field.Add("TotalMilliseconds", (DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds.ToString());
                    return Api("failed", field, "Bảo mật server, hàm api này không chạy được do ngày giờ ở máy bị sai số quá 10 phút hoặc chưa gửi key bảo mật!");
                }
                //Kích hoạt
                if (db.sn_active.Any(o => o.guid == guid))
                {
                    field.Add("active", "Sản phẩm này đã kích hoạt trước đó rồi.");// + db.sn_active.Where(o => o.guid == guid).FirstOrDefault().date_time.ToString()
                }
                else
                {
                    field.Add("active", "Kích hoạt bảo hành sản phẩm thành công");
                    sn_active sa = new sn_active();
                    sa.date_time = DateTime.Now;
                    sa.guid = guid;
                    db.sn_active.Add(sa);
                    db.SaveChanges();
                }
                //Báo địa điểm 
                if (db.sn_locations.Any(o => o.guid == guid))
                {
                    field.Add("location", "Sản phẩm này đã báo địa điểm bán rồi");
                }
                else
                {
                                 
                    if (address == null || address == "")
                    {
                        address = RetrieveFormatedAddress(lat, lon);
                        if (address != null && address != "")
                            field.Add("location", "Báo địa điểm sản phẩm thành công");
                        else
                            field.Add("location", "Báo địa điểm được nhưng không lấy được địa chỉ cụ thể");
                    }
                    sn_locations sl = new sn_locations();
                    sl.address = address;
                    sl.guid = guid;
                    sl.lat = lat;
                    sl.lon = lon;
                    sl.user_id = user_id;
                    db.sn_locations.Add(sl);
                    db.SaveChanges();
                }
                //Tích điểm
                if (db.sn_smart_point.Any(o => o.guid == guid))
                {
                    int count = db.sn_smart_point.Count(o => o.user_id == user_id);
                    field.Add("point", "Sản phẩm này đã được tích điểm 1 lần rồi");
                    field.Add("total", count.ToString());
                }
                else
                {
                    sn_smart_point ssp = new sn_smart_point();
                    ssp.date_time = DateTime.Now;
                    ssp.guid = guid;
                    ssp.points = 1;
                    ssp.user_id = user_id;
                    db.sn_smart_point.Add(ssp);
                    db.SaveChanges();
                    int count = db.sn_smart_point.Count(o => o.user_id == user_id);
                    field.Add("point", "Tích điểm thành công, số điểm hiện tại của bạn là " + count.ToString());
                    field.Add("total", count.ToString());
                }
                return Api("success", field, "Gửi thông tin về server thành công!");
            }
            catch (Exception ex)
            {
                field.Clear();
                field.Add("active","");
                field.Add("location", "");
                field.Add("point","");
                field.Add("total","");
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
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

    }
}