using Starcounter;
using Simplified.Ring6;

namespace Chatter {
    partial class LobbyPage : Page {
        public void RefreshData() {
            this.ChatGroups = Db.SQL<ChatGroup>("SELECT g FROM Simplified.Ring6.ChatGroup g ORDER BY g.Name");
        }

        void Handle(Input.GoToNewGroup Action) {
            string name = string.IsNullOrEmpty(this.NewGroupName) ? "Anonymous group" : this.NewGroupName;
            ChatGroup group = null;

            Db.Transact(() => {
                group = new ChatGroup() {
                    Name = name
                };
            });

            RedirectUrl = "/chatter/chatgroup/" + group.Key;
        }

        [LobbyPage_json.ChatGroups]
        partial class LobbyPageChatGroupRow : Json, IBound<ChatGroup> {
            void Handle(Input.Delete Action) {
                Db.Transact(() => {
                    var messages = Db.SQL<ChatMessage>("SELECT m FROM Simplified.Ring6.ChatMessage m WHERE m.\"Group\" = ?", this.Data);

                    foreach (ChatMessage message in messages) {
                        var attachments = Db.SQL<ChatAttachment>("SELECT a FROM Simplified.Ring6.ChatAttachment a WHERE a.Message = ?", message);

                        foreach (ChatAttachment attachment in attachments) {
                            attachment.Delete();
                        }

                        message.Delete();
                    }

                    this.Data.Delete();
                });

                this.ParentPage.RefreshData();
            }

            public LobbyPage ParentPage {
                get {
                    return this.Parent.Parent as LobbyPage;
                }
            }

            protected override void OnData() {
                base.OnData();
                this.Url = string.Format("/chatter/chatgroup/{0}", this.Key);
            }
        }
    }
}