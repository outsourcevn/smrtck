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
    
        public virtual DbSet<company> companies { get; set; }
        public virtual DbSet<customer_bonus_log> customer_bonus_log { get; set; }
        public virtual DbSet<partner> partners { get; set; }
        public virtual DbSet<qrcode_log> qrcode_log { get; set; }
        public virtual DbSet<sn_active> sn_active { get; set; }
        public virtual DbSet<sn_locations> sn_locations { get; set; }
        public virtual DbSet<sn_smart_point> sn_smart_point { get; set; }
        public virtual DbSet<user> users { get; set; }
        public virtual DbSet<voucher_log> voucher_log { get; set; }
        public virtual DbSet<voucher_points> voucher_points { get; set; }
        public virtual DbSet<winning_log> winning_log { get; set; }
        public virtual DbSet<config_bonus_point> config_bonus_point { get; set; }
        public virtual DbSet<winning> winnings { get; set; }
        public virtual DbSet<customer> customers { get; set; }
        public virtual DbSet<splash> splashes { get; set; }
        public virtual DbSet<waranty_customer> waranty_customer { get; set; }
        public virtual DbSet<qrcode> qrcodes { get; set; }
        public virtual DbSet<checkall> checkalls { get; set; }
        public virtual DbSet<config_app> config_app { get; set; }
    }
}
