using System;
using System.Linq;
using Starcounter;
using Starcounter.Internal;
using Simplified.Ring1;
using Simplified.Ring3;
using Simplified.Ring5;
using Simplified.Ring6;

namespace Chatter {

    partial class ChatGroupPage : Page, IBound<ChatGroup> {
        private static object sessionLock = new object();
        private long maxMsgs = 10;

        public void RefreshData(string ChatGroupId) {
            var group = DbHelper.FromID(DbHelper.Base64DecodeObjectID(ChatGroupId)) as ChatGroup;
            
            this.RefreshUser();
            this.Data = group;
            this.RefreshChatMessages();
            this.SetNewChatMessage();
        }

        public void RefreshUser() {
            SystemUserSession session = this.GetCurrentSystemUserSession();

            if (session != null) {
                this.SystemUser.Data = session.Token.User;
                this.UserName = session.Token.User.Username;
            } else {
                this.SystemUser.Data = null;
                this.UserName = "Anonymous";
            }
        }

        public void RefreshChatMessages() {
            long count = Db.SlowSQL<long>(@"
					SELECT COUNT(*) FROM Simplified.Ring6.ChatMessage m 
                    WHERE m.""Group"" = ?", this.Data).First;

            long offset = count > maxMsgs ? count - maxMsgs : 0;
            var messages = Db.SQL<ChatMessage>(@"
					SELECT m FROM Simplified.Ring6.ChatMessage m WHERE m.""Group"" = ? 
					ORDER BY m.""Date"" ASC FETCH ? OFFSET ?", this.Data, 10, offset);

            this.ChatMessagePages.Clear();

            foreach (ChatMessage item in messages) {
                var page = Self.GET<Json>("/chatter/partials/chatmessages/" + item.Key);
                this.ChatMessagePages.Add(page);
            }
        }

        public void SetNewChatMessage() {
            this.ChatMessage.Data = new ChatMessage() {
                Group = this.Data
            };

            this.Warning = string.Empty;
        }

        public void AddAttachment(Something Something) {
            ChatAttachment attachment = new ChatAttachment() {
                Message = this.ChatMessage.Data,
                Attachment = Something
            };

            //this.ChatMessage.ChatAttachments.Add().Data = attachment;
        }

        protected void PushChanges(string ChatMessageKey) {
            Session.ForAll((Session s) => {
                StandalonePage master = s.Data as StandalonePage;

                if (master != null && master.CurrentPage is ChatGroupPage) {
                    ChatGroupPage page = (ChatGroupPage)master.CurrentPage;

                    if (page.Data.Equals(this.Data)) {
                        if (page.ChatMessagePages.Count >= maxMsgs) {
                            page.ChatMessagePages.RemoveAt(0);
                        }

                        page.ChatMessagePages.Add(Self.GET<Json>("/chatter/partials/chatmessages/" + ChatMessageKey));
                        s.CalculatePatchAndPushOnWebSocket();
                    }
                }
            });
        }

        void Handle(Input.Send Action) {
            if (string.IsNullOrEmpty(this.ChatMessage.Text)) {
                Warning = "Message cannot be empty";
                return;
            }

            var images = Db.SQL<Illustration>("SELECT i FROM Simplified.Ring1.Illustration i WHERE i.Concept = ?", this.Data);

            foreach (Illustration image in images) {
                this.AddAttachment(image);
                image.Concept = this.ChatMessage.Data;
            }

            this.ChatMessage.Data.Date = DateTime.Now;
            this.ChatMessage.Data.UserName = this.UserName;
            this.ChatMessage.Data.User = this.SystemUser.Data;
            this.Transaction.Commit();
            this.PushChanges(this.ChatMessage.Data.Key);
            this.SetNewChatMessage();
        }

        void Handle(Input.AttachmentName Action) {
            this.FoundAttachment.Clear();
            this.FoundAttachment.Data = Db.SQL<Something>("SELECT s FROM Simplified.Ring1.Something s WHERE s.Name LIKE ? ORDER BY s.Name", "%" + Action.Value + "%").Take(10);
        }

        void Handle(Input.ClearFoundAttachments Action) {
            this.FoundAttachment.Clear();
            this.AttachmentName = string.Empty;
        }

        protected SystemUserSession GetCurrentSystemUserSession() {
            return Db.SQL<SystemUserSession>("SELECT o FROM Simplified.Ring5.SystemUserSession o WHERE o.SessionIdString = ?", Session.Current.SessionIdString).First;
        }

        [ChatGroupPage_json.SystemUser]
        public partial class ChatGroupPageSystemUser : Json, IBound<SystemUser> { 
        }

        [ChatGroupPage_json.FoundAttachment]
        public partial class ChatGroupPageFoundAttachment : Json, IBound<Something> {
            void Handle(Input.Choose Action) {
                this.ParentPage.AddAttachment(this.Data);
            }

            ChatGroupPage ParentPage {
                get {
                    return this.Parent.Parent as ChatGroupPage;
                }
            }
        }

        [ChatGroupPage_json.ChatMessage]
        public partial class ChatGroupPageChatMessage : Json, IBound<ChatMessage> { 
        
        }

        [ChatGroupPage_json.ChatMessage.ChatAttachments]
        public partial class ChatGroupPageChatMessageChatAttachment : Json, IBound<ChatAttachment> {
            void Handle(Input.Delete Action) {
                this.Data.Delete();
            }

            ChatGroupPage ParentPage {
                get {
                    return this.Parent.Parent.Parent as ChatGroupPage;
                }
            }
        }
    }
}
