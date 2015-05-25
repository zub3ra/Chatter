using System;
using Starcounter;
using PolyjuiceNamespace;
using Simplified.Ring6;

namespace Chatter {
    class Program {
        static void Main() {
            MainHandlers handlers = new MainHandlers();
            CommitHooks hooks = new CommitHooks();
            DefaultStyles styles = new DefaultStyles();

            handlers.Register();
            hooks.Register();
            styles.ApplyIfEmpty();
        }
    }
}
