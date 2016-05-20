using Chatter.Helpers;
using Simplified.Ring6;
using Starcounter;

namespace Chatter {
    partial class ChatMessageTextWarningPage : Page
    {
        public void RefreshData(TextRelation textRelation)
        {
            Warning = ChatMessageTextValidator.IsValid(textRelation.Content);
            var relation = Db.SQL<ChatWarning>(@"Select m from Simplified.Ring6.ChatWarning m Where m.ErrorRelation = ?", textRelation).First;
            if (!string.IsNullOrEmpty(Warning))
            {
                if (relation == null)
                {
                    new ChatWarning
                    {
                        ErrorRelation = textRelation
                    };
                }
            }
            else
            {
                relation?.Delete();
            }
        }
    }
}
