using Azure.Messaging.ServiceBus;
using EmailProvider_Rika_V2.Functions;
using EmailProvider_Rika_V2.Models;
using EmailProvider_Rika_V2.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text;
using System.Text.Json;
using Xunit;

namespace EmailProvider.Tests;

public class EmailSenderTests
{
    private readonly Mock<IEmailService> _mockEmailService;
    private readonly Mock<ILogger<EmailSender>> _mockLogger;
    private readonly EmailSender _emailSender;

    public EmailSenderTests()
    {
        _mockEmailService = new Mock<IEmailService>();
        _mockLogger = new Mock<ILogger<EmailSender>>();
        _emailSender = new EmailSender(_mockLogger.Object, _mockEmailService.Object);
    }

    [Fact]
    public async Task Run_ValidEmailRequest_CompletesMessage()
    {
        // Arrange
        var emailRequest = new EmailRequest
        {
            To = "test@example.com",
            Subject = "Test Subject",
            HtmlBody = "<p>Test</p>",
            PlainText = "Test"
        };

        var messageBody = JsonSerializer.Serialize(emailRequest);
        var serviceBusMessage = CreateServiceBusReceivedMessage(messageBody);

        var testMessageActions = new TestServiceBusMessageActions();

        _mockEmailService.Setup(service => service.UnpackEmailRequest(serviceBusMessage)).Returns(emailRequest);
        _mockEmailService.Setup(service => service.SendEmail(emailRequest)).Returns(true);

        // Act
        await _emailSender.Run(serviceBusMessage, testMessageActions);

        // Assert
        Assert.True(testMessageActions.IsCompleteMessageCalled);
    }

    [Fact]
    public async Task Run_InvalidEmailRequest_DoesNotCompleteMessage()
    {
        // Arrange
        var serviceBusMessage = CreateServiceBusReceivedMessage("Invalid JSON");
        var testMessageActions = new TestServiceBusMessageActions();

        _mockEmailService.Setup(service => service
            .UnpackEmailRequest(serviceBusMessage)).Returns((EmailRequest)null);

        // Act
        await _emailSender.Run(serviceBusMessage, testMessageActions);

        // Assert
        Assert.False(testMessageActions.IsCompleteMessageCalled);
    }

    private ServiceBusReceivedMessage CreateServiceBusReceivedMessage(string body)
    {
        var message = new ServiceBusMessage(Encoding.UTF8.GetBytes(body));
        return ServiceBusModelFactory.ServiceBusReceivedMessage(
            messageId: Guid.NewGuid().ToString(),
            body: message.Body,
            contentType: "application/json"
        );
    }

    
    private class TestServiceBusMessageActions : ServiceBusMessageActions
    {
        public bool IsCompleteMessageCalled { get; private set; } = false;

        public override Task CompleteMessageAsync(ServiceBusReceivedMessage message, CancellationToken cancellationToken = default)
        {
            IsCompleteMessageCalled = true;
            return Task.CompletedTask;
        }

        
    }
}