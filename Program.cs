using CallAuditPortal1.Model;
using CallAuditPortal1.Service;
using CallAuditPortal1.Service.BAL;
using CallAuditPortal1.Service.DAL;
using CallAuditPortal1.Service.Interface;
using Microsoft.EntityFrameworkCore;
using Quartz;
using Oracle.ManagedDataAccess.Client;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<OracleConnection>(x =>
    new OracleConnection(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container.


builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddTransient<DatabaseConnection>();
builder.Services.AddTransient<AuditBAL>();
builder.Services.AddTransient<AuditDAL>();
builder.Services.AddScoped<IDataLoaderService, DataLoaderService>();
builder.Services.AddScoped<IDataLoaderDAL, DataLoaderDAL>();
builder.Services.AddScoped<IAuditMonitoringDAL, AuditMonitoringDAL>();
builder.Services.AddScoped<IAuditMonitoringService, AuditMonitoringService>();
builder.Services.AddScoped<IReviewProcessDAL, ReviewProcessDAL>();
builder.Services.AddScoped<IAuditEvaluationProcessDAL, AuditEvaluationProcessDAL>();
builder.Services.AddScoped<IFileUploadBAL, FileUploadBAL>();
builder.Services.AddScoped<IReportDAL,ReportDAL>();


builder.Services.AddDbContext<DbContext>(options =>
{
  options.UseOracle(builder.Configuration.GetConnectionString("OracleConnection"));
});

// Add Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
  options.AddPolicy("AllowAll",
      builder =>
      {
        builder.AllowAnyOrigin()
                 .AllowAnyHeader()
                 .AllowAnyMethod()
                 .WithExposedHeaders("Content-Disposition");
      });
});


var app = builder.Build();

app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.MapOpenApi();

  app.UseSwagger();      // Generates swagger.json
  app.UseSwaggerUI();    // Swagger UI
}
app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
