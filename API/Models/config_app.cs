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
    
    public partial class config_app
    {
        public int id { get; set; }
        public string text_in_qr_code { get; set; }
        public string text_in_active { get; set; }
        public string text_in_location { get; set; }
        public string text_in_point { get; set; }
        public Nullable<int> code_company { get; set; }
        public string company { get; set; }
        public Nullable<int> waranty_year { get; set; }
        public string waranty_text { get; set; }
        public string waranty_link_web { get; set; }
        public string buy_more { get; set; }
        public Nullable<int> is_waranty { get; set; }
    }
}
