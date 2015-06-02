# Chatter
Instant messaging app with multiple chat groups and push capabilities.

Uses `Session.ForAll` / `CalculatePatchAndPushOnWebSocket` to push messages to all clients connected to a chat group.

Allows to mention `Something` as a chat message attachment. All running apps get a chance to respond with a representation of this `Something`. For example, you get output from Images if the attachment is an image or from People if the attachment is a person.
