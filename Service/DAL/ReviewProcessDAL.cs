using CallAuditPortal1.Model;
using CallAuditPortal1.Model.RequestDTO;
using CallAuditPortal1.Service.Interface;
using OfficeOpenXml;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System.Configuration;
using System.Data;
using System.Dynamic;

namespace CallAuditPortal1.Service.DAL
{
    public class ReviewProcessDAL : IReviewProcessDAL
    {
        private readonly IConfiguration _configuration;
        public ReviewProcessDAL(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task<byte[]> DownloadReviewProcess(DownloadReviewProcessRequest request)
        {
            using OracleConnection con = new OracleConnection(
                _configuration.GetConnectionString("DefaultConnection"));
            await con.OpenAsync();
            using OracleCommand cmd = new OracleCommand("report_pkg.download_audit_data", con);
            cmd.CommandType = CommandType.StoredProcedure;
            string receiptNos = string.Join(",", request.SelectedIds);
            cmd.Parameters.Add("p_gsfs_receipt_nos", OracleDbType.Varchar2).Value = receiptNos;
            cmd.Parameters.Add("p_result", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
            cmd.Parameters.Add("p_msg", OracleDbType.Varchar2, 500).Direction = ParameterDirection.Output;

            await cmd.ExecuteNonQueryAsync();
            var message = cmd.Parameters["p_msg"].Value?.ToString();

            OracleRefCursor cursor = (OracleRefCursor)cmd.Parameters["p_result"].Value;
            using OracleDataReader reader = cursor.GetDataReader();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using ExcelPackage package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Audit Report");
            int row = 1;
            for (int col = 0; col < reader.FieldCount; col++)
            {
                worksheet.Cells[row, col + 1].Value =
                    reader.GetName(col);
                worksheet.Cells[row, col + 1]
                         .Style.Font.Bold = true;
            }
            row++;
            while (await reader.ReadAsync())
            {
                for (int col = 0; col < reader.FieldCount; col++)
                {
                    worksheet.Cells[row, col + 1].Value =
                        reader.IsDBNull(col)
                            ? ""
                            : reader.GetValue(col);
                }
                row++;
            }
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
            return package.GetAsByteArray();
        }

        public async Task<(int,List<dynamic>)> SearchReviewProcess(ReviewProcessSearchRequest request)
        {
            List<dynamic> data = new List<dynamic>();

            try
            {
                using (OracleConnection con =
                       new OracleConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    await con.OpenAsync();
                    using (OracleCommand cmd =
                           new OracleCommand("report_pkg.GET_REVIEW_DATA", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("p_gsfs_reciept_no", OracleDbType.Varchar2)
                           .Value = request.ReceiptNo;

                        cmd.Parameters.Add("p_suspicious", OracleDbType.Varchar2)
                           .Value = request.Suspicious;

                        cmd.Parameters.Add("p_from_audit_date", OracleDbType.Varchar2)
                           .Value = request.FromAuditDate;

                        cmd.Parameters.Add("p_to_audit_date", OracleDbType.Varchar2)
                           .Value = request.ToAuditDate;

                        cmd.Parameters.Add("p_page_no", OracleDbType.Int32)
                           .Value = request.PageNumber != null ? request.PageNumber - 1 : request.PageNumber;

                        cmd.Parameters.Add("p_page_size", OracleDbType.Int32)
                           .Value = request.PageSize;

                        cmd.Parameters.Add("p_err", OracleDbType.Varchar2, 4000)
                           .Direction = ParameterDirection.Output;

                        cmd.Parameters.Add("p_count", OracleDbType.Int32)
                           .Direction = ParameterDirection.Output;

                        cmd.Parameters.Add("p_result", OracleDbType.RefCursor)
                           .Direction = ParameterDirection.Output;

                        await cmd.ExecuteNonQueryAsync();

                        OracleRefCursor refCursor = (OracleRefCursor)cmd.Parameters["p_result"].Value;

                        using (OracleDataReader reader = refCursor.GetDataReader())
                        {
                            while (await reader.ReadAsync())
                            {
                                Console.WriteLine("Row Found");
                                var row = new ExpandoObject() as IDictionary<string, object>;

                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    row.Add(
                                        reader.GetName(i),
                                        reader.IsDBNull(i)
                                            ? null
                                            : reader.GetValue(i)
                                    );
                                }

                                data.Add(row);
                            }
                        }
                        string errMsg = cmd.Parameters["p_err"].Value?.ToString();
                        int count = Convert.ToInt32(cmd.Parameters["p_count"].Value?.ToString());
                        return (count, data);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(
                    "Error in VerifyUpload : " + ex.Message +
                    " | Inner Exception : " + ex.InnerException?.Message
                );
            }
        }
    }
}
