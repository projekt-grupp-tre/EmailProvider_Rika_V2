

using Azure.Messaging.ServiceBus;
using EmailProvider_Rika_V2.Models;

namespace EmailProvider_Rika_V2.Services;

public interface IEmailService
{
    EmailRequest UnpackEmailRequest(ServiceBusReceivedMessage message);

    bool SendEmail(EmailRequest emailRequest);
}
