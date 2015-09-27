# Chatter
Sample app to chat about anything. Create chat rooms. Mention things to create attachments in chat messages. Messages are pushed over WebSocket.

---

Uses `Session.ForAll` / `CalculatePatchAndPushOnWebSocket` to push messages to all clients connected to a chat group.

Allows to mention `Something` as a chat message attachment. All running apps get a chance to respond with a representation of this `Something`. For example, you get output from Images if the attachment is an image or from People if the attachment is a person.

**Note:** the application has been migrated to Polymer 1.x.
- Latest Polymer 0.5 commit: https://github.com/Polyjuice/Chatter/commit/37972debf6d78e8ae6bb6543aea3a651642f618b
- Latest Polymer 0.5 release: https://github.com/Polyjuice/Chatter/releases/tag/1.0.0