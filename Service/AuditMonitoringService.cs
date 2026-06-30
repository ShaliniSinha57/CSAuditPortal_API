using System.Data;
using System.Diagnostics;
using CallAuditPortal1.Model;
using CallAuditPortal1.Model.RequestDTO;
using CallAuditPortal1.Service.Helper;
using CallAuditPortal1.Service.Interface;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;

namespace CallAuditPortal1.Service
{
    public class AuditMonitoringService : IAuditMonitoringService
    {
        private readonly IAuditMonitoringDAL _auditMonitoringDAL;
        private readonly IEmailService _emailService;
        private readonly IMailProcessDAL _emailDAL;
        public AuditMonitoringService(IAuditMonitoringDAL auditMonitoringDAL, IEmailService emailService, IMailProcessDAL mailDAL)
        {
            _auditMonitoringDAL = auditMonitoringDAL;
            _emailService = emailService;
            _emailDAL = mailDAL;
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
                        var emailModel = new Email
                        {
                            To = item.MailTo,
                            CC = item.MailCc,
                            MailSubject = item.Subject,
                            Header = item.Header,
                            Footer = item.Footer,
                            ShipToCode = item.ShipToCode,
                            CompanyName = item.CompanyName,
                            AuditMonth = item.AuditMonth,
                            LastDate = item.ValidTill,
                            Rows = item.Rows
                        };
                        await _emailService.SendEmailAsync(emailModel, process);
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
