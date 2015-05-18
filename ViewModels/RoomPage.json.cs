using Starcounter;
using System;

namespace Chatter {
    partial class RoomPage : Page, IBound<Room> {
        public void RefreshStatements() {
            Statements = Db.SQL<Statement>(@"
					SELECT m FROM Statement m WHERE m.room = ? 
					ORDER BY tick DESC LIMIT ?", this.Data, 10);
        }

        void Handle(Input.Send action) {
            Db.Transact(() => {
                var m = new Statement() {
                    user = Nick == "" ? "Anonymous" : Nick,
                    room = this.Data,
                    msg = Statement,
                    tick = DateTime.Now.Ticks
                };
            });
            RefreshStatements();
        }
    }
}
