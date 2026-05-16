using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nimble.Modulith.Email.Integrations;
using Nimble.Modulith.Email.Interfaces;

namespace Nimble.Modulith.Email;

public class EmailSendingBackgroundWorker : BackgroundService
{
    private readonly IQueueService<EmailToSend> _queueService;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<EmailSendingBackgroundWorker> _logger;

    public EmailSendingBackgroundWorker(IQueueService<EmailToSend> queueService, IEmailSender emailSender, ILogger<EmailSendingBackgroundWorker> logger)
    {
        _queueService = queueService;
        _emailSender = emailSender;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Email background worker started");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                bool processedAny = false;
                while (true)
                {
                    try
                    {
                        using var cts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
                        cts.CancelAfter(TimeSpan.FromMilliseconds(100));
                        var email = await _queueService.DequeueAsync(cts.Token);
                        await _emailSender.SendEmailAsync(new EmailMessage(email.To, email.Subject, email.Body, email.From), stoppingToken);
                        processedAny = true;
                    }
                    catch (OperationCanceledException) { break; }
                }
                if (processedAny) _logger.LogInformation("Finished processing email batch");
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Error processing email queue");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }
}