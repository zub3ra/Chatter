using Starcounter;
using Simplified.Ring1;
using Simplified.Ring6;
using System;

namespace Chatter {

    [Database]
    public class SavedSession {
        public string SessionId;
    }

    internal class MainHandlers {
        public void Register() {
            Db.Transact(() => {
                Db.SlowSQL("DELETE FROM SavedSession");
            });

            Handle.GET("/chatter/standalone", () => {
                Session session = Session.Current;

                if (session != null && session.Data != null) {
                    return session.Data;
                }

                var standalone = new StandalonePage();

                if (session == null) {
                    session = new Session(SessionOptions.PatchVersioning);
                    session.AddDestroyDelegate((Session s) => {
                        Db.Transact(() => {
                            String ss = s.ToAsciiString();
                            SavedSession saved = Db.SQL<SavedSession>("SELECT s FROM SavedSession s WHERE s.SessionId = ?", ss).First;
                            saved.Delete();
                        });
                    });

                    standalone.Html = "/Chatter/viewmodels/StandalonePage.html";

                    Db.Transact(() => {
                        new SavedSession() {
                            SessionId = session.ToAsciiString()
                        };
                    });
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
                    Html = "/Chatter/ViewModels/ChatGroupPage.html",
                    ChooseAttachment = Self.GET("/chatter/partials/chooseattachment/I")
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

            Handle.GET("/chatter/partials/chooseattachment", () =>
            {
                var page = new ChooseAttachmentPage();
                return page;
            });

            Handle.GET("/chatter/partials/chooseattachment/{?}", (string id) =>
            {
                var page = new ChooseAttachmentPage();
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
            UriMapping.OntologyMap("/chatter/partials/chooseattachment/@w", "simplified.ring6.chatgroup", null, null);
            UriMapping.OntologyMap("/chatter/partials/chatattachment/@w", "simplified.ring6.chatattachment", null, null);


            UriMapping.OntologyMap("/chatter/partials/chooseattachment/@w", "dupa", null, null);
        }
    }
}
