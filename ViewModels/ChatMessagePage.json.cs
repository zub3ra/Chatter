using System;
using Chatter.Helpers;
using Starcounter;
using Simplified.Ring6;

namespace Chatter {
    partial class ChatMessagePage : Page, IBound<ChatMessage>
    {
        public void RefreshData(string chatMessageId)
        {
            var message = (ChatMessage) DbHelper.FromID(DbHelper.Base64DecodeObjectID(chatMessageId));
            Data = message;
        }

        [ChatMessagePage_json.User]
        public partial class ChatMessageUserPage : Json
        {
            protected override void OnData()
            {
                base.OnData();
                Url = $"/chatter/systemuser/{Key}";
            }
        }

        void Handle(Input.Send Action)
        {
            Data.IsDraft = false;
            Data.Date = DateTime.Now;
            Transaction.Commit();
            PageManager.Refresh(Data.Key);
        }

        public void SetDraft(string objectId)
        {
            Data.IsDraft = true;
            Draft = Self.GET("/chatter/partials/chatattachment/" + objectId);
        }
    }
}
