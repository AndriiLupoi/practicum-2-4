using Mediator;
using Nimble.Modulith.Email.Contracts;
using Nimble.Modulith.Email.Interfaces;

namespace Nimble.Modulith.Email.Integrations;

public class SendEmailCommandHandler : ICommandHandler<SendEmailCommand>
{
    private readonly IQueueService<EmailToSend> _queueService;

    public SendEmailCommandHandler(IQueueService<EmailToSend> queueService)
      => _queueService = queueService;

    public async ValueTask<Unit> Handle(SendEmailCommand command, CancellationToken cancellationToken)
    {
        await _queueService.EnqueueAsync(new EmailToSend(command.To, command.Subject, command.Body, command.From), cancellationToken);
        return Unit.Value;
    }
}