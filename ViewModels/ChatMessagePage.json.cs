using Starcounter;
using Simplified.Ring6;

namespace Chatter {
    partial class ChatMessagePage : Page, IBound<ChatMessage> {
        public void RefreshData(string ChatMessageId) {
            ChatMessage message = DbHelper.FromID(DbHelper.Base64DecodeObjectID(ChatMessageId)) as ChatMessage;

            this.Data = message;
        }
    }
}
