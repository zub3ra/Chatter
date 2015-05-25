using System;
using System.IO;
using Starcounter;

namespace Chatter {
    internal class DefaultStyles {
        public void ApplyIfEmpty() {
            if (Db.SQL("SELECT * FROM JuicyTiles.JuicyTilesSetup WHERE Key LIKE ?", "/Chatter/%").First != null) {
                return;
            }

            this.Apply();
        }

        public void Apply() {
            TextReader treader = new StreamReader(typeof(DefaultStyles).Assembly.GetManifestResourceStream("Chatter.Content.default-layout.sql"));
            string sql = treader.ReadToEnd();

            treader.Dispose();

            Db.Transact(() => {
                Db.SlowSQL(sql);
            });
        }

        public void Clear() {
            Db.Transact(() => {
                Db.SlowSQL("DELETE FROM JuicyTiles.JuicyTilesSetup WHERE Key LIKE '/Chatter/%'");
            });
        }
    }
}
