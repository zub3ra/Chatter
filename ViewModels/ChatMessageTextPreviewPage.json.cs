using Simplified.Ring6;
using Starcounter;

namespace Chatter {
    partial class ChatMessageTextPreviewPage : Json, IBound<ChatMessageText>
    {
        public void RefreshData(string chatMessageDraftId)
        {
            var chatMessageText = DbHelper.FromID(DbHelper.Base64DecodeObjectID(chatMessageDraftId)) as ChatMessageText;
            Data = chatMessageText;
        }
    }
}
