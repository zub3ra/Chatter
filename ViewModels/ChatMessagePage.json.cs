using Starcounter;
using Simplified.Ring6;

namespace Chatter {
    partial class ChatMessagePage : Page, IBound<ChatMessage> {
        public void RefreshData(string ChatMessageId) {
            ChatMessage message = DbHelper.FromID(DbHelper.Base64DecodeObjectID(ChatMessageId)) as ChatMessage;

            this.Data = message;
            this.ChatAttachmentPages.Clear();

            var attachments = Db.SQL<ChatAttachment>("SELECT a FROM Simplified.Ring6.ChatAttachment a WHERE a.Message = ?", message);

            foreach (var item in attachments) {
                this.ChatAttachmentPages.Add(Self.GET("/chatter/partials/chatattachment/" + item.Key));
            }
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
