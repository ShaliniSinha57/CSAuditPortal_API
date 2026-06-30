using System.Configuration;
using System.Data;
using System.Net.NetworkInformation;
using System.Text.Json;
using CallAuditPortal1.Model;
using CallAuditPortal1.Model.RequestDTO;
using CallAuditPortal1.Service.Interface;
using DocumentFormat.OpenXml.Drawing.Charts;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

namespace CallAuditPortal1.Service.DAL
{
    public class MailProcessDAL : IMailProcessDAL
    {
        private readonly IConfiguration _configuration;
        public MailProcessDAL(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<List<MailResponseModel>> GetMailData(string screenType, string sessionId, string caseType)
        {
            List<MailResponseModel> mailList = new();
            using OracleConnection con = new OracleConnection(
                _configuration.GetConnectionString("DefaultConnection"));
            await con.OpenAsync();

            using OracleCommand cmd = new OracleCommand("CSNET_PLUS_MAIL_PKG.get_mail_excel_data", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("p_event", OracleDbType.Varchar2).Value = screenType;
            cmd.Parameters.Add("p_row_ids", OracleDbType.Varchar2).Value = sessionId;
            cmd.Parameters.Add("p_case", OracleDbType.Varchar2).Value = caseType;
            cmd.Parameters.Add("p_res", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
            cmd.Parameters.Add("p_msg", OracleDbType.Varchar2, 4000).Direction = ParameterDirection.Output;
            await cmd.ExecuteNonQueryAsync();
            string message = DBHelper.GetString(cmd.Parameters, "p_msg");
            if (!string.IsNullOrWhiteSpace(message) &&
                !message.Equals("SUCCESS", StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception(message);
            }

            using OracleDataReader reader = ((OracleRefCursor)cmd.Parameters["p_mail_data"].Value).GetDataReader();
            while (await reader.ReadAsync())
            {
                MailResponseModel model = new MailResponseModel();
                model.ShipToCode = reader["SHIP_TO_CODE"]?.ToString() ?? "";
                model.AuditMonth = reader["AUDIT_MONTH"]?.ToString() ?? "";
                model.ValidTill = reader["VALID_TILL"]?.ToString() ?? "";
                model.CompanyName = reader["COMPANY_NAME"]?.ToString() ?? "";
                model.EventType = reader["EVENT_TYPE"]?.ToString() ?? "";
                model.CreatedBy = reader["CREATED_BY"]?.ToString() ?? "";
                model.MailTo = reader["MAIL_TO"]?.ToString() ?? "";
                model.MailCc = reader["MAIL_CC"]?.ToString() ?? "";
                string summaryJson = reader["SUMMARY_JSON"]?.ToString() ?? "[]";
                model.Rows = ConvertJsonToRows(summaryJson);
                mailList.Add(model);
            }
            return mailList;
        }
        private List<MailDynamicRow> ConvertJsonToRows(string json)
        {
            try
            {
                List<MailDynamicRow> rows = new();
                if (string.IsNullOrWhiteSpace(json))
                    return rows;
                using JsonDocument document = JsonDocument.Parse(json);
                foreach (JsonElement item in document.RootElement.EnumerateArray())
                {
                    MailDynamicRow row = new MailDynamicRow();
                    row.AuditHead = item.GetProperty("audit_head").GetString() ?? "";
                    row.NewUpload = GetIntOrDefault(item, "new_upload");
                    row.PreviousPending = GetIntOrDefault(item, "pending");

                    row.AcceptedCount = GetIntOrDefault(item, "accepted");

                    row.RejectedCount = GetIntOrDefault(item, "rejected");
                    row.FeedbackSubmitCount = GetIntOrDefault(item, "feedback");

                    row.Total =
                        row.NewUpload +
                        row.PreviousPending +
                        row.AcceptedCount +
                        row.RejectedCount +
                        row.FeedbackSubmitCount;
                    rows.Add(row);
                }
                return rows;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        private static int GetIntOrDefault(JsonElement item, string propertyName)
        {
            if (!item.TryGetProperty(propertyName, out var value))
                return 0;

            if (value.ValueKind == JsonValueKind.Null)
                return 0;

            if (value.ValueKind == JsonValueKind.Number)
                return value.GetInt32();

            if (value.ValueKind == JsonValueKind.String &&
                int.TryParse(value.GetString(), out var result))
                return result;

            return 0;
        }


        public async Task UpdateMailStatusAsync(string rowIds, string status, string msg)
        {
            try
            {
                using (OracleConnection con = new OracleConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    await con.OpenAsync();
                    using (OracleCommand cmd = new OracleCommand("CSNET_PLUS_MAIL_PKG.update_mail_status", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("p_row_ids", OracleDbType.Varchar2).Value = rowIds;
                        cmd.Parameters.Add("p_status", OracleDbType.Varchar2).Value = status;
                        cmd.Parameters.Add("p_error", OracleDbType.Varchar2).Value = msg;

                        // Output
                        cmd.Parameters.Add("p_msg", OracleDbType.Varchar2, 4000).Direction= ParameterDirection.Output;
                        await cmd.ExecuteNonQueryAsync();

                        string message = DBHelper.GetString(cmd.Parameters, "p_msg");
                    }
                }
            }
            catch
            {

            }
        }
    }
}
