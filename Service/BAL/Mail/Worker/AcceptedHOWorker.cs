using CallAuditPortal1.Model;
using CallAuditPortal1.Service.Interface;

namespace CallAuditPortal1.Service.BAL.Mail.Worker
{
    public class AcceptedHOWorker : BaseMailWorker
    {
        public AcceptedHOWorker(
            IEmailService emailService,
            IMailProcessDAL mailProcessDAL,
            MailConcurrencyLimiter limiter,
            ILogger<BaseMailWorker> logger
            ):base(emailService, mailProcessDAL, limiter, logger) { }

        protected override string ProcessCode => "HO_ACCEPT";
    }
}
