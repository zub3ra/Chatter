using Starcounter;
using Simplified.Ring3;
using Simplified.Ring5;
using Simplified.Ring6;
using System.Linq;

namespace Chatter
{
    partial class ChatGroupPage : Json, IBound<ChatGroup>
    {
        private readonly long _maxMsgs = 10;

        public void RefreshData(string chatGroupId)
        {
            var group = DbHelper.FromID(DbHelper.Base64DecodeObjectID(chatGroupId)) as ChatGroup;
            RefreshUser();
            Data = group;
            RefreshChatMessages();
            SetNewChatMessage();
        }

        public void RefreshUser()
        {
            var session = GetCurrentSystemUserSession();

            if (session != null)
            {
                var user = session.Token.User;

                UserKey = user.Key;
                UserName = user.Username;
            }
            else {
                UserKey = null;
                UserName = "Anonymous";
            }
        }

        protected void PushChanges(string chatMessageKey)
        {
            var sessions = Db.SQL<SavedSession>("SELECT s FROM SavedSession s").Select(x => x.SessionId).ToList();
            Session.ScheduleTask(sessions, (Session s, string sessionId) =>
            {
                StandalonePage master = s.Data as StandalonePage;

                var page = master?.CurrentPage as ChatGroupPage;
                if (page?.Data != null && page.Data.Equals(Data))
                {
                    if (page.ChatMessagePages.Count >= _maxMsgs)
                    {
                        page.ChatMessagePages.RemoveAt(0);
                    }

                    page.ChatMessagePages.Add(Self.GET<Json>("/chatter/partials/chatmessages/" + chatMessageKey));
                    s.CalculatePatchAndPushOnWebSocket();
                }
            });
        }

        public void RefreshChatMessages()
        {
            var count = Db.SlowSQL<long>(@"
					SELECT COUNT(*) FROM Simplified.Ring6.ChatMessage m 
                    WHERE m.""Group"" = ?", Data).First;

            var offset = count > _maxMsgs ? count - _maxMsgs : 0;
            var messages = Db.SQL<ChatMessage>(@"
					SELECT m FROM Simplified.Ring6.ChatMessage m WHERE m.""Group"" = ? AND m.IsDraft = ?
					ORDER BY m.""Date"" ASC FETCH ? OFFSET ?", Data, false, 10, offset);

            ChatMessagePages.Clear();

            foreach (ChatMessage item in messages)
            {
                var page = Self.GET<Json>("/chatter/partials/chatmessages/" + item.Key);
                ChatMessagePages.Add(page);
            }
        }

        public void SetNewChatMessage()
        {
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
            ChatMessageDraft = Self.GET("/chatter/partials/chatmessages/" + draft.Key);
            ChatMessageTextDraft = GetTextAttachment(draft);
        }

        public void Refresh(string key)
        {
            PushChanges(key);
            SetNewChatMessage();
        }

        private Json GetTextAttachment(ChatMessage chatMessage)
        {
            var relation = new ChatMessageTextRelation { Concept = chatMessage };
            return new ChatAttachmentPage
            {
                SubPage = new ChatMessagePage
                {
                    Html = "/Chatter/ViewModels/ChatMessageDraft.html",
                    Data = chatMessage,
                    Relation = relation,
                    Draft = Self.GET("/chatter/partials/chatattachmenttext/" + relation.GetObjectID())
                }
            };
        }

        protected SystemUserSession GetCurrentSystemUserSession()
        {
            return Db.SQL<SystemUserSession>("SELECT o FROM Simplified.Ring5.SystemUserSession o WHERE o.SessionIdString = ?", Session.Current.SessionId).First;
        }
    }
}
