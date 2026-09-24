using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using WebUI.Areas.Admin.Controllers;
using WebUI.ExternalServices;
using WebUI.Models.Mail;

namespace WebUI.IntegrationTests.Unit.Mail;

public sealed class MailControllerTests
{
    [Fact]
    public async Task Index_UsesInboxWhenFolderIsNotProvided()
    {
        var inbox = new MailFolderDto
        {
            Key = "inbox",
            DisplayName = "Gelen Kutusu",
            Name = "INBOX",
            Icon = "fa-inbox"
        };
        var folders = new[] { inbox };
        var messages = new MailMessageListResult
        {
            Messages =
            [
                new MailMessageSummaryDto
                {
                    FolderKey = inbox.Key,
                    Uid = 7,
                    SenderAddress = "sender@example.com",
                    Subject = "Konu"
                }
            ],
            TotalCount = 1,
            Page = 1,
            PageSize = 50
        };
        var service = new Mock<IMailboxService>();
        service.Setup(item => item.GetFoldersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(folders);
        service.Setup(item => item.ListMessagesAsync(inbox.Key, null, 1, 50, It.IsAny<CancellationToken>()))
            .ReturnsAsync(messages);

        var controller = new MailController(service.Object, NullLogger<MailController>.Instance);
        var result = await controller.Index(null, null, 1, null, CancellationToken.None);

        var view = result.Should().BeOfType<Microsoft.AspNetCore.Mvc.ViewResult>().Subject;
        var model = view.Model.Should().BeOfType<MailPageViewModel>().Subject;
        model.CurrentFolderKey.Should().Be("inbox");
        model.Messages.Should().ContainSingle(item => item.Uid == 7);
    }

    [Fact]
    public void Controller_IsRestrictedToAdminRole()
    {
        var authorize = typeof(MailController)
            .GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), inherit: true)
            .OfType<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>()
            .Single();

        authorize.Roles.Should().Be("Admin");
    }
}
