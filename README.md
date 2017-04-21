# Chatter

Chat about anything. Create chat rooms, mention things, add attachments. Messages are pushed over WebSocket.

---

Uses `Session.ForAll` / `CalculatePatchAndPushOnWebSocket` to push messages to all clients connected to a chat group.

Allows to mention `Something` as a chat message attachment. All running apps get a chance to respond with a representation of this `Something`. For example, you get output from Images if the attachment is an image or from People if the attachment is a person.

## Developer instructions

For developer instructions, go to [CONTRIBUTING](CONTRIBUTING.md)

## License

MIT
