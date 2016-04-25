using Starcounter;
using Simplified.Ring3;
using Simplified.Ring5;
using Simplified.Ring6;

namespace Chatter {

    partial class ChatGroupPage : Page, IBound<ChatGroup> {
        private readonly long _maxMsgs = 10;

        public void RefreshData(string chatGroupId) {
            var group = DbHelper.FromID(DbHelper.Base64DecodeObjectID(chatGroupId)) as ChatGroup;
            RefreshUser();
            Data = group;
            RefreshChatMessages();
            SetNewChatMessage();
        }

        public void RefreshUser() {
            var session = GetCurrentSystemUserSession();

            if (session != null) {
                var user = session.Token.User;

                UserKey = user.Key;
                UserName = user.Username;
            } else {
                UserKey = null;
                UserName = "Anonymous";
            }
        }

        public void RefreshChatMessages() {
            var count = Db.SlowSQL<long>(@"
					SELECT COUNT(*) FROM Simplified.Ring6.ChatMessage m 
                    WHERE m.""Group"" = ?", Data).First;

            var offset = count > _maxMsgs ? count - _maxMsgs : 0;
            var messages = Db.SQL<ChatMessage>(@"
					SELECT m FROM Simplified.Ring6.ChatMessage m WHERE m.""Group"" = ? 
					ORDER BY m.""Date"" ASC FETCH ? OFFSET ?", Data, 10, offset);

            ChatMessagePages.Clear();

            foreach (ChatMessage item in messages) {
                var page = Self.GET<Json>("/chatter/partials/chatmessages/" + item.Key);
                ChatMessagePages.Add(page);
            }
        }

        public void SetNewChatMessage() {
            SystemUser user = null;
            if (!string.IsNullOrEmpty(UserKey))
            {
                user = DbHelper.FromID(DbHelper.Base64DecodeObjectID(UserKey)) as SystemUser;
            }
            var draft = new ChatMessage
            {
                IsDraft = true,
                Group = Data,
                UserName = UserName,
                User = user
            };
            ChatMessageDraft = Self.GET<Json>("/chatter/partials/chatmessages/" + draft.Key);
        }

        public void Refresh(string key)
        {
            ChatMessagePages.Add(Self.GET<Json>("/chatter/partials/chatmessages/" + key));
            SetNewChatMessage();
        }

        protected SystemUserSession GetCurrentSystemUserSession() {
            return Db.SQL<SystemUserSession>("SELECT o FROM Simplified.Ring5.SystemUserSession o WHERE o.SessionIdString = ?", Session.Current.SessionIdString).First;
        }
    }
}
