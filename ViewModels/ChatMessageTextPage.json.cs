using Simplified.Ring6;
using Starcounter;

namespace Chatter {
    partial class ChatMessageTextPage : Page, IBound<ChatMessageText>
    {
        public void RefreshData(string chatMessageDraftId)
        {
            var messageText = DbHelper.FromID(DbHelper.Base64DecodeObjectID(chatMessageDraftId)) as ChatMessageText;
            Data = messageText;
        }
    }
}
