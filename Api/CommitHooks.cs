using Starcounter;
using Starcounter.Internal;
using PolyjuiceNamespace;

namespace Chatter {
    internal class CommitHooks {
        public static string LocalAppUrl = "/Chatter/__db/__" + StarcounterEnvironment.DatabaseNameLower + "/societyobjects/systemusersession";
        public static string MappedTo = "/polyjuice/signin";

        public void Register() {
            Handle.POST(LocalAppUrl, () => {
                StandalonePage master = GetStandalonePage();

                if (master == null) {
                    return (ushort)System.Net.HttpStatusCode.OK;
                }

                ChatGroupPage page = master.CurrentPage as ChatGroupPage;

                if (page == null) {
                    return (ushort)System.Net.HttpStatusCode.OK;
                }

                page.RefreshUser();

                return (ushort)System.Net.HttpStatusCode.OK;
            });

            // User signed out event
            Handle.DELETE(LocalAppUrl, () => {
                StandalonePage master = GetStandalonePage();

                if (master == null) {
                    return (ushort)System.Net.HttpStatusCode.OK;
                }

                ChatGroupPage page = master.CurrentPage as ChatGroupPage;

                if (page == null) {
                    return (ushort)System.Net.HttpStatusCode.OK;
                }

                page.RefreshUser();

                return (ushort)System.Net.HttpStatusCode.OK;
            });

            Polyjuice.Map(LocalAppUrl, MappedTo, "POST");
            Polyjuice.Map(LocalAppUrl, MappedTo, "DELETE");
        }

        protected StandalonePage GetStandalonePage() {
            if (Session.Current != null && Session.Current.Data is StandalonePage) {
                return Session.Current.Data as StandalonePage;
            }

            return null;
        }
    }
}
