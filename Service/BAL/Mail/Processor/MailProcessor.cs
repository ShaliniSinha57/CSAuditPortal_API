using CallAuditPortal1.Service.BAL.Mail.Worker;

namespace CallAuditPortal1.Service.BAL.Mail.Processor
{
    public class MailProcessor : IMailProcessor
    {
        private readonly AcceptedHOWorker _acceptedWorker;
        private readonly RejectedHOWorker _rejectedHOWorker;
        private readonly FeedbackSubmitWorker _feedbackSubmitWorker;

        public MailProcessor(AcceptedHOWorker acceptedWorker, RejectedHOWorker rejectedHOWorker, FeedbackSubmitWorker feedbackSubmitWorker)
        {
            _acceptedWorker = acceptedWorker;
            _rejectedHOWorker = rejectedHOWorker;
            _feedbackSubmitWorker = feedbackSubmitWorker;
        }

        public async Task ProcessAsync()
        {
            await Task.WhenAll(
                _acceptedWorker.ProcessAsync(),
                _rejectedHOWorker.ProcessAsync(),
                _feedbackSubmitWorker.ProcessAsync());
        }
    }
}
