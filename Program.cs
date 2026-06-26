using CallAuditPortal1.Model;
using CallAuditPortal1.Model.RequestDTO;
using CallAuditPortal1.Service;
using CallAuditPortal1.Service.BAL;
using CallAuditPortal1.Service.BAL.Schedular;
using CallAuditPortal1.Service.DAL;
using CallAuditPortal1.Service.Helper;
using CallAuditPortal1.Service.Interface;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;
using Quartz;
using System.Configuration;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<SmtpSettings>(
    builder.Configuration.GetSection("SmtpSettings")
);

builder.Services.AddScoped<OracleConnection>(x =>
    new OracleConnection(
        builder.Configuration.GetConnectionString("DefaultConnection")));
// Add services to the container.

builder.Services.AddControllersWithViews();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddOpenApi();
builder.Services.AddScoped<DatabaseConnection>();
builder.Services.AddScoped<AuditBAL>();
builder.Services.AddScoped<AuditDAL>();
builder.Services.AddScoped<IDataLoaderService, DataLoaderService>();
builder.Services.AddScoped<IDataLoaderDAL, DataLoaderDAL>();
builder.Services.AddScoped<IAuditMonitoringDAL, AuditMonitoringDAL>();
builder.Services.AddScoped<IAuditMonitoringService, AuditMonitoringService>();
builder.Services.AddScoped<IReviewProcessDAL, ReviewProcessDAL>();
builder.Services.AddScoped<IAuditEvaluationProcessDAL, AuditEvaluationProcessDAL>();
builder.Services.AddScoped<IReportDAL, ReportDAL>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddSingleton<RazorViewRenderer>();


//builder.Services.AddDbContext<DbContext>(options =>
//{
//    options.UseOracle(builder.Configuration.GetConnectionString("OracleConnection"));
//});

builder.Services.Configure<FormOptions>(options =>
{
    var sendMailJobKey = new JobKey("SendMailSchedular");

    q.AddJob<SendMailSchedular>(opts => opts.WithIdentity(sendMailJobKey));
    q.AddTrigger(opts => opts
        .ForJob(sendMailJobKey)
        .WithIdentity("SendMailRecordsTrigger")
        .WithCronSchedule("0/15 * * * * ?"));
});





builder.Services.AddQuartzHostedService(options =>
{
    options.Limits.MaxRequestBodySize = 100 * 1024 * 1024; // 100 MB
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
    //app.MapOpenApi();

    app.UseSwagger();      // Generates swagger.json
    app.UseSwaggerUI();    // Swagger UI
}
app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
