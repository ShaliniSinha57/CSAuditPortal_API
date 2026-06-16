using CallAuditPortal1.Model.RequestDTO;
using CallAuditPortal1.Model.ResponseDTO;
using CallAuditPortal1.Service.Interface;
using OfficeOpenXml;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System.Data;
using System.Dynamic;

namespace CallAuditPortal1.Service.DAL
{
    public class ReportDAL: IReportDAL
    {
        
            private readonly IConfiguration _configuration;
            public ReportDAL(IConfiguration configuration)
            {
                _configuration = configuration;
            }
        public async Task<byte[]> DownloadFeedbackReport(ExportExcel report)
        {
            using OracleConnection con = new OracleConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            await con.OpenAsync();

            using OracleCommand cmd = new OracleCommand(
                "",
                con);

            cmd.CommandType = CommandType.StoredProcedure;

            string selectedIds = string.Join(",", report.SelectedIds);

            cmd.Parameters.Add("p_selected_ids", OracleDbType.Varchar2)
                .Value = selectedIds;

            cmd.Parameters.Add("p_err", OracleDbType.Varchar2, 4000)
                .Direction = ParameterDirection.Output;

            cmd.Parameters.Add("p_result", OracleDbType.RefCursor)
                .Direction = ParameterDirection.Output;

            await cmd.ExecuteNonQueryAsync();

            string errorMessage = cmd.Parameters["p_err"].Value?.ToString();

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                throw new Exception(errorMessage);
            }

            OracleRefCursor cursor =
                (OracleRefCursor)cmd.Parameters["p_result"].Value;

            using OracleDataReader reader = cursor.GetDataReader();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using ExcelPackage package = new ExcelPackage();

            var worksheet = package.Workbook.Worksheets.Add("Feedback Status Report");

            int row = 1;

            // Header
            for (int col = 0; col < reader.FieldCount; col++)
            {
                worksheet.Cells[row, col + 1].Value = reader.GetName(col);
                worksheet.Cells[row, col + 1].Style.Font.Bold = true;
            }

            row++;

            // Data
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

            if (worksheet.Dimension != null)
            {
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
            }

            return package.GetAsByteArray();
        }

        public async Task<byte[]> DownloadSummaryStatusReport(ExportSummaryExcel export)
        {
            using OracleConnection con = new OracleConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            await con.OpenAsync();

            using OracleCommand cmd = new OracleCommand(
                "",
                con);

            cmd.CommandType = CommandType.StoredProcedure;

            // Convert selected IDs to comma-separated string
            string selectedIds = export.SelectedIds != null && export.SelectedIds.Any()
                ? string.Join(",", export.SelectedIds)
                : string.Empty;

            // Input Parameter
            cmd.Parameters.Add("p_selected_ids", OracleDbType.Varchar2)
                .Value = selectedIds;

            // Output Parameters
            cmd.Parameters.Add("p_err", OracleDbType.Varchar2, 4000)
                .Direction = ParameterDirection.Output;

            cmd.Parameters.Add("p_result", OracleDbType.RefCursor)
                .Direction = ParameterDirection.Output;

            await cmd.ExecuteNonQueryAsync();

            // Check for errors
            string errorMessage = cmd.Parameters["p_err"].Value?.ToString();

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                throw new Exception(errorMessage);
            }

            OracleRefCursor cursor =
                (OracleRefCursor)cmd.Parameters["p_result"].Value;

            using OracleDataReader reader = cursor.GetDataReader();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using ExcelPackage package = new ExcelPackage();

            var worksheet = package.Workbook.Worksheets.Add("Summary Status Report");

            int row = 1;

            // Add Headers
            for (int col = 0; col < reader.FieldCount; col++)
            {
                worksheet.Cells[row, col + 1].Value = reader.GetName(col);
                worksheet.Cells[row, col + 1].Style.Font.Bold = true;
            }

            row++;

            // Add Data
            while (await reader.ReadAsync())
            {
                for (int col = 0; col < reader.FieldCount; col++)
                {
                    worksheet.Cells[row, col + 1].Value =
                        reader.IsDBNull(col)
                            ? string.Empty
                            : reader.GetValue(col);
                }

                row++;
            }

            // Auto-fit columns
            if (worksheet.Dimension != null)
            {
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
            }

            return package.GetAsByteArray();
        }

        public async Task<List<BranchResponse>> GetBranches()
        {
            List<BranchResponse> branches = new List<BranchResponse>();

            try
            {
                using OracleConnection con = new OracleConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                await con.OpenAsync();

                using OracleCommand cmd = new OracleCommand(
                    "CSNET_PLUS.CSNET_PLUS_REPORT_PKG.GET_BRANCHES",
                    con);

                cmd.CommandType = CommandType.StoredProcedure;

                // Output Parameters
                cmd.Parameters.Add("p_err",
                    OracleDbType.Varchar2,
                    4000).Direction = ParameterDirection.Output;

                cmd.Parameters.Add("p_result",
                    OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                await cmd.ExecuteNonQueryAsync();

                // Check for Oracle error message
                string errorMessage = cmd.Parameters["p_err"].Value?.ToString();

                if (!string.IsNullOrWhiteSpace(errorMessage))
                {
                    throw new Exception(errorMessage);
                }

                OracleRefCursor refCursor =
                    (OracleRefCursor)cmd.Parameters["p_result"].Value;

                using OracleDataReader reader = refCursor.GetDataReader();

                while (await reader.ReadAsync())
                {
                    branches.Add(new BranchResponse
                    {
                        BranchCode = reader["BRANCH_CODE"] == DBNull.Value
                            ? string.Empty
                            : reader["BRANCH_CODE"].ToString(),

                        BranchName = reader["BRANCH_NAME"] == DBNull.Value
                            ? string.Empty
                            : reader["BRANCH_NAME"].ToString()
                    });
                }

                return branches;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Error in GetBranches: {ex.Message}",
                    ex);
            }
        }

        public async Task<(int Count, List<dynamic> Data)> SearchSummaryStatusReport(
     SummaryStatusRequest request)
        {
            List<dynamic> data = new List<dynamic>();

            try
            {
                using (OracleConnection con = new OracleConnection(
                    _configuration.GetConnectionString("DefaultConnection")))
                {
                    await con.OpenAsync();

                    using (OracleCommand cmd = new OracleCommand(
                        "",con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        // Input Parameters
                        cmd.Parameters.Add(
                            "p_branch_code",
                            OracleDbType.Varchar2)
                            .Value = string.IsNullOrWhiteSpace(request.BranchCode)
                                ? DBNull.Value
                                : request.BranchCode;

                        cmd.Parameters.Add(
                            "p_month",
                            OracleDbType.Int32)
                            .Value = request.Month;

                        cmd.Parameters.Add(
                            "p_year",
                            OracleDbType.Int32)
                            .Value = request.Year;

                        cmd.Parameters.Add(
                            "p_user_role",
                            OracleDbType.Varchar2)
                            .Value = string.IsNullOrWhiteSpace(request.UserRole)
                                ? DBNull.Value
                                : request.UserRole;

                        cmd.Parameters.Add(
                            "p_logged_in_branch",
                            OracleDbType.Varchar2)
                            .Value = string.IsNullOrWhiteSpace(request.LoggedInBranch)
                                ? DBNull.Value
                                : request.LoggedInBranch;

                        // Output Parameters
                        cmd.Parameters.Add(
                            "p_err",
                            OracleDbType.Varchar2,
                            4000)
                            .Direction = ParameterDirection.Output;

                        cmd.Parameters.Add(
                            "p_count",
                            OracleDbType.Int32)
                            .Direction = ParameterDirection.Output;

                        cmd.Parameters.Add(
                            "p_result",
                            OracleDbType.RefCursor)
                            .Direction = ParameterDirection.Output;

                        await cmd.ExecuteNonQueryAsync();

                        // Check for Oracle errors
                        string errMsg = cmd.Parameters["p_err"].Value?.ToString();

                        if (!string.IsNullOrWhiteSpace(errMsg))
                        {
                            throw new Exception(errMsg);
                        }

                        // Read Ref Cursor
                        OracleRefCursor refCursor =
                            (OracleRefCursor)cmd.Parameters["p_result"].Value;

                        using (OracleDataReader reader = refCursor.GetDataReader())
                        {
                            while (await reader.ReadAsync())
                            {
                                var row = new ExpandoObject() as IDictionary<string, object>;

                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    row.Add(
                                        reader.GetName(i),
                                        reader.IsDBNull(i)
                                            ? null
                                            : reader.GetValue(i));
                                }

                                data.Add(row);
                            }
                        }

                        // Get total count
                        int count = 0;

                        if (cmd.Parameters["p_count"].Value != DBNull.Value &&
                            cmd.Parameters["p_count"].Value != null)
                        {
                            count = Convert.ToInt32(
                                cmd.Parameters["p_count"].Value);
                        }

                        return (count, data);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Error in SearchSummaryStatusReport: {ex.Message}" +
                    $" | Inner Exception: {ex.InnerException?.Message}");
            }
        }

        public async Task<(int Count, List<dynamic> Data)> SearchFeedbackStatusReport(
     FeedbackStatusRequest request)
        {
            List<dynamic> data = new List<dynamic>();

            try
            {
                using OracleConnection con = new OracleConnection(
                    _configuration.GetConnectionString("DefaultConnection"));

                await con.OpenAsync();

                using OracleCommand cmd = new OracleCommand(
                    "", con);

                cmd.CommandType = CommandType.StoredProcedure;

                // Input Parameters
                cmd.Parameters.Add("p_branch_code", OracleDbType.Varchar2)
                    .Value = string.IsNullOrWhiteSpace(request.BranchCode)
                        ? DBNull.Value
                        : request.BranchCode;

                cmd.Parameters.Add("p_month", OracleDbType.Int32)
                    .Value = request.Month;

                cmd.Parameters.Add("p_year", OracleDbType.Int32)
                    .Value = request.Year;

                cmd.Parameters.Add("p_user_role", OracleDbType.Varchar2)
                    .Value = string.IsNullOrWhiteSpace(request.UserRole)
                        ? DBNull.Value
                        : request.UserRole;

                cmd.Parameters.Add("p_logged_in_branch", OracleDbType.Varchar2)
                    .Value = string.IsNullOrWhiteSpace(request.LoggedInBranch)
                        ? DBNull.Value
                        : request.LoggedInBranch;

                // Output Parameters
                cmd.Parameters.Add("p_err", OracleDbType.Varchar2, 4000)
                    .Direction = ParameterDirection.Output;

                cmd.Parameters.Add("p_count", OracleDbType.Int32)
                    .Direction = ParameterDirection.Output;

                cmd.Parameters.Add("p_result", OracleDbType.RefCursor)
                    .Direction = ParameterDirection.Output;

                await cmd.ExecuteNonQueryAsync();

                // Check for Oracle error message
                string errMsg = cmd.Parameters["p_err"].Value?.ToString();

                if (!string.IsNullOrWhiteSpace(errMsg))
                {
                    throw new Exception(errMsg);
                }

                // Read Ref Cursor
                OracleRefCursor refCursor =
                    (OracleRefCursor)cmd.Parameters["p_result"].Value;

                using OracleDataReader reader = refCursor.GetDataReader();

                while (await reader.ReadAsync())
                {
                    IDictionary<string, object?> row = new ExpandoObject();

                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        row.Add(
                            reader.GetName(i),
                            reader.IsDBNull(i)
                                ? null
                                : reader.GetValue(i)
                        );
                    }

                    data.Add(row);
                }

                int count = 0;

                if (cmd.Parameters["p_count"].Value != DBNull.Value)
                {
                    count = Convert.ToInt32(
                        cmd.Parameters["p_count"].Value);
                }

                return (count, data);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Error in SearchFeedbackStatusReport: {ex.Message}",
                    ex);
            }
        }
    }
    }

