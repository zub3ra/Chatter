using Starcounter;
using PolyjuiceNamespace;
using Simplified.Ring1;
using Simplified.Ring6;

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
                    standalone.Html = "/Chatter/viewmodels/StandalonePage.html";
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
            Handle.GET("/chatter/partials/chatgroups/{?}", (string ChatGroupId) => {
                var page = new ChatGroupPage() {
                    Html = "/Chatter/ViewModels/ChatGroupPage.html"
                };

                page.RefreshData(ChatGroupId);

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

            Handle.GET("/chatter/partials/chatattachment/{?}", (string ChatAttachmentId) => {
                ChatAttachmentPage page = new ChatAttachmentPage();
                ChatAttachment rel = DbHelper.FromID(DbHelper.Base64DecodeObjectID(ChatAttachmentId)) as ChatAttachment;
                Something obj = rel.Attachment;

                page.RefreshData(ChatAttachmentId);

                if (obj != null) {
                    page.Html += "?" + obj.GetType().FullName;
                }

                return page;
            });
        }

        protected void RegisterMap() {
            Polyjuice.Map("/chatter/app-name", "/polyjuice/app-name");
            Polyjuice.Map("/chatter/app-icon", "/polyjuice/app-icon");
            Polyjuice.Map("/chatter/menu", "/polyjuice/menu");

            Polyjuice.OntologyMap("/chatter/partials/person/@w", "/so/person/@w", null, null);
            Polyjuice.OntologyMap("/chatter/partials/chatgroups/@w", "/so/group/@w", null, null);
            Polyjuice.OntologyMap("/chatter/partials/chatattachment/@w", "/db/simplified.ring6.chatattachment/@w", null, null);
        }
    }
}
