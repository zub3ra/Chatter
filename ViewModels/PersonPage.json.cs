using Starcounter;
using Simplified.Ring2;

namespace Chatter {
    partial class PersonPage : Json, IBound<Person> {
        public void RefreshData(string PersonId) {
            Person person = DbHelper.FromID(DbHelper.Base64DecodeObjectID(PersonId)) as Person;

            this.Data = person;
        }
    }
}
