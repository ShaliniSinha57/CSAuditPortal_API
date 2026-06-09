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

        public async Task<ReviewProcessSearchResponse> SearchReviewProcess(
     ReviewProcessSearchRequest request)
        {
            List<dynamic> data = new();

            try
            {
                using OracleConnection con = new OracleConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                await con.OpenAsync();

                using OracleCommand cmd = new OracleCommand(
                    "report_pkg.GET_REVIEW_DATA",
                    con);

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add(
                    "p_gsfs_reciept_no",
                    OracleDbType.Varchar2)
                    .Value =
                        string.IsNullOrWhiteSpace(request.ReceiptNo)
                        ? DBNull.Value
                        : request.ReceiptNo;

                cmd.Parameters.Add(
                    "p_suspicious",
                    OracleDbType.Varchar2)
                    .Value =
                        string.IsNullOrWhiteSpace(request.Suspicious)
                        ? DBNull.Value
                        : request.Suspicious;

                cmd.Parameters.Add(
                    "p_from_audit_date",
                    OracleDbType.Varchar2)
                    .Value =
                        request.FromAuditDate.HasValue
                        ? request.FromAuditDate.Value.ToString("yyyyMMdd")
                        : DBNull.Value;

                cmd.Parameters.Add(
                    "p_to_audit_date",
                    OracleDbType.Varchar2)
                    .Value =
                        request.ToAuditDate.HasValue
                        ? request.ToAuditDate.Value.ToString("yyyyMMdd")
                        : DBNull.Value;

                cmd.Parameters.Add(
                    "p_page_no",
                    OracleDbType.Int32)
                    .Value =
                        request.PageNumber ?? 1;

                cmd.Parameters.Add(
                    "p_page_size",
                    OracleDbType.Int32)
                    .Value =
                        request.PageSize ?? 10;

                cmd.Parameters.Add(
                    "p_err",
                    OracleDbType.Varchar2,
                    4000)
                    .Direction =
                        ParameterDirection.Output;

                cmd.Parameters.Add(
                    "p_count",
                    OracleDbType.Int32)
                    .Direction =
                        ParameterDirection.Output;

                cmd.Parameters.Add(
                    "p_result",
                    OracleDbType.RefCursor)
                    .Direction =
                        ParameterDirection.Output;

                await cmd.ExecuteNonQueryAsync();

                OracleRefCursor refCursor =
                    (OracleRefCursor)cmd.Parameters["p_result"].Value;

                using OracleDataReader reader =
                    refCursor.GetDataReader();

                while (await reader.ReadAsync())
                {
                    IDictionary<string, object> row =
                        new ExpandoObject();

                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        row.Add(
                            reader.GetName(i),
                            reader.IsDBNull(i)
                                ? null
                                : reader.GetValue(i));
                    }

                    data.Add(row);
                }

                string errMsg =
                    cmd.Parameters["p_err"].Value?.ToString();

                if (!string.IsNullOrWhiteSpace(errMsg))
                {
                    throw new Exception(errMsg);
                }

                int count =
                    cmd.Parameters["p_count"].Value == DBNull.Value
                    ? 0
                    : Convert.ToInt32(
                        cmd.Parameters["p_count"].Value);

                return new ReviewProcessSearchResponse
                {
                    Count = count,
                    Data = data
                };
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Error in SearchReviewProcess: {ex.Message}",
                    ex);
            }
        }
    }
}
