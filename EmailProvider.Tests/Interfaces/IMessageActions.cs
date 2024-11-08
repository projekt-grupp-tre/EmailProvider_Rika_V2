

namespace EmailProvider.Tests.Interfaces;

public interface IMessageActions
{
    Task CompleteMessageAsync(IMessage message);
}
