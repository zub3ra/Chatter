using Chatter.Helpers;
using Starcounter;
using Simplified.Ring5;

namespace Chatter {
    internal class CommitHooks {
        public void Register() {
            Hook<SystemUserSession>.CommitInsert += (s, a) => {
                PageManager.RefreshSignInState();
            };

            Hook<SystemUserSession>.CommitDelete += (s, a) => {
                PageManager.RefreshSignInState();
            };

            Hook<SystemUserSession>.CommitUpdate += (s, a) => {
                PageManager.RefreshSignInState();
            };
        }
    }
}
