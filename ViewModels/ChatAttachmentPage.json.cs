using Starcounter;
using Simplified.Ring6;

namespace Chatter {
    partial class ChatAttachmentPage : Page, IBound<ChatAttachment> {
        [ChatAttachmentPage_json.Attachment]
        public partial class ChatAttachmentAttachmentPage : Json {
            protected override void OnData() {
                base.OnData();
                this.Url = string.Format("/chatter/chatgroup/{0}", this.Key);
            }
        }
    }
}
