using CallAuditPortal1.Service.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.CompilerServices;
using static CallAuditPortal1.Model.RequestDTO.EvaluationProcessRequest;

namespace CallAuditPortal1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EvaluationProcessController : ControllerBase
    {
        private readonly IAuditEvaluationProcessDAL _services;
        private readonly IFileUploadBAL _fileUpload;
        public EvaluationProcessController(IAuditEvaluationProcessDAL service)
        {
            _services = service;
            _configuration = configuration;
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

        [HttpPost("save-feedback")]
        public async Task<IActionResult> SaveFeedback([FromForm] SaveFeedbackRequest request)
        {
            try
            {
                request.AttachementUrl = await _fileUpload.SaveFile(request.Attachement);
                var response = await _services.SaveFeedbackStatus(request);
                return Ok(new
                {
                    status = "success",
                    response
                });
            }catch(Exception ex)
            {
                await _fileUpload.DeleteFile(request.AttachementUrl);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("SaveFeedbackStatus")]
        public async Task<string> SaveFeedbackStatus(SaveFeedbackRequest request)
        {
            try
            {
                using (OracleConnection con = new OracleConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    await con.OpenAsync();

                    using (OracleCommand cmd = new OracleCommand("CSNET_PLUS_REPORT_PKG.save_feedback", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("p_gsfs_receipt_no", OracleDbType.Varchar2).Value = request.GSFS_ReceiptNo;
                        cmd.Parameters.Add("p_audit_type_id", OracleDbType.Varchar2).Value = request.AuditTypeId;
                        cmd.Parameters.Add("p_attachement_name", OracleDbType.Varchar2).Value = request.GSFS_ReceiptNo;
                        cmd.Parameters.Add("p_status", OracleDbType.Varchar2).Value = request.Status;
                        cmd.Parameters.Add("p_remarks", OracleDbType.Varchar2).Value = request.Remark;
                        cmd.Parameters.Add("p_f_by", OracleDbType.Varchar2).Value = request.ActionBy;

                        cmd.Parameters.Add("p_msg", OracleDbType.Varchar2, 4000).Direction = ParameterDirection.Output;

                        await cmd.ExecuteNonQueryAsync();

                        return cmd.Parameters["p_msg"].Value.ToString();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
