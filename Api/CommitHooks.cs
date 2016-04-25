using Chatter.Helpers;
using Starcounter;
using Simplified.Ring5;
using Simplified.Ring6;

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

            Hook<ChatMessageText>.CommitInsert += (s, a) => {
                if (string.IsNullOrEmpty(a.Text))
                {
                    a.Delete();
                }
            };
        }
    }
}
