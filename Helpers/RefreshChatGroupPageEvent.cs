namespace Chatter.Helpers
{
    public class RefreshChatGroupPageEvent
    {
        public string ChatMessageKey { get; private set; }

        public RefreshChatGroupPageEvent(string chatMessageKey)
        {
            ChatMessageKey = chatMessageKey;
        }
    }
}
