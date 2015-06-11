using Starcounter;
using Starcounter.Internal;
using PolyjuiceNamespace;
using Simplified.Ring5;

namespace Chatter {
    internal class CommitHooks {
        public void Register() {
            Hook<SystemUserSession>.OnInsert(s => {
                this.RefreshSignInState();
            });

            Hook<SystemUserSession>.OnDelete(s => {
                this.RefreshSignInState();
            });

            Hook<SystemUserSession>.OnUpdate(s => {
                this.RefreshSignInState();
            });
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
