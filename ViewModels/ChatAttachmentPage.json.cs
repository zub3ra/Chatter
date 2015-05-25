using Starcounter;
using Simplified.Ring6;

namespace Chatter {
    partial class ChatAttachmentPage : Page, IBound<ChatAttachment> {
        public void RefreshData(string ChatAttachmentId) {
            ChatAttachment attachment = DbHelper.FromID(DbHelper.Base64DecodeObjectID(ChatAttachmentId)) as ChatAttachment;

            this.Data = attachment;
        }
    }
}
