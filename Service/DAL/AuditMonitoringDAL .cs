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

namespace CallAuditPortal1.Service.DAL
{
    public class AuditMonitoringDAL:IAuditMonitoringDAL
    {
        private readonly IConfiguration _configuration;
        public AuditMonitoringDAL(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task<string> SubmitToBranch(SubmitBranchRequest request)
        {
            using (OracleConnection con =
                   new OracleConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await con.OpenAsync();

                foreach (int id in request.SelectedIds)
                {
                    string escEmail = "";
                    string lgcEmail = "";
                    string asmEmail = "";
                    string bsmEmail = "";
                    string claimNo = "";
                    string auditType = "";

                    string query = @"
                SELECT
                    ESC_EMAIL,
                    LGC_EMAIL,
                    ASM_EMAIL,
                    BSM_EMAIL,
                    CLAIM_NO,
                    AUDIT_TYPE
                FROM AUDIT_MONITORING
                WHERE ID = :ID";

                    using (OracleCommand cmd = new OracleCommand(query, con))
                    {
                        cmd.Parameters.Add("ID", OracleDbType.Int32).Value = id;

                        using (OracleDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                escEmail = reader["ESC_EMAIL"]?.ToString();
                                escEmail = reader["LGC_EMAIL"]?.ToString();
                                asmEmail = reader["ASM_EMAIL"]?.ToString();
                                bsmEmail = reader["BSM_EMAIL"]?.ToString();
                                claimNo = reader["CLAIM_NO"]?.ToString();
                                auditType = reader["AUDIT_TYPE"]?.ToString();
                            }
                        }
                    }

                    Email email = new Email
                    {
                        To = $"{escEmail},{lgcEmail}",
                        CC = $"{asmEmail},{bsmEmail}",
                        MailSubject = "Audit Submitted To Branch",
                        MailBody = $@"
                    Claim No : {claimNo}<br/>
                    Audit Type : {auditType}<br/>
                    Status : Submitted To Branch"
                    };

                    SendingEmail(email, _configuration);
                }

                return "Submitted To Branch Successfully";
            }
        }
        public static void SendingEmail(Email email, IConfiguration configuration)
        {
            try
            {
                string eMailServer = configuration["EmailServer"];

                string eMailSender =
                    string.IsNullOrWhiteSpace(email.From)
                    ? configuration["EmailSupport"]
                    : email.From;

                using (MailMessage mailMsg = new MailMessage())
                {
                    // To
                    if (!string.IsNullOrWhiteSpace(email.To))
                    {
                        mailMsg.To.Add(email.To);
                    }

                    // CC
                    if (!string.IsNullOrWhiteSpace(email.CC))
                    {
                        foreach (string cc in email.CC.Split(','))
                        {
                            if (!string.IsNullOrWhiteSpace(cc))
                            {
                                mailMsg.CC.Add(cc.Trim());
                            }
                        }
                    }

                    mailMsg.From = new MailAddress(eMailSender);
                    mailMsg.Subject = email.MailSubject;
                    mailMsg.Body = email.MailBody;
                    mailMsg.IsBodyHtml = true;

                    // Single Attachment
                    if (!string.IsNullOrWhiteSpace(email.AttachmentFileName)
                        && File.Exists(email.AttachmentFileName))
                    {
                        mailMsg.Attachments.Add(
                            new Attachment(email.AttachmentFileName));
                    }

                    // Multiple Attachments
                    if (email.Attachments != null)
                    {
                        foreach (string file in email.Attachments)
                        {
                            if (!string.IsNullOrWhiteSpace(file)
                                && File.Exists(file))
                            {
                                mailMsg.Attachments.Add(
                                    new Attachment(file));
                            }
                        }
                    }

                    using (SmtpClient smtp = new SmtpClient(eMailServer))
                    {
                        smtp.Send(mailMsg);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(
                    "Error while sending email : " + ex.Message);
            }
        }

        public async Task<string> Reject(RejectRequest request)
        {
            using (OracleConnection con = new OracleConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await con.OpenAsync();
                using (OracleCommand cmd = new OracleCommand("report_pkg.submit_reject", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    string receiptNos = string.Join(",", request.GSFS_Receipt_Nos);
                    cmd.Parameters.Add("p_audit_type_id", OracleDbType.Int32).Value = request.AuditTypeId;
                    cmd.Parameters.Add("p_gsfs_receipt_nos", OracleDbType.Varchar2).Value = receiptNos;
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
            using OracleCommand cmd = new OracleCommand("report_pkg.download_audit_data", con);
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

