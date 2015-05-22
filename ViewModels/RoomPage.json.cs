using System;
using Starcounter;
using Starcounter.Internal;
using Simplified.Ring6;

namespace Chatter {

    partial class RoomPage : Page, IBound<ChatGroup> {

        private long maxMsgs = 10;

        public void RefreshStatements() {
            long count = Db.SlowSQL<long>(@"
					SELECT COUNT(*) FROM Simplified.Ring6.ChatMessage m 
                    WHERE m.""Group"" = ?", this.Data).First;

            long offset = count > maxMsgs ? count - maxMsgs : 0;

            ChatMessages = Db.SQL<ChatMessage>(@"
					SELECT m FROM Simplified.Ring6.ChatMessage m WHERE m.""Group"" = ? 
					ORDER BY m.""Date"" ASC FETCH ? OFFSET ?", this.Data, 10, offset);
        }

        void Handle(Input.Send action) {
            if (string.IsNullOrEmpty(UserName)) {
                Warning = "Nick cannot be empty";
                return;
            }
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
                    Date = DateTime.Now
                };
            });

            Warning = string.Empty;
            Text = string.Empty;

            Session.ForEach((Session s) => {
                MasterPage data = (MasterPage)s.Data;
                if (data.CurrentPage is RoomPage) {
                    RoomPage page = (RoomPage)data.CurrentPage;
                    if (page.Data.Equals(this.Data)) { //is current room?
                        if (page.ChatMessages.Count >= maxMsgs) {
                            page.ChatMessages.RemoveAt(0);
                        }
                        var item = page.ChatMessages.Add();
                        item.Data = m;
                        s.CalculatePatchAndPushOnWebSocket();
                    }
                }
            });
        }
    }
}
