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
    
    public partial class sn_locations
    {
        public long id { get; set; }
        public string guid { get; set; }
        public Nullable<double> lon { get; set; }
        public Nullable<double> lat { get; set; }
        public string address { get; set; }
        public Nullable<long> user_id { get; set; }
        public Nullable<int> os { get; set; }
        public Nullable<System.DateTime> date_time { get; set; }
    }
}
