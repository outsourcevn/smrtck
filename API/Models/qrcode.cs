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
    
    public partial class qrcode
    {
        public long id { get; set; }
        public string sn { get; set; }
        public string qrcode1 { get; set; }
        public string qrcode2 { get; set; }
        public Nullable<int> code_company { get; set; }
        public string company { get; set; }
        public string guid { get; set; }
        public Nullable<System.DateTime> date_time { get; set; }
        public Nullable<int> date_id { get; set; }
        public Nullable<int> status { get; set; }
        public Nullable<long> stt { get; set; }
        public Nullable<int> id_partner { get; set; }
        public string partner { get; set; }
        public Nullable<long> winning_id { get; set; }
        public Nullable<int> w_from_stt { get; set; }
        public Nullable<int> w_to_stt { get; set; }
        public Nullable<int> from_stt { get; set; }
        public Nullable<int> to_stt { get; set; }
        public Nullable<int> id_config_app { get; set; }
    }
}
