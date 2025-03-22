using System;
using System.Data;
using System.IO;
using System.Windows.Forms;
using ClosedXML.Excel;
using Npgsql;

namespace ParkingOut.Utils
{
    public static class ReportManager
    {
        private static readonly IAppLogger _logger = new FileLogger();

        public static DataTable GetDailyReport(DateTime tanggal, string jenisKendaraan)
        {
            try
            {
                string query = @"
                    SELECT 
                        p.nomor_polisi,
                        p.jenis_kendaraan,
                        p.waktu_masuk,
                        p.waktu_keluar,
                        p.durasi,
                        p.biaya,
                        p.status_tiket,
                        COALESCE(m.nomor_kartu, '-') as nomor_kartu_member,
                        COALESCE(m.nama_pemilik, '-') as nama_member
                    FROM t_parkir p
                    LEFT JOIN m_member m ON p.nomor_polisi = m.no_polisi
                    WHERE DATE(p.waktu_masuk) = @tanggal
                    AND (@jenisKendaraan = 'SEMUA' OR p.jenis_kendaraan = @jenisKendaraan)
                    ORDER BY p.waktu_masuk";

                using (var conn = new NpgsqlConnection(Database.ConnectionString))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@tanggal", tanggal.Date);
                        cmd.Parameters.AddWithValue("@jenisKendaraan", jenisKendaraan);

                        using (var adapter = new NpgsqlDataAdapter(cmd))
                        {
                            var dt = new DataTable();
                            adapter.Fill(dt);
                            return dt;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to generate daily report", ex);
                throw;
            }
        }

        public static DataTable GetMonthlyReport(DateTime startDate, DateTime endDate, string jenisKendaraan)
        {
            try
            {
                string query = @"
                    SELECT 
                        DATE(waktu_masuk) as tanggal,
                        jenis_kendaraan,
                        COUNT(*) as total_kendaraan,
                        SUM(biaya) as total_pendapatan
                    FROM t_parkir
                    WHERE DATE(waktu_masuk) BETWEEN @startDate AND @endDate
                    AND (@jenisKendaraan = 'SEMUA' OR jenis_kendaraan = @jenisKendaraan)
                    GROUP BY DATE(waktu_masuk), jenis_kendaraan
                    ORDER BY tanggal";

                using (var conn = new NpgsqlConnection(Database.ConnectionString))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@startDate", startDate.Date);
                        cmd.Parameters.AddWithValue("@endDate", endDate.Date);
                        cmd.Parameters.AddWithValue("@jenisKendaraan", jenisKendaraan);

                        using (var adapter = new NpgsqlDataAdapter(cmd))
                        {
                            var dt = new DataTable();
                            adapter.Fill(dt);
                            return dt;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to generate monthly report", ex);
                throw;
            }
        }

        public static DataTable GetMemberReport(DateTime startDate, DateTime endDate, string jenisKendaraan)
        {
            try
            {
                string query = @"
                    SELECT 
                        m.nomor_kartu,
                        m.nama_pemilik as nama,
                        m.jenis_kendaraan,
                        m.tanggal_daftar,
                        m.tanggal_expired,
                        COUNT(p.id) as total_parkir,
                        CASE 
                            WHEN m.tanggal_expired < NOW() THEN 'Expired'
                            WHEN m.aktif = false THEN 'Non-Aktif'
                            ELSE 'Aktif'
                        END as status
                    FROM m_member m
                    LEFT JOIN t_parkir p ON m.no_polisi = p.nomor_polisi
                        AND DATE(p.waktu_masuk) BETWEEN @startDate AND @endDate
                    WHERE (@jenisKendaraan = 'SEMUA' OR m.jenis_kendaraan = @jenisKendaraan)
                    GROUP BY m.member_id, m.nomor_kartu, m.nama_pemilik, m.jenis_kendaraan, m.tanggal_daftar, m.tanggal_expired, m.aktif
                    ORDER BY m.nama_pemilik";

                using (var conn = new NpgsqlConnection(Database.ConnectionString))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@startDate", startDate.Date);
                        cmd.Parameters.AddWithValue("@endDate", endDate.Date);
                        cmd.Parameters.AddWithValue("@jenisKendaraan", jenisKendaraan);

                        using (var adapter = new NpgsqlDataAdapter(cmd))
                        {
                            var dt = new DataTable();
                            adapter.Fill(dt);
                            return dt;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to generate member report", ex);
                throw;
            }
        }

        public static void ExportToExcel(DataTable dt, string filePath, string worksheetName)
        {
            try
            {
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add(worksheetName);

                    // Add headers
                    for (int i = 0; i < dt.Columns.Count; i++)
                    {
                        worksheet.Cell(1, i + 1).Value = dt.Columns[i].ColumnName;
                    }

                    // Add data
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        for (int j = 0; j < dt.Columns.Count; j++)
                        {
                            worksheet.Cell(i + 2, j + 1).Value = dt.Rows[i][j].ToString();
                        }
                    }

                    // Auto-fit columns
                    worksheet.Columns().AdjustToContents();

                    // Save the workbook
                    workbook.SaveAs(filePath);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to export to Excel", ex);
                throw;
            }
        }
    }
} 