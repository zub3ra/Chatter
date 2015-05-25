using Starcounter;
using Simplified.Ring6;

namespace Chatter {
    partial class LobbyPage : Page {
        public void RefreshData() {
            this.ChatGroups = Db.SQL<ChatGroup>("SELECT g FROM Simplified.Ring6.ChatGroup g ORDER BY g.Name");
        }

        void Handle(Input.GoToGroup Action) {
            RedirectUrl = "/chatter/chatgroup/" + Group;
        }
    }
}