using Simplified.Ring6;

namespace Chatter.Helpers
{
    public class ChatMessageTextValidator
    {
        public static string IsValid(ChatMessageText chatMessageText)
        {
            return string.IsNullOrEmpty(chatMessageText.Text) ? "Message cannot be empty!" : string.Empty;
        }
    }
}
