using CallAuditPortal1.Model;
using CallAuditPortal1.Model.RequestDTO;
using CallAuditPortal1.Service.Interface;
using DocumentFormat.OpenXml.Spreadsheet;
using OfficeOpenXml;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System.Data;
using System.Net;
using System.Net.Mail;
using static Microsoft.AspNetCore.Razor.Language.TagHelperMetadata;
using OracleCommand = Oracle.ManagedDataAccess.Client.OracleCommand;
using OracleConnection = Oracle.ManagedDataAccess.Client.OracleConnection;
using OracleDataAdapter = Oracle.ManagedDataAccess.Client.OracleDataAdapter;
using OracleParameter = Oracle.ManagedDataAccess.Client.OracleParameter;



namespace CallAuditPortal1.Service.DAL
{
    public class AuditMonitoringDAL : IAuditMonitoringDAL
    {
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        public AuditMonitoringDAL(IConfiguration configuration, IEmailService emailService)
        {
            _configuration = configuration;
            _emailService = emailService;
        }
        public async Task<string> SubmitToBranch(SubmitBranchRequest request)
        {
            using (OracleConnection con = new OracleConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await con.OpenAsync();
                using (OracleCommand cmd = new OracleCommand("CSNET_PLUS_REPORT_PKG.submit_reject", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    string receiptNos = string.Join(",", request.GSFS_Receipt_Nos);
                    cmd.Parameters.Add("p_audit_type_id", OracleDbType.Int32).Value = request.AuditTypeId;
                    cmd.Parameters.Add("p_gsfs_receipt_nos", OracleDbType.Varchar2).Value = receiptNos;
                    cmd.Parameters.Add("p_action", OracleDbType.Varchar2).Value = "SUBMIT_TO_BRANCH";
                    cmd.Parameters.Add("p_msg", OracleDbType.Varchar2, 500).Direction = ParameterDirection.Output;
                    await cmd.ExecuteNonQueryAsync();
                    var message = cmd.Parameters["p_msg"].Value?.ToString();

                    return message;
                }

            }
        }
        public string SubmitToBranchSendEmail(string userId,string process,string sessionId,string receiptNos, out string FromEmail, out string toEmail, out string CcEmail, out string subject, out string header, out string footer, out string attachementFile)
        {
            using (OracleConnection con = new OracleConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                con.Open();

                using (OracleCommand cmd = new OracleCommand("CSNET_PLUS_MAIL_PKG.generate_mail_data", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("p_user_id", OracleDbType.Varchar2).Value = userId;

                    cmd.Parameters.Add("p_process", OracleDbType.Varchar2).Value = process;

                    cmd.Parameters.Add("p_session_id", OracleDbType.Varchar2).Value = sessionId;

                    cmd.Parameters.Add("p_gsfs_receipt_nos", OracleDbType.Varchar2).Value = receiptNos;

                    FromEmail = string.Empty;
                    toEmail = string.Empty;
                    CcEmail = string.Empty;
                    subject = string.Empty;
                    header = string.Empty;
                    footer = string.Empty;
                    attachementFile = string.Empty;


                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("p_userid",
                        OracleDbType.Varchar2).Value = userId;
                   
                    cmd.Parameters.Add("o_msg",
                        OracleDbType.Varchar2,
                        4000).Direction = ParameterDirection.Output;

                    cmd.ExecuteNonQuery();

                    return cmd.Parameters["o_msg"].Value?.ToString() ?? "";
                }
            }
        }

        public async Task<string> Reject(RejectRequest request)
        {
            using (OracleConnection con = new OracleConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await con.OpenAsync();
                using (OracleCommand cmd = new OracleCommand("CSNET_PLUS_REPORT_PKG.submit_reject", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.BindByName = true;
                    string receiptNos = string.Join(",", request.GSFS_Receipt_Nos);
                    cmd.Parameters.Add("p_audit_type_id", OracleDbType.Int32).Value = request.AuditTypeId;
                    cmd.Parameters.Add("p_gsfs_receipt_nos", OracleDbType.Varchar2).Value = receiptNos;
                    cmd.Parameters.Add("p_action", OracleDbType.Varchar2).Value = "REJECT";
                    cmd.Parameters.Add("p_msg", OracleDbType.Varchar2, 500).Direction = ParameterDirection.Output;
                    await cmd.ExecuteNonQueryAsync();
                    var message = cmd.Parameters["p_msg"].Value?.ToString();
                    return message;
                }

            }
        }


        public async Task<byte[]> Download(DownloadRequest request)
        {
            using OracleConnection con = new OracleConnection(
                _configuration.GetConnectionString("DefaultConnection"));
            await con.OpenAsync();
            using OracleCommand cmd = new OracleCommand("CSNET_PLUS_REPORT_PKG.download_audit_data", con);
            cmd.CommandType = CommandType.StoredProcedure;
            string receiptNos = string.Join(",", request.GSFS_Receipt_Nos);
            cmd.Parameters.Add("p_audit_type_id", OracleDbType.Int32).Value = request.AuditTypeId;
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


    }
}

