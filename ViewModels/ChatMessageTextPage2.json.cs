using Simplified.Ring1;
using Simplified.Ring6;
using Starcounter;

namespace Chatter {
    partial class ChatMessageTextPage2 : Page, IBound<ChatMessageText2>
    {
        public void RefreshData(string chatMessageDraftId)
        {
            var message = (Something)DbHelper.FromID(DbHelper.Base64DecodeObjectID(chatMessageDraftId));
            var messageText = new ChatMessageText2();
            messageText.MessageText = messageText;
            messageText.ChatMessage = message;
            Data = messageText;
        }
    }
}
