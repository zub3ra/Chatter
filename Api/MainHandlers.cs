using Starcounter;
using Simplified.Ring6;
using System;
using Simplified.Ring1;

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
                    Html = "/Chatter/ViewModels/ChatGroupPage.html"
                };

                page.RefreshData(ChatGroupId);

                return page;
            });

            Handle.GET("/chatter/partials/chatmessages/{?}", (string ObjectId) => {
                var message = DbHelper.FromID(DbHelper.Base64DecodeObjectID(ObjectId)) as ChatMessage;
                var page = new ChatMessagePage
                {
                    Data = message
                };
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

            //For draft
            Handle.GET("/chatter/partials/chatmessagedraft/{?}", (string objectId) => {
                var page = new ChatMessagePage
                {
                    Html = "/Chatter/ViewModels/ChatMessageDraft.html"
                };
                var relation = (Relation)DbHelper.FromID(DbHelper.Base64DecodeObjectID(objectId));
                page.RefreshData(relation.ToWhat.GetObjectID());
                page.SetDraft(objectId);
                return page;
            });
            Handle.GET("/chatter/partials/chatattachment/{?}", (string objectPath) => null);

            //For TextPage similar in Images, People etc.
            Handle.GET("/chatter/partials/chatattachmenttext/{?}", (string chatMessageDraftId) =>
            {
                var message = (ChatMessage)DbHelper.FromID(DbHelper.Base64DecodeObjectID(chatMessageDraftId));
                if (message.IsDraft)
                {
                    var messageText = new ChatMessageText();
                    messageText.MessageText = messageText;
                    messageText.ChatMessage = message;

                    var draft = Self.GET("/chatter/partials/chatdraftannouncement/" + messageText.GetObjectID());
                    return draft;
                }
                else
                {
                    var page = new ChatMessageTextPreviewPage();
                    page.RefreshData(chatMessageDraftId);
                    return page;
                }
            });
            Handle.GET("/chatter/partials/chatmessagetext/{?}", (string chatmessageId) =>
            {
                var page = new ChatMessageTextPage();
                page.RefreshData(chatmessageId);
                return page;
            });
            Handle.GET("/chatter/partials/chatdraftannouncement/{?}", (string objectPath) => null);
        }

        protected void RegisterMap() {
            UriMapping.Map("/chatter/app-name", "/sc/mapping/app-name");
            UriMapping.Map("/chatter/menu", "/sc/mapping/menu");

            UriMapping.OntologyMap("/chatter/partials/person/@w", "simplified.ring2.person", null, null);
            UriMapping.OntologyMap("/chatter/partials/chatmessages/@w", "simplified.ring6.chatmessage", (string objectId) => objectId, (string objectId) =>
            {
                var message = (ChatMessage)DbHelper.FromID(DbHelper.Base64DecodeObjectID(objectId));
                return message.IsDraft ? null : objectId;
            });

            //For draft
            UriMapping.OntologyMap("/chatter/partials/chatmessagedraft/@w", "simplified.ring6.chatdraftannouncement", null, null);
            UriMapping.OntologyMap("/chatter/partials/chatattachment/@w", "simplified.ring6.chatattachment", objectId => objectId, objectId => null);

            //For TextPage similar in Images, People etc.
            UriMapping.OntologyMap("/chatter/partials/chatattachmenttext/@w", "simplified.ring6.chatmessage", null, null);
            UriMapping.OntologyMap("/chatter/partials/chatmessagetext/@w", "simplified.ring6.chatattachment", (string objectId) => objectId, (string objectId) =>
            {
                var sth = DbHelper.FromID(DbHelper.Base64DecodeObjectID(objectId));
                return sth.GetType() == typeof(ChatMessageText) ? sth.GetObjectID() : null;
            });
            UriMapping.OntologyMap("/chatter/partials/chatdraftannouncement/@w", "simplified.ring6.chatdraftannouncement", objectId => objectId, objectId => null);
        }
    }
}
