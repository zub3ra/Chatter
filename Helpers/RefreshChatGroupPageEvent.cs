using Simplified.Ring6;
using Starcounter.Advanced;

namespace Chatter.Helpers
{
    public class RefreshChatGroupPageEvent
    {
        public ChatMessage ChatMessage { get; private set; }
        public ITransaction Transaction { get; private set; }

        public RefreshChatGroupPageEvent(ChatMessage chatMessage, ITransaction transaction)
        {
            ChatMessage = chatMessage;
            Transaction = transaction;
        }
    }
}
