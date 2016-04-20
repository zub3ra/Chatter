using Simplified.Ring1;
using Simplified.Ring6;
using Starcounter;

namespace Chatter {
    partial class ChatMessageTextPage : Page, IBound<ChatMessageText>
    {
        public void RefreshData(string chatMessageDraftId)
        {
            var message = (Something)DbHelper.FromID(DbHelper.Base64DecodeObjectID(chatMessageDraftId));
            var messageText = new ChatMessageText();
            messageText.MessageText = messageText;
            messageText.ChatMessage = message;
            Data = messageText;
        }
    }
}
