using System;
using Chatter.Helpers;
using Simplified.Ring1;
using Starcounter;
using Simplified.Ring6;

namespace Chatter {
    partial class ChatMessagePage : Page, IBound<ChatMessage>
    {
        private Relation Relation { get; set; }

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
            var warningPage = Self.GET<Json>("/chatter/partials/chatwarning/" + Relation.GetObjectID());
            if (warningPage == null)
            {
                Warning = null;
                Data.IsDraft = false;
                Data.Date = DateTime.Now;
                Transaction.Commit();
                PageManager.Refresh(Data.Key);
            }
            else
            {
                Warning = warningPage;
            }
        }

        public void SetDraft(Relation relation)
        {
            Relation = relation;
            Data.IsDraft = true;
            Draft = Self.GET("/chatter/partials/chatattachment/" + relation.GetObjectID());
        }
    }
}
