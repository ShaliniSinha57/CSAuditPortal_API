using CallAuditPortal1.Service.BAL.Mail.Processor;
using CallAuditPortal1.Service.Interface;
using Quartz;
using System.Text;

namespace CallAuditPortal1.Service.BAL.Mail.Schedular
{
   
    public class SendMailSchedular : IJob
    {
        private readonly IMailProcessor _mailProcessor;
        public SendMailSchedular(IMailProcessor mailProcessor) { 
            _mailProcessor = mailProcessor;
        }

       public async Task Execute(IJobExecutionContext context)
        {
            await _mailProcessor.ProcessAsync();
        }
    }
}
