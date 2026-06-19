using Quartz;

namespace CallAuditPortal1.Service.BAL.Schedular
{
    public class SendMailSchedular : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            Console.WriteLine($"Job Started: {DateTime.Now}");
            await Task.CompletedTask;
            Console.WriteLine($"Job Ended: {DateTime.Now}");
        }
    }
}
