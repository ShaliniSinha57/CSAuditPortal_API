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
        public async Task<List<AuditMonitoringModel>> SearchAuditData(AuditSearchRequest request)
        {
            List<AuditMonitoringModel> data = new List<AuditMonitoringModel>();
            using (OracleConnection con = new OracleConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await con.OpenAsync();
                string query = @" ";

                using (OracleCommand cmd = new OracleCommand(query, con))
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
                            ClaimNo = reader["CLAIM_NO"].ToString(),
                            Status = reader["STATUS"].ToString(),
                            AuditType = reader["AUDIT_TYPE"].ToString(),
                            AuditDate = reader["AUDIT_DATE"].ToString(),
                            Email = reader["ECG_LGC_EMAIL"].ToString()
                        });
                    }
                }
            }
            return data;
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
    }
}

