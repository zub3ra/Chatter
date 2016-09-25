using Starcounter;
using Simplified.Ring3;

namespace Chatter {
    partial class SystemUserPage : Json, IBound<SystemUser> {
        public void RefreshData(string SystemUserId) {
            SystemUser user = DbHelper.FromID(DbHelper.Base64DecodeObjectID(SystemUserId)) as SystemUser;

            this.Data = user;
            this.Person = Self.GET("/chatter/partials/people/" + user.WhoIs.Key);
        }
    }
}
