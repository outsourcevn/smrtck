//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace API.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class winning_log
    {
        public long id { get; set; }
        public Nullable<long> winning_id { get; set; }
        public string winning_name { get; set; }
        public Nullable<long> money { get; set; }
        public Nullable<long> user_id { get; set; }
        public string user_name { get; set; }
        public string user_email { get; set; }
        public string user_phone { get; set; }
        public Nullable<System.DateTime> date_time { get; set; }
        public Nullable<double> lon { get; set; }
        public Nullable<double> lat { get; set; }
        public string address { get; set; }
        public string qrcode { get; set; }
        public Nullable<long> sn { get; set; }
        public string company { get; set; }
        public string partner { get; set; }
        public string product_name { get; set; }
        public Nullable<int> code_company { get; set; }
        public Nullable<int> id_partner { get; set; }
        public Nullable<int> is_received { get; set; }
    }
}
