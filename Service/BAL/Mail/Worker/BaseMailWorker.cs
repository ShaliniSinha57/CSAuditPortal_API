using System.Diagnostics;
using AspNetCoreGeneratedDocument;
using CallAuditPortal1.Model;
using CallAuditPortal1.Model.RequestDTO;
using CallAuditPortal1.Service.Helper;
using CallAuditPortal1.Service.Interface;

namespace CallAuditPortal1.Service.BAL.Mail.Worker
{
    public abstract class BaseMailWorker : IMailWorker
    {
        private readonly IEmailService _mailService;
        private readonly IMailProcessDAL _mailDataLoader;
        private readonly MailConcurrencyLimiter _limiter;
        private readonly ILogger<BaseMailWorker> _logger;

        public BaseMailWorker(IEmailService mailService, IMailProcessDAL mailDataLoader, MailConcurrencyLimiter limiter, ILogger<BaseMailWorker> logger)
        {
            _mailService = mailService;
            _mailDataLoader = mailDataLoader;
            _limiter = limiter;
            _logger = logger;
        }

        protected abstract string ProcessCode { get; }

        public async Task ProcessAsync()
        {
            while (true)
            {
                _logger.LogInformation($"Process started with {ProcessCode}");
                var mailData = await _mailDataLoader.GetMailData(ProcessCode, null, "PAGINATION");

                _logger.LogInformation("Fetched {Count} mails for {ProcessCode}",mailData.Count,ProcessCode);

                if (mailData == null || !mailData.Any())
                    break;
                var task = mailData.Select(ProcessSingleMailAsync);

                await Task.WhenAll(task);
            }
        }

        private async Task ProcessSingleMailAsync(MailResponseModel mail)
        {
            await _limiter.Semaphore.WaitAsync();
            try
            {
                var emailModel = new Email
                {
                    To = mail.MailTo,
                    CC = mail.MailCc,
                    MailSubject = mail.Subject,
                    Header = mail.Header,
                    Footer = mail.Footer,
                    ShipToCode = mail.ShipToCode,
                    CompanyName = mail.CompanyName,
                    AuditMonth = mail.AuditMonth,
                    LastDate = mail.ValidTill,
                    Rows = mail.Rows
                };
                await _mailService.SendEmailAsync(emailModel, ProcessCode);
                _logger.LogInformation("Mail sent successfully to {Email}",emailModel.To);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                _limiter.Semaphore.Release();
            }
        }
    }
}
