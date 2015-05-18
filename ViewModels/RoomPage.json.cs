using Starcounter;
using System;

namespace Chatter {
    partial class RoomPage : Page, IBound<Room> {
        public void RefreshStatements() {
            Statements = Db.SQL<Statement>(@"
					SELECT m FROM Statement m WHERE m.room = ? 
					ORDER BY tick DESC LIMIT ?", this.Data, 10);

            //Reverse the array
            for (int i = 0; i < Statements.Count / 2; i++) {
                var tmp = Statements[i];
                Statements[i] = Statements[Statements.Count - i - 1];
                Statements[Statements.Count - i - 1] = tmp;
            }
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
            Warning = "";

            Db.Transact(() => {
                var m = new Statement() {
                    user = Nick,
                    room = this.Data,
                    msg = Statement,
                    tick = DateTime.Now.Ticks
                };
            });
            RefreshStatements();
        }
    }
}
