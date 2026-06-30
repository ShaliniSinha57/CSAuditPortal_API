namespace CallAuditPortal1.Model
{
    public class MailConcurrencyLimiter
    {
        public SemaphoreSlim Semaphore { get; } = new SemaphoreSlim(10);
    }
}
