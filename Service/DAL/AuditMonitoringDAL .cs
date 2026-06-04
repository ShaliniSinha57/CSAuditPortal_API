using CallAuditPortal1.Model;
using CallAuditPortal1.Model.RequestDTO;
using CallAuditPortal1.Service.Interface;
using Oracle.ManagedDataAccess.Client;
using System.Net;
using System.Net.Mail;

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
            using (OracleConnection con = new OracleConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await con.OpenAsync();

                string query = @" ";

                using (OracleCommand cmd = new OracleCommand(query, con))
                {
                    cmd.Parameters.Add("ID", OracleDbType.Int32).Value = request.Id;
                    await cmd.ExecuteNonQueryAsync();

                }

                MailMessage mail = new MailMessage();
                mail.From = new MailAddress("test@gmail.com");
                mail.To.Add(request.Email);
                mail.Subject = "Audit Submitted To Branch";
                mail.Body = $"Claim No : " + $"{request.ClaimNo}<br/>" +
                    $"Audit Type : " + $"{request.AuditType}<br/>" +
                    $"Status : " +
                    $"Submitted To Branch";
                mail.IsBodyHtml = true;
                SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
                smtp.Credentials = new NetworkCredential("test@gmail.com", "password");
                smtp.EnableSsl = true;
                await smtp.SendMailAsync(mail);
                return
                    "Submitted To Branch Successfully";
            }
        }

        public async Task<string> Download(DownloadRequest request)
        {
            using (OracleConnection con = new OracleConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await con.OpenAsync();
                string query = @"";

                using (OracleCommand cmd = new OracleCommand(query, con))
                {
                    cmd.Parameters.Add("ID", OracleDbType.Varchar2).Value = request.Ids.ToString();
                    await cmd.ExecuteNonQueryAsync();

                }

                return "Downloaded Successfully !";
            }
        }

        public async Task<string> Reject(RejectRequest request)
        {
            using (OracleConnection con = new OracleConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await con.OpenAsync();
                string query = @"";

                using (OracleCommand cmd = new OracleCommand(query, con))
                {
                    cmd.Parameters.Add("ID", OracleDbType.Varchar2).Value = request.Ids.ToString();
                    cmd.Parameters.Add("Reason", OracleDbType.Varchar2).Value = request.Reason;
                    await cmd.ExecuteNonQueryAsync();

                }

                return "Rejected Successfully !";
            }
        }
    }
}

