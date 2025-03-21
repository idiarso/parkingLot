using System;

namespace SimpleParkingAdmin.Models
{
    public class Member
    {
        public int Id { get; set; }
        public string NomorKartu { get; set; }
        public string Nama { get; set; }
        public string Alamat { get; set; }
        public string NoTelp { get; set; }
        public string JenisKendaraan { get; set; }
        public DateTime TanggalDaftar { get; set; }
        public DateTime TanggalExpired { get; set; }
        public decimal Biaya { get; set; }
        public bool Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public bool IsExpired => DateTime.Now > TanggalExpired;
        public bool IsActive => Status && !IsExpired;
    }

    public class MemberHistory
    {
        public int Id { get; set; }
        public int MemberId { get; set; }
        public string JenisTransaksi { get; set; }
        public DateTime Tanggal { get; set; }
        public decimal Biaya { get; set; }
        public string Keterangan { get; set; }
        public DateTime CreatedAt { get; set; }
    }
} 