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
    
    public partial class winning
    {
        public long id { get; set; }
        public Nullable<int> code_company { get; set; }
        public string company { get; set; }
        public string partner { get; set; }
        public Nullable<int> id_partner { get; set; }
        public string name { get; set; }
        public Nullable<long> money { get; set; }
        public Nullable<int> quantity { get; set; }
        public Nullable<System.DateTime> from_date { get; set; }
        public Nullable<System.DateTime> to_date { get; set; }
        public Nullable<int> from_date_id { get; set; }
        public Nullable<int> to_date_id { get; set; }
        public string image { get; set; }
        public string big_image { get; set; }
        public string image1 { get; set; }
        public string image2 { get; set; }
        public string image3 { get; set; }
        public string des { get; set; }
    }
}
