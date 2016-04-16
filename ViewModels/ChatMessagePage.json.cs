using Starcounter;
using Simplified.Ring6;

namespace Chatter {
    partial class ChatMessagePage : Page, IBound<ChatMessage> {
        public void RefreshData(string ChatMessageId) {
            var message = (ChatMessage) DbHelper.FromID(DbHelper.Base64DecodeObjectID(ChatMessageId));
            this.Attachment = Self.GET("/chatter/partials/chatattachment/" + message.Attachment.Key);
        }

        [ChatMessagePage_json.User]
        public partial class ChatMessageUserPage : Json {
            protected override void OnData() {
                base.OnData();
                this.Url = string.Format("/chatter/systemuser/{0}", this.Key);
            }
        }
    }
}
