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
using System.Text.Json;
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
            using OracleConnection con = new OracleConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            await con.OpenAsync();

            using OracleCommand cmd =
                new OracleCommand("CSNET_PLUS_REPORT_PKG.submit_reject", con);

            cmd.CommandType = CommandType.StoredProcedure;

            string receiptNos = string.Join(",", request.GSFS_Receipt_Nos);

            cmd.Parameters.Add("p_audit_type_id",
                OracleDbType.Int32).Value = request.AuditTypeId;

            cmd.Parameters.Add("p_gsfs_receipt_nos",
                OracleDbType.Varchar2).Value = receiptNos;

            cmd.Parameters.Add("p_action",
                OracleDbType.Varchar2).Value = "SUBMIT_TO_BRANCH";

            cmd.Parameters.Add("p_msg",
                OracleDbType.Varchar2,
                500).Direction = ParameterDirection.Output;

            await cmd.ExecuteNonQueryAsync();

            return cmd.Parameters["p_msg"].Value?.ToString() ?? "";
        }

        public async Task<string> GenerateMailData(
            string userId,
            string process,
            string sessionId,
            string receiptNos)
        {
            using OracleConnection con = new OracleConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            await con.OpenAsync();

            using OracleCommand cmd =
                new OracleCommand("CSNET_PLUS_MAIL_PKG.generate_mail_data", con);

            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("p_user_id",
                OracleDbType.Varchar2).Value = userId;

            cmd.Parameters.Add("p_process",
                OracleDbType.Varchar2).Value = process;

            cmd.Parameters.Add("p_session_id",
                OracleDbType.Varchar2).Value = sessionId;

            cmd.Parameters.Add("p_gsfs_receipt_nos",
                OracleDbType.Varchar2).Value = receiptNos;

            cmd.Parameters.Add("p_msg",
                OracleDbType.Varchar2,
                4000).Direction = ParameterDirection.Output;

            await cmd.ExecuteNonQueryAsync();

            return cmd.Parameters["p_msg"].Value?.ToString() ?? "";
        }
        public string SubmitToBranchSendEmail(
            string userId,
            string process,
            string sessionId,
            string receiptNos,
            out string fromEmail,
            out string toEmail,
            out string ccEmail,
            out string subject,
            out string header,
            out string footer,
            out string attachmentFile)
        {
            fromEmail = "";
            toEmail = "";
            ccEmail = "";
            subject = "";
            header = "";
            footer = "";
            attachmentFile = "";

            using OracleConnection con = new OracleConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            con.Open();

            using OracleCommand cmd =
                new OracleCommand("CSNET_PLUS_MAIL_PKG.generate_mail_data", con);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("p_user_id", OracleDbType.Varchar2).Value = userId;
            cmd.Parameters.Add("p_process", OracleDbType.Varchar2).Value = process;
            cmd.Parameters.Add("p_session_id", OracleDbType.Varchar2).Value = sessionId;
            cmd.Parameters.Add("p_gsfs_receipt_nos", OracleDbType.Varchar2).Value = receiptNos;
            cmd.Parameters.Add("p_msg", OracleDbType.Varchar2, 4000).Direction = ParameterDirection.Output;
            cmd.ExecuteNonQuery();
            string message = cmd.Parameters["p_msg"].Value?.ToString() ?? "";
            return message;
        }
        public async Task<List<MailResponseModel>> GetMailExcelData(string screenType, string sessionId)
        {
            List<MailResponseModel> mailList = new();
            using OracleConnection con = new OracleConnection(
                _configuration.GetConnectionString("DefaultConnection"));
            await con.OpenAsync();
            using OracleCommand cmd = new OracleCommand("CSNET_PLUS_MAIL_PKG.get_mail_excel_data", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("p_screen_type", OracleDbType.Varchar2).Value = screenType;
            cmd.Parameters.Add("p_session_id", OracleDbType.Varchar2).Value = sessionId;
            cmd.Parameters.Add("p_mail_data", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
            cmd.Parameters.Add("p_msg", OracleDbType.Varchar2, 4000).Direction = ParameterDirection.Output;
            await cmd.ExecuteNonQueryAsync();
            string message = cmd.Parameters["p_msg"].Value?.ToString() ?? "";
            if (!string.IsNullOrWhiteSpace(message) &&
                !message.Equals("SUCCESS", StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception(message);
            }

            using OracleDataReader reader = ((OracleRefCursor)cmd.Parameters["p_mail_data"].Value).GetDataReader();
            while (await reader.ReadAsync())
            {
                MailResponseModel model = new MailResponseModel();
                model.ShipToCode = reader["SHIP_TO_CODE"]?.ToString() ?? "";
                model.AuditMonth = reader["AUDIT_MONTH"]?.ToString() ?? "";
                model.ValidTill = reader["VALID_TILL"]?.ToString() ?? "";
                model.CompanyName = reader["COMPANY_NAME"]?.ToString() ?? "";
                model.EventType = reader["EVENT_TYPE"]?.ToString() ?? "";
                model.CreatedBy = reader["CREATED_BY"]?.ToString() ?? "";
                model.MailTo = reader["MAIL_TO"]?.ToString() ?? "";
                model.MailCc = reader["MAIL_CC"]?.ToString() ?? "";
                string summaryJson = reader["SUMMARY_JSON"]?.ToString() ?? "[]";
                model.Rows = ConvertJsonToRows(summaryJson);
                mailList.Add(model);
            }
            return mailList;
        }
        private List<MailDynamicRow> ConvertJsonToRows(string json)
        {
            List<MailDynamicRow> rows = new();
            if (string.IsNullOrWhiteSpace(json))
                return rows;
            using JsonDocument document = JsonDocument.Parse(json);
            foreach (JsonElement item in document.RootElement.EnumerateArray())
            {
                MailDynamicRow row = new MailDynamicRow();
                row.AuditHead = item.GetProperty("audit_head").GetString() ?? "";
                row.NewUpload = item.TryGetProperty("new_upload", out var newUpload) ? newUpload.GetInt32() : 0;
                row.PreviousPending =
                    item.TryGetProperty("pending", out var pending)
                        ? pending.GetInt32()
                        : 0;

                row.AcceptedCount =
                    item.TryGetProperty("accepted", out var accepted)
                        ? accepted.GetInt32()
                        : 0;

                row.RejectedCount =
                    item.TryGetProperty("rejected", out var rejected)
                        ? rejected.GetInt32()
                        : 0;

                row.FeedbackSubmitCount =
                    item.TryGetProperty("feedback", out var feedback)
                        ? feedback.GetInt32()
                        : 0;

                row.Total =
                    row.NewUpload +
                    row.PreviousPending +
                    row.AcceptedCount +
                    row.RejectedCount +
                    row.FeedbackSubmitCount;
                rows.Add(row);
            }
            return rows;
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

