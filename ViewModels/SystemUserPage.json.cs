using Starcounter;
using Simplified.Ring3;

namespace Chatter {
    partial class SystemUserPage : Page, IBound<SystemUser> {
        public void RefreshData(string SystemUserId) {
            SystemUser user = DbHelper.FromID(DbHelper.Base64DecodeObjectID(SystemUserId)) as SystemUser;

            this.Data = user;
            this.Person = Self.GET("/chatter/partials/person/" + user.WhoIs.Key);
        }
    }
}