using CallAuditPortal1.Model;
using CallAuditPortal1.Model.RequestDTO;
using CallAuditPortal1.Service;
using CallAuditPortal1.Service.BAL;
using CallAuditPortal1.Service.BAL.Mail.Processor;
using CallAuditPortal1.Service.BAL.Mail.Schedular;
using CallAuditPortal1.Service.BAL.Mail.Worker;
using CallAuditPortal1.Service.DAL;
using CallAuditPortal1.Service.Helper;
using CallAuditPortal1.Service.Interface;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;
using Quartz;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<SmtpSettings>(
    builder.Configuration.GetSection("SmtpSettings")
);

builder.Services.AddScoped<OracleConnection>(x =>
    new OracleConnection(
        builder.Configuration.GetConnectionString("DefaultConnection")));
// Add services to the container.

builder.Services.AddControllersWithViews();
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

builder.Services.AddSingleton<RazorViewRenderer>();

// Mail Services 
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IMailProcessDAL, MailProcessDAL>();
builder.Services.AddScoped<IMailWorker, MailWorker>();
builder.Services.AddScoped<IMailProcessor, MailProcessor>();

builder.Services.AddQuartz(q =>
{
    var jobKey = new JobKey("SendMailSchedular");

    q.AddJob<SendMailSchedular>(opts =>
        opts.WithIdentity(jobKey));

    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity("SendMailTrigger")

        // Every day at 7 PM
        .WithCronSchedule("0 0 19 * * ?"));
});

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 100 * 1024 * 1024; //100 MB
});



builder.Services.AddQuartzHostedService(options =>
{
    options.WaitForJobsToComplete = true;
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