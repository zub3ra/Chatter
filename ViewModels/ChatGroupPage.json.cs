using System;
using System.Linq;
using Starcounter;
using Simplified.Ring1;
using Simplified.Ring3;
using Simplified.Ring5;
using Simplified.Ring6;

namespace Chatter {

    partial class ChatGroupPage : Page, IBound<ChatGroup> {
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
                SystemUser user = session.Token.User;

                this.UserKey = user.Key;
                this.UserName = user.Username;
            } else {
                this.UserKey = null;
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
            var draft = new ChatMessage
            {
                IsDraft = true
            };
            SystemUser user = null;
            if (!string.IsNullOrEmpty(this.UserKey))
            {
                user = DbHelper.FromID(DbHelper.Base64DecodeObjectID(this.UserKey)) as SystemUser;
            }
            draft.Group = Data;
            draft.UserName = UserName;
            draft.User = user;
            ChatMessageDraft = Self.GET<Json>("/chatter/partials/chatmessages/" + draft.Key);
            this.Warning = string.Empty;
        }

        protected void PushChanges(string ChatMessageKey) {
            Session.ScheduleTask(Db.SQL<SavedSession>("SELECT s FROM SavedSession s").Select(x => x.SessionId).ToList(), (Session s, String sessionId) => {
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

        public void Refresh(string key)
        {
            PushChanges(key);
            SetNewChatMessage();
        }

        void Handle(Input.AttachmentSearch Action) {
            this.FoundAttachment.Clear();
            this.FoundAttachment.Data = Db.SQL<Something>("SELECT s FROM Simplified.Ring1.Something s WHERE s.Name LIKE ? ORDER BY s.Name", "%" + Action.Value + "%").Take(10);
        }

        void Handle(Input.ClearFoundAttachments Action) {
            this.FoundAttachment.Clear();
            this.AttachmentSearch = string.Empty;
        }

        protected SystemUserSession GetCurrentSystemUserSession() {
            return Db.SQL<SystemUserSession>("SELECT o FROM Simplified.Ring5.SystemUserSession o WHERE o.SessionIdString = ?", Session.Current.SessionIdString).First;
        }

        [ChatGroupPage_json.FoundAttachment]
        public partial class ChatGroupPageFoundAttachment : Json, IBound<Something> {
            protected override void OnData() {
                base.OnData();

                if (this.Data != null) {
                    this.Type = this.Data.GetType().Name;
                } else {
                    this.Type = string.Empty;
                }

                this.NameAndType = string.Format("{0} - {1}", this.Name, this.Type);
            }

            ChatGroupPage ParentPage {
                get {
                    return this.Parent.Parent as ChatGroupPage;
                }
            }
        }

        [ChatGroupPage_json.ChatMessage]
        public partial class ChatGroupPageChatMessage : Json, IBound<ChatMessage> {
            protected override void OnData()
            {
                base.OnData();
                if (this.Data != null)
                {
                    this.Type = this.Data.GetType().Name;
                }
                else
                {
                    this.Type = string.Empty;
                }
            }

            ChatGroupPage ParentPage
            {
                get
                {
                    return this.Parent.Parent.Parent as ChatGroupPage;
                }
            }
        }
    }
}
