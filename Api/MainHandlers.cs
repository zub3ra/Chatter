using Chatter.Helpers;
using Starcounter;
using Simplified.Ring6;
using Simplified.Ring1;
using Starcounter.Advanced;

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
                var session = Session.Current;

                if (session?.Data != null) {
                    return session.Data;
                }

                var standalone = new StandalonePage();

                if (session == null) {
                    session = new Session(SessionOptions.PatchVersioning);
                    session.AddDestroyDelegate(s => {
                        Db.Transact(() => {
                            var ss = s.ToAsciiString();
                            var saved = Db.SQL<SavedSession>("SELECT s FROM SavedSession s WHERE s.SessionId = ?", ss).First;
                            saved.Delete();
                        });
                    });

                    standalone.Html = "/Chatter/viewmodels/StandalonePage.html";

                    Db.Transact(() => {
                        new SavedSession {
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

            RegisterLauncher();
            RegisterPartials();
            RegisterMap();
        }

        protected void RegisterLauncher() {
            Handle.GET("/chatter/app-name", () => {
                return new AppName();
            });

            Handle.GET("/chatter/menu", () => {
                var p = new Page {
                    Html = "/Chatter/ViewModels/MenuPage.html"
                };
                return p;
            });
        }

        protected void RegisterPartials() {
            Handle.GET("/chatter/partials/chatgroups/{?}", (string chatGroupId) => {
                var page = new ChatGroupPage {
                    Html = "/Chatter/ViewModels/ChatGroupPage.html"
                };

                page.RefreshData(chatGroupId);

                return page;
            });

            Handle.GET("/chatter/partials/chatmessages/{?}", (string objectId) => {
                var message = DbHelper.FromID(DbHelper.Base64DecodeObjectID(objectId)) as ChatMessage;
                var page = new ChatMessagePage
                {
                    Data = message,
                    PreviewText = Self.GET("/chatter/partials/chatattachmenttextpreview/" + objectId)
                };
                page.RefreshData(objectId);
                return page;
            });

            Handle.GET("/chatter/partials/chatgroups", () => {
                var page = new LobbyPage {
                    Html = "/Chatter/ViewModels/LobbyPage.html"
                };

                page.RefreshData();

                return page;
            });

            Handle.GET("/chatter/partials/people/{?}", (string personId) => {
                var page = new PersonPage();

                page.RefreshData(personId);

                return page;
            });

            Handle.GET("/chatter/partials/systemuser/{?}", (string systemUserId) => {
                var page = new SystemUserPage();
                page.RefreshData(systemUserId);
                return page;
            });
            
            #region Draft handlers
            Handle.GET("/chatter/partials/chatdraftannouncements/{?}", (string relationId) => {
                var page = new ChatMessagePage
                {
                    Html = "/Chatter/ViewModels/ChatMessageDraft.html"
                };
                var relation = (Relation)DbHelper.FromID(DbHelper.Base64DecodeObjectID(relationId));
                page.RefreshData(relation.ToWhat.GetObjectID());
                page.SetDraft(relation);
                return page;
            });
            #endregion

            #region Custom application handlers
            Handle.GET("/chatter/partials/chatmessages-draft/{?}", (string chatMessageId) =>
            {
                var chatMessage = (ChatMessage)DbHelper.FromID(DbHelper.Base64DecodeObjectID(chatMessageId));
                var relation = new ChatMessageTextRelation
                {
                    Concept = chatMessage
                };

                var page = new ChatAttachmentPage
                {
                    SubPage = Self.GET("/chatter/partials/chatdraftannouncements/" + relation.GetObjectID())
                };
                return page;
            });
            Handle.GET("/chatter/partials/chatattachmenttextpreview/{?}", (string chatMessageId) =>
            {
                var chatMessage = (ChatMessage)DbHelper.FromID(DbHelper.Base64DecodeObjectID(chatMessageId));
                if (chatMessage.IsDraft) return new Page();

                var textRelation = Db.SQL<ChatMessageTextRelation>(@"Select m from Simplified.Ring6.ChatMessageTextRelation m Where m.ToWhat = ?", chatMessage).First;
                var chatMessageTextId = textRelation?.Content?.GetObjectID();
                if(chatMessageTextId == null) return new Page();

                var page = new ChatMessageTextPreviewPage();
                page.RefreshData(chatMessageTextId);
                return page;
            });
            Handle.GET("/chatter/partials/chatattachments/{?}", (string textRelationId) => 
            {
                var textRelation = DbHelper.FromID(DbHelper.Base64DecodeObjectID(textRelationId)) as ChatMessageTextRelation;
                if (textRelation == null) return new Page();

                var page = new ChatMessageTextPage();
                page.AddNew(textRelation);
                return page;
            });
            Handle.GET("/chatter/partials/chatwarnings/{?}", (string textRelationId) =>
            {
                var textRelation = DbHelper.FromID(DbHelper.Base64DecodeObjectID(textRelationId)) as ChatMessageTextRelation;
                if (textRelation == null) return new Page();

                var page = new ChatMessageTextWarningPage();
                page.RefreshData(textRelation);
                return page;
            });
            #endregion
        }

        protected void RegisterMap() {
            UriMapping.Map("/chatter/app-name", "/sc/mapping/app-name");
            UriMapping.Map("/chatter/menu", "/sc/mapping/menu");

            UriMapping.OntologyMap("/chatter/partials/people/@w", "simplified.ring2.person");

            #region Custom application ontology mapping
            UriMapping.OntologyMap("/chatter/partials/chatmessages/@w", "simplified.ring6.chatmessage", (string objectId) => objectId, (string objectId) =>
            {
                var message = DbHelper.FromID(DbHelper.Base64DecodeObjectID(objectId)) as ChatMessage;
                return message.IsDraft ? null : objectId;
            });
            UriMapping.OntologyMap("/chatter/partials/chatmessages-draft/@w", "simplified.ring6.chatmessage", (string objectId) => objectId, (string objectId) =>
            {
                var chatMessage = (ChatMessage)DbHelper.FromID(DbHelper.Base64DecodeObjectID(objectId));
                return chatMessage.IsDraft ? objectId : null;
            });

            UriMapping.OntologyMap("/chatter/partials/chatattachments/@w", "simplified.ring6.chatattachment");
            UriMapping.OntologyMap("/chatter/partials/chatdraftannouncements/@w", "simplified.ring6.chatdraftannouncement");
            UriMapping.OntologyMap("/chatter/partials/chatwarnings/@w", "simplified.ring6.chatwarning");
            #endregion
        }
    }
}
