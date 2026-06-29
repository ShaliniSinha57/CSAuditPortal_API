using CallAuditPortal1.Service.Interface;
using Quartz;
using System.Text;

namespace CallAuditPortal1.Service.BAL.Schedular
{
   
    public class SendMailSchedular : IJob
    {
        private readonly IAuditMonitoringDAL _auditMonitoringDAL;
        private readonly IEmailService _emailService;
        private readonly IAuditMonitoringService _auditMonitoringService;

        public SendMailSchedular(
            IAuditMonitoringDAL auditMonitoringDAL,
            IEmailService emailService,
            IAuditMonitoringService auditMonitoringService)
        {
            _auditMonitoringDAL = auditMonitoringDAL;
            _emailService = emailService;
            _auditMonitoringService = auditMonitoringService;
        }

       
       public async Task Execute(IJobExecutionContext context)
        {
            string sessionId = Guid.NewGuid().ToString();

            //await _auditMonitoringService.SendMailByScreenType("SYSTEM", "HO_ACCEPT", sessionId);
            //await _auditMonitoringService.SendMailByScreenType("SYSTEM", "HO_REJECT", sessionId);
            //await _auditMonitoringService.SendMailByScreenType("SYSTEM", "FEEDBACK_ESC_LGC", sessionId);

        }
    }
}
