using System;
using Starcounter;
using Simplified.Ring6;
using Starcounter.Templates;

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
            (Parent as ChatGroupPage).AddNewMessage(Data);
        }

        public void SetDraft(string path)
        {
            Data.IsDraft = true;
            Draft = Self.GET("/chatter/partials/chatattachment/" + path);
        }
    }
}
