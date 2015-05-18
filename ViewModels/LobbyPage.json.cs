using Starcounter;

partial class LobbyPage : Page {
    void Handle(Input.GoToRoom action) {
        RedirectUrl = "/chatter/rooms/" + Room;
    }
}
