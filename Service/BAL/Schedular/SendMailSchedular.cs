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

            {
                await _auditMonitoringService.SendMailForAccepted("123","admin");
                await _auditMonitoringService.SendMailForFeedback("123", "admin");
                await _auditMonitoringService.SendMailForRejected("123", "admin");
                await Task.CompletedTask;
               
            }

        }
    }
}
