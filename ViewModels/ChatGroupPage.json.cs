using System;
using System.Linq;
using Starcounter;
using Starcounter.Internal;
using Simplified.Ring3;
using Simplified.Ring5;
using Simplified.Ring6;

namespace Chatter {

    partial class ChatGroupPage : Page, IBound<ChatGroup> {

        private long maxMsgs = 10;

        public void RefreshData(string GroupName) {
            var group = Db.SQL<ChatGroup>("SELECT r FROM Simplified.Ring6.ChatGroup r WHERE r.name = ?", GroupName).First;
            
            if (group == null) {
                Db.Transact(() => {
                    group = new ChatGroup() { Name = GroupName };
                });
            };

            this.RefreshUser();
            this.Data = group;
            this.RefreshChatMessages();
        }

        public void RefreshUser() {
            SystemUserSession session = this.GetCurrentSystemUserSession();

            if (session != null) {
                this.SystemUser.Data = session.Token.User;
                this.UserName = session.Token.User.Username;
            } else {
                this.SystemUser.Data = null;
                this.UserName = "Anonymous";
            }
        }

        public void RefreshChatMessages() {
            long count = Db.SlowSQL<long>(@"
					SELECT COUNT(*) FROM Simplified.Ring6.ChatMessage m 
                    WHERE m.""Group"" = ?", this.Data).First;

            long offset = count > maxMsgs ? count - maxMsgs : 0;
            var messages = Db.SQL<ChatMessage>(@"
					SELECT m FROM Simplified.Ring6.ChatMessage m WHERE m.""Group"" = ? 
					ORDER BY m.""Date"" ASC FETCH ? OFFSET ?", this.Data, 10, offset);

            this.ChatMessages.Clear();

            foreach (ChatMessage item in messages) {
                var page = Self.GET<Json>("/chatter/partials/chatmessages/" + item.Key);
                this.ChatMessages.Add(page);
            }
        }

        void Handle(Input.Send Action) {
            if (string.IsNullOrEmpty(Text)) {
                Warning = "Message cannot be empty";
                return;
            }

            ChatMessage m = null;
            Db.Transact(() => {
                m = new ChatMessage() {
                    UserName = UserName,
                    Group = this.Data,
                    Text = Text,
                    Date = DateTime.Now,
                    User = this.SystemUser.Data
                };
            });

            Warning = string.Empty;
            Text = string.Empty;

            Session.ForEach((Session s) => {
                StandalonePage master = s.Data as StandalonePage;

                if (master != null && master.CurrentPage is ChatGroupPage) {
                    ChatGroupPage page = (ChatGroupPage)master.CurrentPage;
                    
                    if (page.Data.Equals(this.Data)) {
                        if (page.ChatMessages.Count >= maxMsgs) {
                            page.ChatMessages.RemoveAt(0);
                        }

                        page.ChatMessages.Add(Self.GET<Json>("/chatter/partials/chatmessages/" + m.Key));
                        s.CalculatePatchAndPushOnWebSocket();
                    }
                }
            });
        }

        protected SystemUserSession GetCurrentSystemUserSession() {
            return Db.SQL<SystemUserSession>("SELECT o FROM Simplified.Ring5.SystemUserSession o WHERE o.SessionIdString = ?", Session.Current.SessionIdString).First;
        }

        [ChatGroupPage_json.SystemUser]
        public partial class ChatGroupPageSystemUser : Json, IBound<SystemUser> { 
        }
    }
}
