using System;
using System.Collections.Generic;
using CcsSportScheduler_Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;

namespace CcsSportScheduler_Database
{
    public partial class SportSchedulerContext : DbContext
    {
        public SportSchedulerContext()
        {
        }

        public SportSchedulerContext(DbContextOptions<SportSchedulerContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Item> Items { get; set; } = null!;
        public virtual DbSet<Klub> Klubs { get; set; } = null!;
        public virtual DbSet<Liga> Ligas { get; set; } = null!;
        public virtual DbSet<NaplataTermina> Naplataterminas { get; set; } = null!;
        public virtual DbSet<Obavestenja> Obavestenjas { get; set; } = null!;
        public virtual DbSet<PopustiTermina> Popustiterminas { get; set; } = null!;
        public virtual DbSet<Racun> Racuns { get; set; } = null!;
        public virtual DbSet<RacunItem> Racunitems { get; set; } = null!;
        public virtual DbSet<Teren> Terens { get; set; } = null!;
        public virtual DbSet<Termin> Termins { get; set; } = null!;
        public virtual DbSet<TerminLiga> Terminligas { get; set; } = null!;
        public virtual DbSet<TerminTurnir> Terminturnirs { get; set; } = null!;
        public virtual DbSet<Turnir> Turnirs { get; set; } = null!;
        public virtual DbSet<UcesnikLiga> Ucesnikligas { get; set; } = null!;
        public virtual DbSet<UcesnikTurnir> Ucesnikturnirs { get; set; } = null!;
        public virtual DbSet<Uplata> Uplata { get; set; } = null!;
        public virtual DbSet<User> Users { get; set; } = null!;
        public virtual DbSet<VestiLiga> Vestiligas { get; set; } = null!;
        public virtual DbSet<VestiTurnir> Vestiturnirs { get; set; } = null!;
        public virtual DbSet<Zaduzenje> Zaduzenja { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                try
                {
                    IConfigurationRoot configuration = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json")
                    .Build();

                    var connectionString = configuration.GetConnectionString("WebApiDatabase");
                    optionsBuilder.UseMySql(connectionString, ServerVersion.Parse("8.0.26-mysql"));
                }
                catch (Exception ex)
                {

                }
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseCollation("utf8_general_ci")
                .HasCharSet("utf8");

            modelBuilder.Entity<Item>(entity =>
            {
                entity.ToTable("items");

                entity.HasIndex(e => e.KlubId, "fk_Items_Klub1_idx");

                entity.Property(e => e.Id)
                    .HasMaxLength(10)
                    .HasColumnName("id");

                entity.Property(e => e.KlubId).HasColumnName("Klub_id");

                entity.Property(e => e.Name)
                    .HasMaxLength(50)
                    .HasColumnName("name");

                entity.Property(e => e.Price)
                    .HasPrecision(10, 2)
                    .HasColumnName("price");

                entity.HasOne(d => d.Klub)
                    .WithMany(p => p.Items)
                    .HasForeignKey(d => d.KlubId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_Items_Klub1");
            });

            modelBuilder.Entity<Klub>(entity =>
            {
                entity.ToTable("klub");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Pib)
                    .HasMaxLength(15)
                    .HasColumnName("pib");

                entity.Property(e => e.DanaValute).HasColumnName("danaValute");

                entity.Property(e => e.Address)
                    .HasMaxLength(45)
                    .HasColumnName("address");

                entity.Property(e => e.City)
                    .HasMaxLength(45)
                    .HasColumnName("city");

                entity.Property(e => e.Email)
                    .HasMaxLength(60)
                    .HasColumnName("email");

                entity.Property(e => e.Name)
                    .HasMaxLength(100)
                    .HasColumnName("name");

                entity.Property(e => e.Number)
                    .HasMaxLength(45)
                    .HasColumnName("number");
            });

            modelBuilder.Entity<Liga>(entity =>
            {
                entity.ToTable("liga");

                entity.HasIndex(e => e.KlubId, "fk_Liga_Klub1_idx");

                entity.Property(e => e.Id)
                    .HasMaxLength(36)
                    .HasColumnName("id");

                entity.Property(e => e.EndDate)
                    .HasColumnType("datetime")
                    .HasColumnName("endDate");

                entity.Property(e => e.ImagesFolderName)
                    .HasMaxLength(45)
                    .HasColumnName("imagesFolderName");

                entity.Property(e => e.KlubId).HasColumnName("Klub_id");

                entity.Property(e => e.Name)
                    .HasMaxLength(45)
                    .HasColumnName("name");

                entity.Property(e => e.StartDate)
                    .HasColumnType("datetime")
                    .HasColumnName("startDate");

                entity.HasOne(d => d.Klub)
                    .WithMany(p => p.Ligas)
                    .HasForeignKey(d => d.KlubId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_Liga_Klub1");
            });

            modelBuilder.Entity<NaplataTermina>(entity =>
            {
                entity.ToTable("naplatatermina");

                entity.HasIndex(e => e.KlubId, "fk_NaplataTermina_Klub1_idx");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.KlubId).HasColumnName("Klub_id");

                entity.Property(e => e.Name)
                    .HasMaxLength(45)
                    .HasColumnName("name");

                entity.Property(e => e.Price)
                    .HasPrecision(10, 2)
                    .HasColumnName("price");

                entity.HasOne(d => d.Klub)
                    .WithMany(p => p.Naplataterminas)
                    .HasForeignKey(d => d.KlubId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_NaplataTermina_Klub1");
            });

            modelBuilder.Entity<Obavestenja>(entity =>
            {
                entity.ToTable("obavestenja");

                entity.HasIndex(e => e.UserId, "fk_Obavestenja_User1_idx");

                entity.HasIndex(e => e.TerminId, "fk_Obavestenja_Termin1_idx");

                entity.Property(e => e.Id)
                    .HasMaxLength(36)
                    .HasColumnName("id");

                entity.Property(e => e.Date)
                    .HasColumnType("datetime")
                    .HasColumnName("date");

                entity.Property(e => e.Description)
                    .HasColumnType("blob")
                    .HasColumnName("description");

                entity.Property(e => e.TerminId)
                    .HasMaxLength(36)
                    .HasColumnName("Termin_id");

                entity.Property(e => e.Seen).HasColumnName("seen");

                entity.Property(e => e.PrvoSlanje).HasColumnName("prvoSlanje");

                entity.Property(e => e.UserId).HasColumnName("User_id");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Obavestenjas)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_Obavestenja_User1");

                entity.HasOne(d => d.Termin)
                    .WithMany(p => p.Obavestenjas)
                    .HasForeignKey(d => d.TerminId)
                    .HasConstraintName("fk_Obavestenja_Termin1");
            });

            modelBuilder.Entity<PopustiTermina>(entity =>
            {
                entity.ToTable("popustitermina");

                entity.HasIndex(e => e.KlubId, "fk_PopustiTermina_Klub1_idx");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.KlubId).HasColumnName("Klub_id");

                entity.Property(e => e.Name)
                    .HasMaxLength(45)
                    .HasColumnName("name");

                entity.Property(e => e.Popust)
                    .HasPrecision(10, 2)
                    .HasColumnName("popust");

                entity.Property(e => e.TypeUser).HasColumnName("typeUser");

                entity.HasOne(d => d.Klub)
                    .WithMany(p => p.Popustiterminas)
                    .HasForeignKey(d => d.KlubId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_PopustiTermina_Klub1");
            });

            modelBuilder.Entity<Racun>(entity =>
            {
                entity.ToTable("racun");

                entity.HasIndex(e => e.UserId, "fk_Racun_User1_idx");

                entity.Property(e => e.Id)
                    .HasMaxLength(36)
                    .HasColumnName("id");

                entity.Property(e => e.InvoiceNumber)
                    .HasMaxLength(100)
                    .HasColumnName("invoiceNumber");

                entity.Property(e => e.Date)
                    .HasColumnType("datetime")
                    .HasColumnName("date");

                entity.Property(e => e.Placeno)
                    .HasPrecision(15, 2)
                    .HasColumnName("placeno");

                entity.Property(e => e.Pretplata)
                    .HasPrecision(15, 2)
                    .HasColumnName("pretplata");

                entity.Property(e => e.Otpis)
                    .HasPrecision(15, 2)
                    .HasColumnName("otpis");

                entity.Property(e => e.TotalAmount)
                    .HasPrecision(15, 2)
                    .HasColumnName("totalAmount");

                entity.Property(e => e.UserId).HasColumnName("User_id");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Racuns)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_Racun_User1");
            });

            modelBuilder.Entity<Zaduzenje>(entity =>
            {
                entity.ToTable("zaduzenje");

                entity.HasIndex(e => e.UserId, "fk_Zaduzenje_User1_idx");

                entity.Property(e => e.Id)
                    .HasMaxLength(36)
                    .HasColumnName("id");

                entity.Property(e => e.Opis)
                    .HasMaxLength(250)
                    .HasColumnName("opis");

                entity.Property(e => e.Date)
                    .HasColumnType("datetime")
                    .HasColumnName("date");

                entity.Property(e => e.Placeno)
                    .HasPrecision(15, 2)
                    .HasColumnName("placeno");

                entity.Property(e => e.Otpis)
                    .HasPrecision(15, 2)
                    .HasColumnName("otpis");

                entity.Property(e => e.TotalAmount)
                    .HasPrecision(15, 2)
                    .HasColumnName("totalAmount");

                entity.Property(e => e.Type)
                    .HasColumnName("type");

                entity.Property(e => e.UserId).HasColumnName("User_id");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Zaduzenja)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_Racun_User1");
            });

            modelBuilder.Entity<RacunItem>(entity =>
            {
                entity.HasKey(e => new { e.RacunId, e.ItemsId })
                    .HasName("PRIMARY")
                    .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

                entity.ToTable("racunitem");

                entity.HasIndex(e => e.ItemsId, "fk_RacunItem_Items1_idx");

                entity.Property(e => e.RacunId)
                    .HasMaxLength(36)
                    .HasColumnName("Racun_id");

                entity.Property(e => e.ItemsId)
                    .HasMaxLength(10)
                    .HasColumnName("Items_id");

                entity.Property(e => e.Name)
                    .HasMaxLength(100)
                    .HasColumnName("name");

                entity.Property(e => e.Quantity)
                    .HasPrecision(10, 3)
                    .HasColumnName("quantity");

                entity.Property(e => e.UnitPrice)
                    .HasPrecision(10, 2)
                    .HasColumnName("unitPrice");

                entity.Property(e => e.TotalAmount)
                    .HasPrecision(10, 2)
                    .HasColumnName("totalAmount");

                entity.HasOne(d => d.Items)
                    .WithMany(p => p.Racunitems)
                    .HasForeignKey(d => d.ItemsId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_RacunItem_Items1");

                entity.HasOne(d => d.Racun)
                    .WithMany(p => p.Racunitems)
                    .HasForeignKey(d => d.RacunId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_RacunItem_Racun1");
            });

            modelBuilder.Entity<Teren>(entity =>
            {
                entity.ToTable("teren");

                entity.HasIndex(e => e.KlubId, "fk_Teren_Klub1_idx");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.KlubId).HasColumnName("Klub_id");

                entity.Property(e => e.Name)
                    .HasMaxLength(45)
                    .HasColumnName("name");

                entity.HasOne(d => d.Klub)
                    .WithMany(p => p.Terens)
                    .HasForeignKey(d => d.KlubId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_Teren_Klub1");
            });

            modelBuilder.Entity<Termin>(entity =>
            {
                entity.ToTable("termin");

                entity.HasIndex(e => e.TerenId, "fk_Termin_Teren1_idx");

                entity.HasIndex(e => e.TerminLigaId, "fk_Termin_TerminLiga1_idx");

                entity.HasIndex(e => e.TerminTurnirId, "fk_Termin_TerminTurnir1_idx");

                entity.HasIndex(e => e.UserId, "fk_Termin_User1_idx");

                entity.Property(e => e.Id)
                    .HasMaxLength(36)
                    .HasColumnName("id");

                entity.Property(e => e.DateRezervacije)
                    .HasColumnType("datetime")
                    .HasColumnName("dateRezervacije");

                entity.Property(e => e.EndDateTime)
                    .HasColumnType("datetime")
                    .HasColumnName("endDateTime");

                entity.Property(e => e.Placeno)
                    .HasPrecision(15, 2)
                    .HasColumnName("placeno");

                entity.Property(e => e.Otpis)
                    .HasPrecision(15, 2)
                    .HasColumnName("otpis");

                entity.Property(e => e.Price)
                    .HasPrecision(15, 2)
                    .HasColumnName("price");

                entity.Property(e => e.StartDateTime)
                    .HasColumnType("datetime")
                    .HasColumnName("startDateTime");

                entity.Property(e => e.TerenId).HasColumnName("Teren_id");

                entity.Property(e => e.Type).HasColumnName("type");

                entity.Property(e => e.TerminLigaId)
                    .HasMaxLength(36)
                    .HasColumnName("TerminLiga_id");

                entity.Property(e => e.TerminTurnirId)
                    .HasMaxLength(36)
                    .HasColumnName("TerminTurnir_id");

                entity.Property(e => e.UserId).HasColumnName("User_id");

                entity.HasOne(d => d.Teren)
                    .WithMany(p => p.Termins)
                    .HasForeignKey(d => d.TerenId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_Termin_Teren1");

                entity.HasOne(d => d.TerminLiga)
                    .WithMany(p => p.Termins)
                    .HasForeignKey(d => d.TerminLigaId)
                    .HasConstraintName("fk_Termin_TerminLiga1");

                entity.HasOne(d => d.TerminTurnir)
                    .WithMany(p => p.Termins)
                    .HasForeignKey(d => d.TerminTurnirId)
                    .HasConstraintName("fk_Termin_TerminTurnir1");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Termins)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("fk_Termin_User1");
            });

            modelBuilder.Entity<TerminLiga>(entity =>
            {
                entity.ToTable("terminliga");

                entity.HasIndex(e => e.LigaId, "fk_MachLiga_Liga1_idx");

                entity.Property(e => e.Id)
                    .HasMaxLength(36)
                    .HasColumnName("id");

                entity.Property(e => e.LigaId)
                    .HasMaxLength(36)
                    .HasColumnName("Liga_id");

                entity.HasOne(d => d.Liga)
                    .WithMany(p => p.Terminligas)
                    .HasForeignKey(d => d.LigaId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_MachLiga_Liga1");
            });

            modelBuilder.Entity<TerminTurnir>(entity =>
            {
                entity.ToTable("terminturnir");

                entity.HasIndex(e => e.TurnirId, "fk_TerminTurnir_Turnir1_idx");

                entity.Property(e => e.Id)
                    .HasMaxLength(36)
                    .HasColumnName("id");

                entity.Property(e => e.TurnirId)
                    .HasMaxLength(36)
                    .HasColumnName("Turnir_id");

                entity.HasOne(d => d.Turnir)
                    .WithMany(p => p.Terminturnirs)
                    .HasForeignKey(d => d.TurnirId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_TerminTurnir_Turnir1");
            });

            modelBuilder.Entity<Turnir>(entity =>
            {
                entity.ToTable("turnir");

                entity.HasIndex(e => e.KlubId, "fk_Turnir_Klub1_idx");

                entity.Property(e => e.Id)
                    .HasMaxLength(36)
                    .HasColumnName("id");

                entity.Property(e => e.EndDate)
                    .HasColumnType("datetime")
                    .HasColumnName("endDate");

                entity.Property(e => e.ImagesFolderName)
                    .HasMaxLength(45)
                    .HasColumnName("imagesFolderName");

                entity.Property(e => e.KlubId).HasColumnName("Klub_id");

                entity.Property(e => e.Name)
                    .HasMaxLength(100)
                    .HasColumnName("name");

                entity.Property(e => e.StartDate)
                    .HasColumnType("datetime")
                    .HasColumnName("startDate");

                entity.HasOne(d => d.Klub)
                    .WithMany(p => p.Turnirs)
                    .HasForeignKey(d => d.KlubId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_Turnir_Klub1");
            });

            modelBuilder.Entity<UcesnikLiga>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.LigaId })
                    .HasName("PRIMARY")
                    .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

                entity.ToTable("ucesnikliga");

                entity.HasIndex(e => e.LigaId, "fk_UcesnikLiga_Liga1_idx");

                entity.Property(e => e.UserId).HasColumnName("User_id");

                entity.Property(e => e.LigaId)
                    .HasMaxLength(36)
                    .HasColumnName("Liga_id");

                entity.Property(e => e.DatumPrijave)
                    .HasColumnType("datetime")
                    .HasColumnName("datumPrijave");

                entity.HasOne(d => d.Liga)
                    .WithMany(p => p.Ucesnikligas)
                    .HasForeignKey(d => d.LigaId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_UcesnikLiga_Liga1");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Ucesnikligas)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_UcesnikLiga_User1");
            });

            modelBuilder.Entity<UcesnikTurnir>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.TurnirId })
                    .HasName("PRIMARY")
                    .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

                entity.ToTable("ucesnikturnir");

                entity.HasIndex(e => e.TurnirId, "fk_UcesnikTurnir_Turnir1");

                entity.HasIndex(e => e.UserId, "fk_UcesnikTurnir_User1_idx");

                entity.Property(e => e.UserId).HasColumnName("User_id");

                entity.Property(e => e.TurnirId)
                    .HasMaxLength(36)
                    .HasColumnName("Turnir_id");

                entity.Property(e => e.DatumPrijave)
                    .HasColumnType("datetime")
                    .HasColumnName("datumPrijave");

                entity.HasOne(d => d.Turnir)
                    .WithMany(p => p.Ucesnikturnirs)
                    .HasForeignKey(d => d.TurnirId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_UcesnikTurnir_Turnir1");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Ucesnikturnirs)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_UcesnikTurnir_User1");
            });

            modelBuilder.Entity<Uplata>(entity =>
            {
                entity.ToTable("uplata");

                entity.HasIndex(e => e.UserId, "fk_Uplata_User1_idx");

                entity.Property(e => e.Id)
                    .HasMaxLength(36)
                    .HasColumnName("id");

                entity.Property(e => e.Date)
                    .HasColumnType("datetime")
                    .HasColumnName("date");

                entity.Property(e => e.TotalAmount)
                    .HasPrecision(15, 2)
                    .HasColumnName("totalAmount");

                entity.Property(e => e.Razduzeno)
                    .HasPrecision(15, 2)
                    .HasColumnName("razduzeno");

                entity.Property(e => e.TypeUplata).HasColumnName("typeUplata");

                entity.Property(e => e.UserId).HasColumnName("User_id");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Uplata)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_Uplata_User1");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("user");

                entity.HasIndex(e => e.KlubId, "fk_User_Klub1_idx");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Contact)
                    .HasMaxLength(45)
                    .HasColumnName("contact");

                entity.Property(e => e.Jmbg)
                    .HasMaxLength(13)
                    .HasColumnName("jmbg");

                entity.Property(e => e.Email)
                    .HasMaxLength(60)
                    .HasColumnName("email");

                entity.Property(e => e.FreeTermin).HasColumnName("freeTermin");

                entity.Property(e => e.Pol).HasColumnName("pol");

                entity.Property(e => e.Year).HasColumnName("year");

                entity.Property(e => e.FullName)
                    .HasMaxLength(100)
                    .HasColumnName("fullName");

                entity.Property(e => e.KlubId).HasColumnName("Klub_id");

                entity.Property(e => e.Password)
                    .HasMaxLength(45)
                    .HasColumnName("password");

                entity.Property(e => e.Type).HasColumnName("type");

                entity.Property(e => e.Valuta).HasColumnName("valuta");

                entity.Property(e => e.Username)
                    .HasMaxLength(45)
                    .HasColumnName("username");

                entity.Property(e => e.ProfileImageUrl)
                    .HasMaxLength(255)
                    .HasColumnName("profileImageUrl");

                entity.HasOne(d => d.Klub)
                    .WithMany(p => p.Users)
                    .HasForeignKey(d => d.KlubId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_User_Klub1");
            });

            modelBuilder.Entity<VestiLiga>(entity =>
            {
                entity.HasKey(e => new { e.Id, e.LigaId })
                    .HasName("PRIMARY")
                    .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

                entity.ToTable("vestiliga");

                entity.HasIndex(e => e.LigaId, "fk_VestiLiga_Liga1_idx");

                entity.Property(e => e.Id)
                    .HasMaxLength(36)
                    .HasColumnName("id");

                entity.Property(e => e.LigaId)
                    .HasMaxLength(36)
                    .HasColumnName("Liga_id");

                entity.Property(e => e.Description)
                    .HasColumnType("blob")
                    .HasColumnName("description");

                entity.HasOne(d => d.Liga)
                    .WithMany(p => p.Vestiligas)
                    .HasForeignKey(d => d.LigaId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_VestiLiga_Liga1");
            });

            modelBuilder.Entity<VestiTurnir>(entity =>
            {
                entity.HasKey(e => new { e.Id, e.TurnirId })
                    .HasName("PRIMARY")
                    .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

                entity.ToTable("vestiturnir");

                entity.HasIndex(e => e.TurnirId, "fk_VestiTurnir_Turnir1_idx");

                entity.Property(e => e.Id)
                    .HasMaxLength(36)
                    .HasColumnName("id");

                entity.Property(e => e.TurnirId)
                    .HasMaxLength(36)
                    .HasColumnName("Turnir_id");

                entity.Property(e => e.Description)
                    .HasColumnType("blob")
                    .HasColumnName("description");

                entity.HasOne(d => d.Turnir)
                    .WithMany(p => p.Vestiturnirs)
                    .HasForeignKey(d => d.TurnirId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_VestiTurnir_Turnir1");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
