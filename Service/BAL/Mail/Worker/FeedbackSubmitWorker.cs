using CallAuditPortal1.Model;
using CallAuditPortal1.Service.Interface;

namespace CallAuditPortal1.Service.BAL.Mail.Worker
{
    public class FeedbackSubmitWorker : BaseMailWorker
    {
        public FeedbackSubmitWorker(
            IEmailService emailService,
            IMailProcessDAL mailProcessDAL,
            MailConcurrencyLimiter limiter
            ) : base(emailService, mailProcessDAL, limiter) { }

        protected override string ProcessCode => "FEEDBACK_ESC_LGC";
    }
}
