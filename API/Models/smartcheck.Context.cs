﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class smartcheckEntities : DbContext
    {
        public smartcheckEntities()
            : base("name=smartcheckEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<customer> customers { get; set; }
        public virtual DbSet<sn_active> sn_active { get; set; }
        public virtual DbSet<sn_locations> sn_locations { get; set; }
        public virtual DbSet<sn_products> sn_products { get; set; }
        public virtual DbSet<sn_smart_point> sn_smart_point { get; set; }
    }
}
