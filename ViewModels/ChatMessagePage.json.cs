using System;
using Starcounter;
using Simplified.Ring6;

namespace Chatter {
    partial class ChatMessagePage : Page, IBound<ChatMessage>
    {
        public void RefreshData(string ChatMessageId)
        {
            var message = (ChatMessage) DbHelper.FromID(DbHelper.Base64DecodeObjectID(ChatMessageId));
            Data = message;
        }

        [ChatMessagePage_json.User]
        public partial class ChatMessageUserPage : Json
        {
            protected override void OnData()
            {
                base.OnData();
                Url = string.Format("/chatter/systemuser/{0}", this.Key);
            }
        }

        void Handle(Input.Send Action)
        {
            Data.IsDraft = false;
            Data.Date = DateTime.Now;
            Transaction.Commit();

            //Send message to ChatGroupPage

            StandalonePage master = GetStandalonePage();

            if (master == null)
            {
                return;
            }

            ChatGroupPage page = master.CurrentPage as ChatGroupPage;

            if (page == null)
            {
                return;
            }

            page.Refresh(Data.Key);
        }

        protected StandalonePage GetStandalonePage()
        {
            StandalonePage page = null;

            if (Session.Current != null && Session.Current.Data is StandalonePage)
            {
                page = Session.Current.Data as StandalonePage;
            }

            return page;
        }

        public void SetDraft(string path)
        {
            Data.IsDraft = true;
            Draft = Self.GET("/chatter/partials/chatattachment/" + path);
        }
    }
}
