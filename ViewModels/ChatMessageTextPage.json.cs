using Simplified.Ring6;
using Starcounter;

namespace Chatter {
    partial class ChatMessageTextPage : Json, IBound<ChatMessageText>
    {
        public void AddNew(ChatMessageTextRelation textRelation)
        {
            var chatMessageText = new ChatMessageText();
            textRelation.Content = chatMessageText;
            Data = chatMessageText;
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
