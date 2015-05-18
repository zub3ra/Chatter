using System;
using Starcounter;
using PolyjuiceNamespace;

namespace Chatter {
    class Program {
        static void Main() {
            Handle.GET("/chatter/app-name", () => {
                return new AppName();
            });

            Handle.GET("/chatter/app-icon", () => {
                Page p = new Page() {
                    Html = "/Chatter/ViewModels/AppIconPage.html"
                };
                return p;
            });

            Handle.GET("/chatter/menu", () => {
                Page p = new Page() {
                    Html = "/Chatter/ViewModels/MenuPage.html"
                };
                return p;
            });

            Handle.GET("/chatter/master", () => {
                Session session = Session.Current;

                if (session != null && session.Data != null)
                    return session.Data;

                var master = new MasterPage();

                if (session == null) {
                    session = new Session(SessionOptions.PatchVersioning);
                    master.Html = "/Chatter/viewmodels/MasterPage.html";
                } else {
                    master.Html = "/Chatter/viewmodels/LauncherWrapperPage.html";
                }

                master.Session = session;
                return master;
            });

            Handle.GET("/chatter", () => {
                return Self.GET("/chatter/rooms");
            });

            Handle.GET("/chatter/rooms", () => {
                var master = (MasterPage)Self.GET("/chatter/master");
                if (!(master.CurrentPage is LobbyPage)) {
                    master.CurrentPage = new LobbyPage() {
                        Html = "/Chatter/ViewModels/LobbyPage.html"
                    };
                }
                return master;
            });

            Handle.GET("/chatter/rooms/{?}", (string roomName) => {
                var room = Db.SQL<Room>("SELECT r FROM Room r WHERE r.name = ?", roomName).First;
                if (room == null) {
                    Db.Transact(() => {
                        room = new Room() { name = roomName };
                    });
                };

                var master = (MasterPage)Self.GET("/chatter/master");
                if (!(master.CurrentPage is RoomPage)) {
                    master.CurrentPage = new RoomPage() {
                        Html = "/Chatter/ViewModels/RoomPage.html"
                    };
                }

                var page = (RoomPage)master.CurrentPage;
                page.Data = room;
                page.RefreshStatements();

                return master;
            });

            Polyjuice.Map("/chatter/app-name", "/polyjuice/app-name");
            Polyjuice.Map("/chatter/app-icon", "/polyjuice/app-icon");
            Polyjuice.Map("/chatter/menu", "/polyjuice/menu");
        }
    }

}
