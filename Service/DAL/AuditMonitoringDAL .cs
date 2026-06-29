using CallAuditPortal1.Model.RequestDTO;
using CallAuditPortal1.Service.Interface;
using OfficeOpenXml;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System.Data;
using System.Text.Json;
using OracleCommand = Oracle.ManagedDataAccess.Client.OracleCommand;
using OracleConnection = Oracle.ManagedDataAccess.Client.OracleConnection;



namespace CallAuditPortal1.Service.DAL
{
    public class AuditMonitoringDAL : IAuditMonitoringDAL
    {
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        public AuditMonitoringDAL(IConfiguration configuration, IEmailService emailService)
        {
            _configuration = configuration;
            _emailService = emailService;
        }
        public async Task<(string msg, string sessionId)> SubmitToBranch(SubmitBranchRequest request)
        {
            using OracleConnection con = new OracleConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            await con.OpenAsync();

            using OracleCommand cmd =
                new OracleCommand("CSNET_PLUS_REPORT_PKG.submit_reject", con);

            cmd.CommandType = CommandType.StoredProcedure;

            string receiptNos = string.Join(",", request.GSFS_Receipt_Nos);

            cmd.Parameters.Add("p_audit_type_id", OracleDbType.Int32).Value = request.AuditTypeId;
            cmd.Parameters.Add("p_gsfs_receipt_nos", OracleDbType.Varchar2).Value = receiptNos;
            cmd.Parameters.Add("p_user_id", OracleDbType.Varchar2).Value = "testing";
            cmd.Parameters.Add("p_action",  OracleDbType.Varchar2).Value = "SUBMIT_PROCESS";
            cmd.Parameters.Add("p_msg", OracleDbType.Varchar2,500).Direction = ParameterDirection.Output;
            cmd.Parameters.Add("p_session_id", OracleDbType.Varchar2,500).Direction = ParameterDirection.Output;
            //cmd.Parameters.Add("p_success_receipts", OracleDbType.Varchar2,400).Direction = ParameterDirection.Output;
            //cmd.Parameters.Add("p_error_receipts", OracleDbType.Varchar2,400).Direction = ParameterDirection.Output;

            await cmd.ExecuteNonQueryAsync();
            string msg = cmd.Parameters["p_msg"].Value?.ToString();
            string sessionId = cmd.Parameters["p_session_id"].Value?.ToString();
            //string successReceipt = cmd.Parameters["p_success_receipts"].Value?.ToString();
            //string errorReceipt = cmd.Parameters["p_error_receipts"].Value?.ToString();
            //return (msg,sessionId,successReceipt,errorReceipt);
            return (msg,sessionId);
        }

      
        public async Task<string> Reject(RejectRequest request)
        {
            using (OracleConnection con = new OracleConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await con.OpenAsync();
                using (OracleCommand cmd = new OracleCommand("CSNET_PLUS_REPORT_PKG.submit_reject", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.BindByName = true;
                    string receiptNos = string.Join(",", request.GSFS_Receipt_Nos);
                    cmd.Parameters.Add("p_audit_type_id", OracleDbType.Int32).Value = request.AuditTypeId;
                    cmd.Parameters.Add("p_gsfs_receipt_nos", OracleDbType.Varchar2).Value = receiptNos;
                    cmd.Parameters.Add("p_action", OracleDbType.Varchar2).Value = "REJECT";
                    cmd.Parameters.Add("p_msg", OracleDbType.Varchar2, 500).Direction = ParameterDirection.Output;
                    await cmd.ExecuteNonQueryAsync();
                    var message = cmd.Parameters["p_msg"].Value?.ToString();
                    return message;
                }

            }
        }


        public async Task<byte[]> Download(DownloadRequest request)
        {
            using OracleConnection con = new OracleConnection(
                _configuration.GetConnectionString("DefaultConnection"));
            await con.OpenAsync();
            using OracleCommand cmd = new OracleCommand("CSNET_PLUS_REPORT_PKG.download_audit_data", con);
            cmd.CommandType = CommandType.StoredProcedure;
            string receiptNos = string.Join(",", request.GSFS_Receipt_Nos);
            cmd.Parameters.Add("p_audit_type_id", OracleDbType.Int32).Value = request.AuditTypeId;
            cmd.Parameters.Add("p_gsfs_receipt_nos", OracleDbType.Varchar2).Value = receiptNos;
            cmd.Parameters.Add("p_result", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
            cmd.Parameters.Add("p_msg", OracleDbType.Varchar2, 500).Direction = ParameterDirection.Output;

            await cmd.ExecuteNonQueryAsync();
            var message = cmd.Parameters["p_msg"].Value?.ToString();

            OracleRefCursor cursor = (OracleRefCursor)cmd.Parameters["p_result"].Value;
            using OracleDataReader reader = cursor.GetDataReader();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using ExcelPackage package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Audit Report");
            int row = 1;
            for (int col = 0; col < reader.FieldCount; col++)
            {
                worksheet.Cells[row, col + 1].Value =
                    reader.GetName(col);
                worksheet.Cells[row, col + 1]
                         .Style.Font.Bold = true;
            }
            row++;
            while (await reader.ReadAsync())
            {
                for (int col = 0; col < reader.FieldCount; col++)
                {
                    worksheet.Cells[row, col + 1].Value =
                        reader.IsDBNull(col)
                            ? ""
                            : reader.GetValue(col);
                }
                row++;
            }
           worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
           return package.GetAsByteArray();
        }


    }
}

