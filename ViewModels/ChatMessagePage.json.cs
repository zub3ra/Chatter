using System;
using Chatter.Helpers;
using Simplified.Ring1;
using Starcounter;
using Simplified.Ring6;

namespace Chatter {
    partial class ChatMessagePage : Json, IBound<ChatMessage>
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
            var relationId = Relation.GetObjectID();
            var warningPage = Self.GET<Json>("/chatter/partials/chatwarnings/" + relationId);
            var error = Db.SQL<ChatWarning>(@"Select m from Simplified.Ring6.ChatWarning m Where m.ErrorRelation = ?", Relation).First;
            if (error == null)
            {
                Warning = null;
                Data.IsDraft = false;
                Data.Date = DateTime.Now;

                var relations = Db.SQL<Relation>(@"SELECT m FROM Simplified.Ring1.Relation m WHERE m.ToWhat = ?", Data);
                foreach (Relation relation in relations)
                {
                    if (relation.GetObjectID() != relationId)
                    {
                        relation.WhatIs?.Delete();
                        relation.Delete();
                    }
                }
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
            Draft = Self.GET("/chatter/partials/chatattachments/" + relation.GetObjectID());
        }
    }
}
