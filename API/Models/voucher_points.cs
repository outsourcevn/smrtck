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
    
    public partial class voucher_points
    {
        public int id { get; set; }
        public string name { get; set; }
        public string des { get; set; }
        public string full_des { get; set; }
        public Nullable<int> quantity { get; set; }
        public Nullable<System.DateTime> from_date { get; set; }
        public Nullable<System.DateTime> to_date { get; set; }
        public Nullable<int> from_date_id { get; set; }
        public Nullable<int> to_date_id { get; set; }
        public string image { get; set; }
        public Nullable<int> price { get; set; }
        public string big_image { get; set; }
        public string image1 { get; set; }
        public string image2 { get; set; }
        public string image3 { get; set; }
        public Nullable<int> code_company { get; set; }
        public string company { get; set; }
    }
}
