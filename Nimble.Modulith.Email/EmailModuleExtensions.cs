using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nimble.Modulith.Email.Integrations;
using Nimble.Modulith.Email.Interfaces;
using Nimble.Modulith.Email.Services;
using Serilog;

namespace Nimble.Modulith.Email;

public static class EmailModuleExtensions
{
    public static WebApplicationBuilder AddEmailModuleServices(this WebApplicationBuilder builder, ILogger logger)
    {
        logger.Information("Adding Email module services...");
        builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("Email"));
        builder.Services.AddSingleton<IEmailSender, SmtpEmailSender>();
        builder.Services.AddSingleton(typeof(IQueueService<>), typeof(ChannelQueueService<>));
        builder.Services.AddScoped<SendEmailCommandHandler>();
        builder.Services.AddHostedService<EmailSendingBackgroundWorker>();
        logger.Information("Email module services added");
        return builder;
    }
}