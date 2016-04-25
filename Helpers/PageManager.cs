using Starcounter;

namespace Chatter.Helpers
{
    public class PageManager
    {
        private static ChatGroupPage GetGroupPage()
        {
            var master = GetStandalonePage();
            return master?.CurrentPage as ChatGroupPage;
        }

        private static StandalonePage GetStandalonePage()
        {
            StandalonePage page = null;

            if (Session.Current != null && Session.Current.Data is StandalonePage)
            {
                page = Session.Current.Data as StandalonePage;
            }

            return page;
        }

        public static void RefreshSignInState()
        {
            GetGroupPage()?.RefreshUser();
        }

        public static void Refresh(string key)
        {
            GetGroupPage()?.Refresh(key);
        }
    }
}
