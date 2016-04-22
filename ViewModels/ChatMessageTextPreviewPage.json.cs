using System.Linq;
using Simplified.Ring1;
using Simplified.Ring6;
using Starcounter;

namespace Chatter {
    partial class ChatMessageTextPreviewPage : Page, IBound<ChatMessageText>
    {
        public void RefreshData(string chatMessageDraftId)
        {
            var message = (Something) DbHelper.FromID(DbHelper.Base64DecodeObjectID(chatMessageDraftId));
            var messageText = Db.SQL<ChatMessageText>(@"Select m from Simplified.Ring6.ChatMessageText m Where m.ToWhat = ?", message);
            if (messageText.Any())
            {
                Data = messageText.First;
            }
        }
    }
}
