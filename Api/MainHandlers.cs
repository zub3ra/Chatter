using Starcounter;
using PolyjuiceNamespace;

namespace Chatter {
    internal class MainHandlers {
        public void Register() {
            Handle.GET("/chatter/standalone", () => {
                Session session = Session.Current;

                if (session != null && session.Data != null) {
                    return session.Data;
                }

                var standalone = new StandalonePage();

                if (session == null) {
                    session = new Session(SessionOptions.PatchVersioning);
                    standalone.Html = "/People/viewmodels/StandalonePage.html";
                }

                standalone.Session = session;
                return standalone;
            });

            Handle.GET("/chatter", () => {
                return Self.GET("/chatter/chatgroups");
            });

            Handle.GET("/chatter/chatgroups", () => {
                return Db.Scope<StandalonePage>(() => {
                    var master = (StandalonePage)Self.GET("/chatter/standalone");

                    master.CurrentPage = Self.GET("/chatter/partials/chatgroups");

                    return master;
                });
            });

            Handle.GET("/chatter/chatgroup/{?}", (string name) => {
                return Db.Scope<StandalonePage>(() => {
                    var master = (StandalonePage)Self.GET("/chatter/standalone");

                    master.CurrentPage = Self.GET("/chatter/partials/chatgroups/" + name);

                    return master;
                });
            });

            Handle.GET("/chatter/systemuser/{?}", (string SystemUserId) => {
                return Db.Scope<StandalonePage>(() => {
                    var master = (StandalonePage)Self.GET("/chatter/standalone");

                    master.CurrentPage = Self.GET("/chatter/partials/systemuser/" + SystemUserId);

                    return master;
                });
            });

            this.RegisterLauncher();
            this.RegisterPartials();
            this.RegisterMap();
        }

        protected void RegisterLauncher() {
            Handle.GET("/chatter/app-name", () => {
                return new AppName();
            });

            Handle.GET("/chatter/app-icon", () => {
                Page p = new Page() {
                    Html = "/Chatter/ViewModels/AppIconPage.html"
                };
                return p;
            });

            Handle.GET("/chatter/menu", () => {
                Page p = new Page() {
                    Html = "/Chatter/ViewModels/MenuPage.html"
                };
                return p;
            });
        }

        protected void RegisterPartials() {
            Handle.GET("/chatter/partials/chatgroups/{?}", (string Name) => {
                var page = new ChatGroupPage() {
                    Html = "/Chatter/ViewModels/ChatGroupPage.html"
                };

                page.RefreshData(Name);

                return page;
            });

            Handle.GET("/chatter/partials/chatmessages/{?}", (string ObjectId) => {
                var page = new ChatMessagePage();

                page.RefreshData(ObjectId);

                return page;
            });

            Handle.GET("/chatter/partials/chatgroups", () => {
                var page = new LobbyPage() {
                    Html = "/Chatter/ViewModels/LobbyPage.html"
                };

                page.RefreshData();

                return page;
            });

            Handle.GET("/chatter/partials/person/{?}", (string PersonId) => {
                PersonPage page = new PersonPage();

                page.RefreshData(PersonId);

                return page;
            });

            Handle.GET("/chatter/partials/systemuser/{?}", (string SystemUserId) => {
                SystemUserPage page = new SystemUserPage();

                page.RefreshData(SystemUserId);

                return page;
            });
        }

        protected void RegisterMap() {
            Polyjuice.Map("/chatter/app-name", "/polyjuice/app-name");
            Polyjuice.Map("/chatter/app-icon", "/polyjuice/app-icon");
            Polyjuice.Map("/chatter/menu", "/polyjuice/menu");

            Polyjuice.OntologyMap("/chatter/partials/person/@w", "/so/person/@w", null, null);
        }
    }
}
