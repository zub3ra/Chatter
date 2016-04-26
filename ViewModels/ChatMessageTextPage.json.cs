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

        public bool IsValid()
        {
            if (string.IsNullOrEmpty(Data.Text))
            {
                Warning = "Message cannot be empty";
                return false;
            }
            else
            {
                Warning = string.Empty;
                return true;
            }
        }
    }
}
