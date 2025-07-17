using System.Text;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TMS.API.Middleware;
using TMS.Repository.Data;
using TMS.Repository.Implementations;
using TMS.Repository.Interfaces;
using TMS.Service;
using TMS.Service.Implementations;
using TMS.Service.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        policy =>
        {
            policy.WithOrigins("http://127.0.0.1:5500")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});
// Add services to the container.

builder.Services.AddDbContext<TmsContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("TMSDbConnection")));

// builder.Services.AddAutoMapper(typeof(MappingConfig).Assembly);

builder.Services.AddControllersWithViews();
builder.Services.AddControllers();

builder.Services.AddHangfire(x =>
    x.UsePostgreSqlStorage(builder.Configuration.GetConnectionString("TMSDbConnection")));
builder.Services.AddHangfireServer();

builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IJWTService, JWTService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICountryService, CountryService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IHolidayService, HolidayService>();
builder.Services.AddScoped<ITaskActionService, TaskActionService>();
builder.Services.AddScoped<ILogService, LogService>();
builder.Services.AddScoped<ITaskReminderService, TaskReminderService>();
builder.Services.AddScoped<IOtpService, OtpService>();
builder.Services.AddScoped<IPdfService, PdfService>();
builder.Services.AddScoped<ISystemConfigurationService, SystemConfigurationService>();
builder.Services.AddHttpClient<HolidayService>();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICountryRepository, CountryRepository>();
builder.Services.AddScoped<ITimezoneRepository, TimezoneRepository>();
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<ITaskAssignRepository,TaskAssignRepository >();
builder.Services.AddScoped<INotificationRepository,NotificationRepository >();
builder.Services.AddScoped<ITaskActionRepository,TaskActionRepository>();
builder.Services.AddScoped<ILogRepository, LogRepository>();
builder.Services.AddScoped<IUserOtpRepository, UserOtpRepository>();
builder.Services.AddScoped<ISystemConfigurationRepository, SystemConfigurationRepository>();
builder.Services.AddHttpClient<CountryRepository>();


builder.Services.AddSignalR();

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var token = context.Request.Cookies["AuthToken"];
                if (!string.IsNullOrEmpty(token))
                {
                    context.Request.Headers["Authorization"] = "Bearer " + token;
                }
                return System.Threading.Tasks.Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                context.Response.StatusCode = 401;
                return context.Response.WriteAsync("Unauthorized");
            },
            OnForbidden = context =>
            {
                context.Response.StatusCode = 403;
                return context.Response.WriteAsync("Forbidden");
            }
        };
    });


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "TMS", Version = "v1" });

    // Add JWT Bearer
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHangfireDashboard("/dashboardHangfire");

app.UseMiddleware<ExceptionMiddleware>();
app.UseRouting();
app.UseCors("AllowSpecificOrigin");
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapHub<ReminderHub>("/reminderHub");
});

using (var scope = app.Services.CreateScope())
{
    var jobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
    var reminderService = scope.ServiceProvider.GetRequiredService<ITaskReminderService>();
    var authService = scope.ServiceProvider.GetRequiredService<IAuthenticationService>();
    
    jobManager.AddOrUpdate(
        "daily-task-reminder",
        () => reminderService.DueDateReminderService(),
        "30 9 * * *",
        new RecurringJobOptions { TimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time") });
    
    jobManager.AddOrUpdate(
        "daily-overdue-reminder",
        () => reminderService.OverdueReminderService(),
        "30 9 * * *",
        new RecurringJobOptions { TimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time") });

    jobManager.AddOrUpdate(
        "recurrent-task-job",
        () => reminderService.RecurrentTaskAssignmentService(),
        "30 9 * * *",
        new RecurringJobOptions { TimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time") });

    jobManager.AddOrUpdate(
        "unblock-user",
        () => authService.UnblockUser(),
        "*/1 * * * *",
        new RecurringJobOptions { TimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time") });
    
    jobManager.AddOrUpdate(
        "unblock-user",
        () => authService.ResetInvalidLoginAttempt(),
        "*/1 * * * *",
        new RecurringJobOptions { TimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time") });
}

app.Run();
