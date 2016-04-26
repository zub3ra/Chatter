using Chatter.Helpers;
using Simplified.Ring6;
using Starcounter;

namespace Chatter {
    partial class ChatMessageTextWarningPage : Page
    {
        public void RefreshData(string chatMessageTextId)
        {
            var chatMessageText = DbHelper.FromID(DbHelper.Base64DecodeObjectID(chatMessageTextId)) as ChatMessageText;
            Warning = ChatMessageTextValidator.IsValid(chatMessageText);
        }
    }
}
