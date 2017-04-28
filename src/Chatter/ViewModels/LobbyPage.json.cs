using Simplified.Ring1;
using Starcounter;
using Simplified.Ring6;

namespace Chatter
{
    partial class LobbyPage : Json
    {
        public void RefreshData()
        {
            ChatGroups = Db.SQL<ChatGroup>("SELECT g FROM Simplified.Ring6.ChatGroup g ORDER BY g.Name");
        }

        void Handle(Input.GoToNewGroup Action)
        {
            var name = string.IsNullOrEmpty(this.NewGroupName) ? "Anonymous group" : this.NewGroupName;
            ChatGroup group = null;

            Db.Transact(() =>
            {
                group = new ChatGroup
                {
                    Name = name
                };
            });

            RedirectUrl = "/chatter/chatgroup/" + group.Key;
        }

        [LobbyPage_json.ChatGroups]
        partial class LobbyPageChatGroupRow : Json, IBound<ChatGroup>
        {
            void Handle(Input.Delete Action)
            {
                Db.Transact(() =>
                {
                    var messages = Db.SQL<ChatMessage>("SELECT m FROM Simplified.Ring6.ChatMessage m WHERE m.\"Group\" = ?", Data);

                    foreach (ChatMessage message in messages)
                    {
                        var relations = Db.SQL<Relation>("SELECT m FROM Simplified.Ring1.Relation m WHERE m.ToWhat = ?", message);
                        foreach (Relation relation in relations)
                        {
                            relation.WhatIs?.Delete();
                            relation.Delete();
                        }
                        message.Delete();
                    }

                    Data.Delete();
                });

                ParentPage.RefreshData();
            }

            public LobbyPage ParentPage
            {
                get
                {
                    return Parent.Parent as LobbyPage;
                }
            }

            protected override void OnData()
            {
                base.OnData();
                Url = $"/chatter/chatgroup/{Key}";
            }
        }
    }
}