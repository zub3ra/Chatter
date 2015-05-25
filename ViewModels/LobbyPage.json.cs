using Starcounter;

partial class LobbyPage : Page {
    void Handle(Input.GoToGroup Action) {
        RedirectUrl = "/chatter/chatgroup/" + Group;
    }
}
