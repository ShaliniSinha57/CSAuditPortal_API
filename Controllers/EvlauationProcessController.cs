using CallAuditPortal1.Service.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CallAuditPortal1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EvaluationProcessController : ControllerBase
    {
        private readonly IAuditEvaluationProcessDAL _services;
        public EvaluationProcessController(IAuditEvaluationProcessDAL service)
        {
            _services = service;
        }
        [HttpGet("Get_Evaluation_Process_Data")]
        public async Task<IActionResult> Get_Evaluation_Process_Data(string receiptNo, int audit_typeId)
        {
            try
            {
                var data = await _services.Get_Evaluation_Data(receiptNo, audit_typeId);
                return Ok(new
                {
                    success = "Success",
                    data
                });
            }catch(Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
