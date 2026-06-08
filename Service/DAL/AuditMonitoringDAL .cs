using CallAuditPortal1.Model;
using CallAuditPortal1.Model.RequestDTO;
using CallAuditPortal1.Service.Interface;
using DocumentFormat.OpenXml.Spreadsheet;
using Oracle.ManagedDataAccess.Client;
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
        public async Task<List<AuditMonitoringModel>> SearchAuditData(AuditSearchRequest request)
        {
            List<AuditMonitoringModel> data = new List<AuditMonitoringModel>();
            using (OracleConnection con = new OracleConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await con.OpenAsync();
                using (OracleCommand cmd =
                       new OracleCommand("report_pkg.get_audit_data", con))
                {
                    cmd.BindByName = true;
                    cmd.Parameters.Add("STATUS", OracleDbType.Varchar2).Value = request.Status;
                    cmd.Parameters.Add("AUDIT_TYPE", OracleDbType.Varchar2).Value = request.AuditType;
                    cmd.Parameters.Add("FROM_DATE", OracleDbType.Varchar2).Value = request.FromDate;
                    cmd.Parameters.Add("TO_DATE", OracleDbType.Varchar2).Value = request.ToDate;
                    OracleDataReader reader = await cmd.ExecuteReaderAsync();
                    while (
                        await reader.ReadAsync())

                    {
                        data.Add(new AuditMonitoringModel
                        {
                                Id = Convert.ToInt32(reader["ID"]),
                           
                            Status = reader["STATUS"].ToString(),
                            AuditType = reader["AUDIT_TYPE"].ToString(),
                            AuditDate = reader["AUDIT_DATE"].ToString(),
                            
                        });
                    }
                }
            }
            return data;
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
            using OracleConnection con = new OracleConnection( _configuration.GetConnectionString("DefaultConnection"));
            await con.OpenAsync();
            foreach (var id in request.Ids)
            {
                using OracleCommand cmd = new OracleCommand("report_pkg.submit_reject", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("p_id", OracleDbType.Int32).Value = id;
                cmd.Parameters.Add("p_reason", OracleDbType.Varchar2).Value = request.Reason;
                await cmd.ExecuteNonQueryAsync();
            }
            return "Rejected Successfully!";
        }
    }
}

