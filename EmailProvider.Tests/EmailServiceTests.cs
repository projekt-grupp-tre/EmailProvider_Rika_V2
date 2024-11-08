using Azure.Communication.Email;
using Azure.Messaging.ServiceBus;
using EmailProvider.Tests.Interfaces;
using EmailProvider_Rika_V2.Models;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;

namespace EmailProvider.Tests;

public class EmailServiceTests
{
    private readonly Mock<ILogger<EmailService>> _mockLogger;
    private readonly Mock<EmailClient> _mockEmailClient;
    private readonly EmailService _emailService;

    public EmailServiceTests()
    {
        _mockLogger = new Mock<ILogger<EmailService>>();
        _mockEmailClient = new Mock<EmailClient>();
        _emailService = new EmailService(_mockEmailClient.Object, _mockLogger.Object);
    }

    [Fact]
    public void UnpackEmailRequest_ValidMessage_ReturnsEmailRequest()
    {
        // Arrange
        var validJson = JsonConvert.SerializeObject(new EmailRequest
        {
            To = "test@example.com",
            Subject = "Test Subject",
            HtmlBody = "<p>Test</p>",
            PlainText = "Test"
        });

        var message = ServiceBusModelFactory.ServiceBusReceivedMessage(
            body: BinaryData.FromString(validJson)
        );

        // Act
        var result = _emailService.UnpackEmailRequest(message);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test@example.com", result.To);
    }

    [Fact]
    public void UnpackEmailRequest_InvalidMessage_ReturnsNull()
    {
        // Arrange
        var message = ServiceBusModelFactory.ServiceBusReceivedMessage(
            body: BinaryData.FromString("Invalid JSON")
        );

        // Act
        var result = _emailService.UnpackEmailRequest(message);

        // Assert
        Assert.Null(result);
    }
}