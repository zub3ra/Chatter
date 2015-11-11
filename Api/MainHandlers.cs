using Starcounter;
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

                page.Data = rel;

                if (obj != null) {
                    System.Type type = obj.GetType();

                    if (type == typeof(ChatGroup)) {
                        page.Html = "/Chatter/ViewModels/ChatAttachmentGroupPage.html";
                    } else {
                        page.Html += "?" + obj.GetType().FullName;
                    }
                }

                return page;
            });
        }

        protected void RegisterMap() {
            UriMapping.Map("/chatter/app-name", "/sc/mapping/app-name");
            UriMapping.Map("/chatter/menu", "/sc/mapping/menu");

            UriMapping.OntologyMap("/chatter/partials/person/@w", "simplified.ring2.person", null, null);
            UriMapping.OntologyMap("/chatter/partials/chatgroups/@w", "simplified.ring6.chatgroup", null, null);
            UriMapping.OntologyMap("/chatter/partials/chatattachment/@w", "simplified.ring6.chatattachment", null, null);
        }
    }
}
