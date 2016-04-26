using Chatter.Helpers;
using Starcounter;
using Simplified.Ring6;
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
                    Data = message
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

            Handle.GET("/chatter/partials/person/{?}", (string personId) => {
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
            Handle.GET("/chatter/partials/chatmessagedraft/{?}", (string relationId) => {
                var page = new ChatMessagePage
                {
                    Html = "/Chatter/ViewModels/ChatMessageDraft.html"
                };
                var relation = (Relation)DbHelper.FromID(DbHelper.Base64DecodeObjectID(relationId));
                page.RefreshData(relation.ToWhat.GetObjectID());
                page.SetDraft(relation);
                return page;
            });
            Handle.GET("/chatter/partials/chatattachment/{?}", (string objectId) => null);
            Handle.GET("/chatter/partials/chatwarning/{?}", (string objectId) => null);
            #endregion

            #region Custom application handlers
            Handle.GET("/chatter/partials/chatattachmenttext/{?}", (string chatMessageId) =>
            {
                var chatMessage = (ChatMessage)DbHelper.FromID(DbHelper.Base64DecodeObjectID(chatMessageId));
                var chatMessageText = new ChatMessageText();
                chatMessageText.MessageText = chatMessageText;
                chatMessageText.ChatMessage = chatMessage;

                var draft = Self.GET("/chatter/partials/chatdraftannouncement/" + chatMessageText.GetObjectID());
                return draft;
            });
            Handle.GET("/chatter/partials/chatattachmenttextpreview/{?}", (string chatMessageTextId) =>
            {
                var page = new ChatMessageTextPreviewPage();
                page.RefreshData(chatMessageTextId);
                return page;
            });
            Handle.GET("/chatter/partials/chatmessagetext/{?}", (string chatMessageTextId) =>
            {
                var page = new ChatMessageTextPage();
                page.RefreshData(chatMessageTextId);
                return page;
            });
            Handle.GET("/chatter/partials/chatdraftannouncement/{?}", (string objectPath) => null);

            Handle.GET("/chatter/partials/chatmessagetextwarning/{?}", (string chatMessageTextId) =>
            {
                var page = new ChatMessageTextWarningPage();
                page.RefreshData(chatMessageTextId);
                return page;
            });
            #endregion
        }

        protected void RegisterMap() {
            UriMapping.Map("/chatter/app-name", "/sc/mapping/app-name");
            UriMapping.Map("/chatter/menu", "/sc/mapping/menu");

            UriMapping.OntologyMap("/chatter/partials/person/@w", "simplified.ring2.person", null, null);
            UriMapping.OntologyMap("/chatter/partials/chatmessages/@w", "simplified.ring6.chatmessage", (string objectId) => objectId, (string objectId) =>
            {
                var chatMessage = (ChatMessage)DbHelper.FromID(DbHelper.Base64DecodeObjectID(objectId));
                return chatMessage.IsDraft ? null : objectId;
            });

            #region Draft ontology mapping
            UriMapping.OntologyMap("/chatter/partials/chatmessagedraft/@w", "simplified.ring6.chatdraftannouncement", null, null);
            UriMapping.OntologyMap("/chatter/partials/chatattachment/@w", "simplified.ring6.chatattachment", objectId => objectId, objectId => null);
            UriMapping.OntologyMap("/chatter/partials/chatwarning/@w", "simplified.ring6.chatwarning", objectId => objectId, objectId => null);
            #endregion

            #region Custom application ontology mapping
            UriMapping.OntologyMap("/chatter/partials/chatattachmenttext/@w", "simplified.ring6.chatmessage", (string objectId) => objectId, (string objectId) =>
            {
                var chatMessage = (ChatMessage)DbHelper.FromID(DbHelper.Base64DecodeObjectID(objectId));
                return chatMessage.IsDraft ? objectId : null;
            });
            UriMapping.OntologyMap("/chatter/partials/chatattachmenttextpreview/@w", "simplified.ring6.chatmessage", (string objectId) => objectId, (string objectId) =>
            {
                var chatMessage = (ChatMessage)DbHelper.FromID(DbHelper.Base64DecodeObjectID(objectId));
                if (chatMessage.IsDraft) return null;

                var chatMessageText = Db.SQL<ChatMessageText>(@"Select m from Simplified.Ring6.ChatMessageText m Where m.ToWhat = ?", chatMessage).First;
                return chatMessageText?.GetObjectID();
            });
            UriMapping.OntologyMap("/chatter/partials/chatmessagetext/@w", "simplified.ring6.chatattachment", (string objectId) => objectId, (string objectId) =>
            {
                var chatMessageText = DbHelper.FromID(DbHelper.Base64DecodeObjectID(objectId));
                return chatMessageText.GetType() == typeof(ChatMessageText) ? chatMessageText.GetObjectID() : null;
            });
            UriMapping.OntologyMap("/chatter/partials/chatdraftannouncement/@w", "simplified.ring6.chatdraftannouncement", objectId => objectId, objectId => null);
            UriMapping.OntologyMap("/chatter/partials/chatmessagetextwarning/@w", "simplified.ring6.chatwarning", (string objectId) => objectId, (string objectId) =>
            {
                var chatMessageText = DbHelper.FromID(DbHelper.Base64DecodeObjectID(objectId));
                if (chatMessageText.GetType() != typeof (ChatMessageText)) return null;

                var result = ChatMessageTextValidator.IsValid(chatMessageText as ChatMessageText);
                return string.IsNullOrEmpty(result) ? null : chatMessageText.GetObjectID();
            });
            #endregion
        }
    }
}
