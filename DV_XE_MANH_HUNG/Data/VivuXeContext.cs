using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using Vivu_Xe.Models;
using VivuXe.Models;

namespace Vivu_Xe.Data;

public partial class VivuXeContext : DbContext
{
    public VivuXeContext()
    {
    }

    public VivuXeContext(DbContextOptions<VivuXeContext> options)
        : base(options)
    {
    }

    public virtual DbSet<DanhGia> DanhGias { get; set; }

    public virtual DbSet<DonDatXe> DonDatXes { get; set; }

    public virtual DbSet<GiayTo> GiayTos { get; set; }

    public virtual DbSet<HangXe> HangXes { get; set; }

    public virtual DbSet<HinhAnhXe> HinhAnhXes { get; set; }

    public virtual DbSet<LoaiXe> LoaiXes { get; set; }

    public virtual DbSet<NguoiDung> NguoiDungs { get; set; }

    public virtual DbSet<SuCoPhatSinh> SuCoPhatSinhs { get; set; }

    public virtual DbSet<VaiTro> VaiTros { get; set; }

    public virtual DbSet<Xe> Xes { get; set; }

    public virtual DbSet<XeYeuThich> XeYeuThiches { get; set; }
    public virtual DbSet<ThanhToan> ThanhToans { get; set; }
    public virtual DbSet<DanhMucTin> DanhMucTins { get; set; }
    public virtual DbSet<TinTuc> TinTucs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Data Source=DESKTOP-VKF78LH;Initial Catalog=VivuXeDB;Integrated Security=True;Encrypt=False");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DanhGia>(entity =>
        {
            entity.HasKey(e => e.MaDanhGia).HasName("PK__DanhGia__AA9515BFD33DE0A5");

            entity.Property(e => e.NgayDanhGia)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.MaDonNavigation).WithMany(p => p.DanhGias)
                .HasForeignKey(d => d.MaDon)
                .HasConstraintName("FK__DanhGia__MaDon__6E01572D");

            entity.HasOne(d => d.MaNguoiDungNavigation).WithMany(p => p.DanhGias)
                .HasForeignKey(d => d.MaNguoiDung)
                .HasConstraintName("FK__DanhGia__MaNguoi__6FE99F9F");

            entity.HasOne(d => d.MaXeNavigation).WithMany(p => p.DanhGia)
                .HasForeignKey(d => d.MaXe)
                .HasConstraintName("FK__DanhGia__MaXe__6EF57B66");
        });

        modelBuilder.Entity<DonDatXe>(entity =>
        {
            entity.HasKey(e => e.MaDon).HasName("PK__DonDatXe__3D89F568C9169DCB");

            entity.ToTable("DonDatXe");

            entity.Property(e => e.NgayDat)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NgayNhanDuKien).HasColumnType("datetime");
            entity.Property(e => e.NgayTraDuKien).HasColumnType("datetime");
            entity.Property(e => e.NgayTraThucTe).HasColumnType("datetime");
            entity.Property(e => e.TienCocDaDong).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.TongTien).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.TrangThaiDon)
                .HasMaxLength(50)
                .HasDefaultValue("Chờ duyệt");

            entity.HasOne(d => d.MaNguoiDungNavigation).WithMany(p => p.DonDatXes)
                .HasForeignKey(d => d.MaNguoiDung)
                .HasConstraintName("FK__DonDatXe__MaNguo__6477ECF3");

            entity.HasOne(d => d.MaXeNavigation).WithMany(p => p.DonDatXes)
                .HasForeignKey(d => d.MaXe)
                .HasConstraintName("FK__DonDatXe__MaXe__656C112C");
        });

        modelBuilder.Entity<GiayTo>(entity =>
        {
            entity.HasKey(e => e.MaGiayTo).HasName("PK__GiayTo__D6796CCA1554E5D8");

            entity.ToTable("GiayTo");

            entity.Property(e => e.AnhMatSau).IsUnicode(false);
            entity.Property(e => e.AnhMatTruoc).IsUnicode(false);
            entity.Property(e => e.DaXacThuc).HasDefaultValue(false);
            entity.Property(e => e.LoaiGiayTo).HasMaxLength(50);
            entity.Property(e => e.NgayTaiLen)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.SoGiayTo)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.MaNguoiDungNavigation).WithMany(p => p.GiayTos)
                .HasForeignKey(d => d.MaNguoiDung)
                .HasConstraintName("FK__GiayTo__MaNguoiD__5165187F");
        });

        modelBuilder.Entity<HangXe>(entity =>
        {
            entity.HasKey(e => e.MaHang).HasName("PK__HangXe__19C0DB1D5D8C8D72");

            entity.ToTable("HangXe");

            entity.Property(e => e.TenHang).HasMaxLength(50);
            entity.Property(e => e.XuatXu).HasMaxLength(50);
        });

        modelBuilder.Entity<HinhAnhXe>(entity =>
        {
            entity.HasKey(e => e.MaHinh).HasName("PK__HinhAnhX__13EE108491ACD86D");

            entity.ToTable("HinhAnhXe");

            entity.Property(e => e.DuongDan).IsUnicode(false);

            entity.HasOne(d => d.MaXeNavigation).WithMany(p => p.HinhAnhXes)
                .HasForeignKey(d => d.MaXe)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__HinhAnhXe__MaXe__619B8048");
        });

        modelBuilder.Entity<LoaiXe>(entity =>
        {
            entity.HasKey(e => e.MaLoai).HasName("PK__LoaiXe__730A5759B5618F96");

            entity.ToTable("LoaiXe");

            entity.Property(e => e.MoTa).HasMaxLength(200);
            entity.Property(e => e.TenLoai).HasMaxLength(50);
        });

        modelBuilder.Entity<NguoiDung>(entity =>
        {
            entity.HasKey(e => e.MaNguoiDung).HasName("PK__NguoiDun__C539D7621CE07BBF");

            entity.ToTable("NguoiDung");

            entity.HasIndex(e => e.Email, "UQ__NguoiDun__A9D10534A161C704").IsUnique();

            entity.Property(e => e.AnhDaiDien).IsUnicode(false);
            entity.Property(e => e.DiaChi).HasMaxLength(255);
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.HoTen).HasMaxLength(100);
            entity.Property(e => e.MatKhau)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.SoDienThoai)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.TrangThai).HasDefaultValue(true);

            entity.HasOne(d => d.MaVaiTroNavigation).WithMany(p => p.NguoiDungs)
                .HasForeignKey(d => d.MaVaiTro)
                .HasConstraintName("FK__NguoiDung__MaVai__4CA06362");
        });

        modelBuilder.Entity<SuCoPhatSinh>(entity =>
        {
            entity.HasKey(e => e.MaSuCo).HasName("PK__SuCoPhat__A69DF79F5D31EE1A");

            entity.ToTable("SuCoPhatSinh");

            entity.Property(e => e.LoaiSuCo).HasMaxLength(50);
            entity.Property(e => e.NgayGhiNhan)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PhiPhat).HasColumnType("decimal(18, 0)");

            entity.HasOne(d => d.MaDonNavigation).WithMany(p => p.SuCoPhatSinhs)
                .HasForeignKey(d => d.MaDon)
                .HasConstraintName("FK__SuCoPhatS__MaDon__6A30C649");
        });

        modelBuilder.Entity<VaiTro>(entity =>
        {
            entity.HasKey(e => e.MaVaiTro).HasName("PK__VaiTro__C24C41CFB56426EC");

            entity.ToTable("VaiTro");

            entity.Property(e => e.MoTa).HasMaxLength(200);
            entity.Property(e => e.TenVaiTro).HasMaxLength(50);
        });

        modelBuilder.Entity<Xe>(entity =>
        {
            entity.HasKey(e => e.MaXe).HasName("PK__Xe__272520CDE7B8B123");

            entity.ToTable("Xe");

            entity.HasIndex(e => e.BienSo, "UQ__Xe__F7052EB6DF326BD9").IsUnique();

            entity.Property(e => e.BienSo)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.GiaThueNgay).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.HopSo).HasMaxLength(20);
            entity.Property(e => e.MauSac).HasMaxLength(30);
            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NhienLieu).HasMaxLength(20);
            entity.Property(e => e.TenXe).HasMaxLength(100);
            entity.Property(e => e.TienCoc).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(50)
                .HasDefaultValue("Sẵn sàng");

            entity.HasOne(d => d.MaHangNavigation).WithMany(p => p.Xes)
                .HasForeignKey(d => d.MaHang)
                .HasConstraintName("FK__Xe__MaHang__5AEE82B9");

            entity.HasOne(d => d.MaLoaiNavigation).WithMany(p => p.Xes)
                .HasForeignKey(d => d.MaLoai)
                .HasConstraintName("FK__Xe__MaLoai__5BE2A6F2");
        });

        modelBuilder.Entity<XeYeuThich>(entity =>
        {
            entity.HasKey(e => new { e.MaNguoiDung, e.MaXe }).HasName("PK__XeYeuThi__074B856E72A40B82");

            entity.ToTable("XeYeuThich");

            entity.Property(e => e.NgayLuu)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.MaNguoiDungNavigation).WithMany(p => p.XeYeuThiches)
                .HasForeignKey(d => d.MaNguoiDung)
                .HasConstraintName("FK__XeYeuThic__MaNgu__2A164134");

            entity.HasOne(d => d.MaXeNavigation).WithMany(p => p.XeYeuThiches)
                .HasForeignKey(d => d.MaXe)
                .HasConstraintName("FK__XeYeuThich__MaXe__2B0A656D");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
