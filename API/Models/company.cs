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
    
    public partial class company
    {
        public int id { get; set; }
        public string name { get; set; }
        public Nullable<int> code { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string pass { get; set; }
        public Nullable<System.DateTime> date_time { get; set; }
        public Nullable<int> is_admin { get; set; }
        public Nullable<int> modifiable { get; set; }
        public string phone_contact { get; set; }
        public string address { get; set; }
        public string email_contact { get; set; }
    }
}
