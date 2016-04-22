using System;
using Chatter.Helpers;
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

            //Send message to ChatGroupPage
            EventBus.Instance.PostEvent(new RefreshChatGroupPageEvent(Data, Transaction));
        }

        public void SetDraft(string path)
        {
            Data.IsDraft = true;
            Draft = Self.GET("/chatter/partials/chatattachment/" + path);
        }
    }
}
