using Starcounter;
using Starcounter.Internal;
using System;

namespace Chatter {

    partial class RoomPage : Page, IBound<Room> {

        private Int64 maxMsgs = 10;

        public void RefreshStatements() {
            var count = Db.SlowSQL<Int64>(@"
					SELECT COUNT(*) FROM Statement m 
                    WHERE m.room = ?", this.Data).First;

            var offset = count > maxMsgs ? count - maxMsgs : 0;

            Statements = Db.SQL<Statement>(@"
					SELECT m FROM Statement m WHERE m.room = ? 
					ORDER BY tick ASC FETCH ? OFFSET ?", this.Data, 10, offset);
        }

        void Handle(Input.Send action) {
            if (Nick == "") {
                Warning = "Nick cannot be empty";
                return;
            }
            if (Statement == "") {
                Warning = "Message cannot be empty";
                return;
            }

            Statement m = null;
            Db.Transact(() => {
                m = new Statement() {
                    user = Nick,
                    room = this.Data,
                    msg = Statement,
                    tick = DateTime.Now.Ticks
                };
            });

            Warning = "";
            Statement = "";

            Session.ForEach((Session s) => {
                StarcounterEnvironment.AppName = "Chatter";
                if (s.Data is MasterPage) {
                    MasterPage data = (MasterPage)s.Data;
                    if (data.CurrentPage is RoomPage) {
                        RoomPage page = (RoomPage)data.CurrentPage;
                        if (page.Data.Equals(this.Data)) { //is current room?
                            if (page.Statements.Count >= maxMsgs) {
                                page.Statements.RemoveAt(0);
                            }
                            var item = page.Statements.Add();
                            item.Data = m;
                            s.CalculatePatchAndPushOnWebSocket();
                        }
                    }
                }
            });
        }
    }
}
