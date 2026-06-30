using CallAuditPortal1.Service.BAL.Mail.Worker;

namespace CallAuditPortal1.Service.BAL.Mail.Processor
{
    public class MailProcessor : IMailProcessor
    {
        private readonly IEnumerable<IMailWorker> _worker;

        public MailProcessor(IEnumerable<IMailWorker> worker)
        {
            _worker = worker;
        }

        public async Task ProcessAsync()
        {
            await Task.WhenAll(_worker.Select(x => x.ProcessAsync()));
        }
    }
}
