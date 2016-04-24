using Starcounter;
using Starcounter.Internal;
using Simplified.Ring5;
using Simplified.Ring6;

namespace Chatter {
    internal class CommitHooks {
        public void Register() {
            Hook<SystemUserSession>.CommitInsert += (s, a) => {
                this.RefreshSignInState();
            };

            Hook<SystemUserSession>.CommitDelete += (s, a) => {
                this.RefreshSignInState();
            };

            Hook<SystemUserSession>.CommitUpdate += (s, a) => {
                this.RefreshSignInState();
            };

            Hook<ChatMessageText>.CommitInsert += (s, a) => {
                if (string.IsNullOrEmpty(a.Text))
                {
                    a.Delete();
                }
            };
        }

        protected void RefreshSignInState() {
            StandalonePage master = GetStandalonePage();

            if (master == null) {
                return;
            }

            ChatGroupPage page = master.CurrentPage as ChatGroupPage;

            if (page == null) {
                return;
            }

            page.RefreshUser();
        }

        protected StandalonePage GetStandalonePage() {
            StandalonePage page = null;

            if (Session.Current != null && Session.Current.Data is StandalonePage) {
                page = Session.Current.Data as StandalonePage;
            }

            return page;
        }
    }
}
