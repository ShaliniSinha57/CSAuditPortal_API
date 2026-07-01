using CallAuditPortal1.Model;
using CallAuditPortal1.Model.RequestDTO;
using CallAuditPortal1.Service.Interface;
using DocumentFormat.OpenXml.Spreadsheet;

namespace CallAuditPortal1.Service
{
    public class AuditMonitoringService : IAuditMonitoringService
    {
        private readonly IAuditMonitoringDAL _auditMonitoringDAL;
        private readonly IEmailService _emailService;
        private readonly IMailProcessDAL _emailDAL;
        private readonly ILogger<AuditMonitoringService> _logger;
        public AuditMonitoringService(IAuditMonitoringDAL auditMonitoringDAL, IEmailService emailService, IMailProcessDAL mailDAL, ILogger<AuditMonitoringService> logger)
        {
            _auditMonitoringDAL = auditMonitoringDAL;
            _emailService = emailService;
            _emailDAL = mailDAL;
            _logger = logger;
        }

        public async Task<string> SubmitToBranch(SubmitBranchRequest request)
        {
            try
            {
                var result = await _auditMonitoringDAL.SubmitToBranch(request);
                if (!string.IsNullOrWhiteSpace(result.sessionId))
                {
                    string process = "SUBMIT_PROCESS";
                    var mailList = await _emailDAL.GetMailData(process, result.sessionId, "ALL_DATA");

                    foreach (var item in mailList)
                    {
                        try
                        {
                            await _emailDAL.UpdateMailStatusAsync(item.RowIds, MailStatus.Processing);
                            var emailModel = new Email
                            {
                                To = item.MailTo,
                                CC = item.MailCc,
                                MailSubject = item.Subject,
                                ShipToCode = item.ShipToCode,
                                CompanyName = item.CompanyName,
                                AuditMonth = item.AuditMonth,
                                LastDate = item.ValidTill,
                                Rows = item.Rows
                            };
                            await _emailService.SendEmailAsync(emailModel, process);
                            _logger.LogInformation(
                                "Process: {ProcessCode}, RowIds: {RowIds}, Mail sent successfully to {Email}",
                                process,
                                item.RowIds,
                                emailModel.To);
                            await _emailDAL.UpdateMailStatusAsync(item.RowIds, MailStatus.Success);
                        }catch(Exception ex)
                        {
                            _logger.LogError(
                               ex,
                               "Process: {ProcessCode}, RowIds: {RowIds}, Failed to send mail to {Email}",
                               process,
                               item.RowIds,
                               item.MailTo);
                            await _emailDAL.UpdateMailStatusAsync(item.RowIds, MailStatus.Retrying, ex.Message);
                        }
                    }
                }
                return result.msg;
            }
            catch
            {
                throw;
            }
        }
        
        public async Task<string> Reject(RejectRequest request)
        {
            return await _auditMonitoringDAL.Reject(request);
        }

        public Task<byte[]> Download(DownloadRequest request)
        {
            return  _auditMonitoringDAL.Download(request);
        }

      
    }


}
