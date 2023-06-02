# Online players pool - PoC

Proof of Concept described in the document.

When run, please follow http://localhost:5000 to see the test ing controls. Keep console log open to see what happens in the service.

Contains all key elements of "Real Time Messaging Service":

* Online Player Pool
* EventQueue with EventQueueRunner
* PlayerAdded, PlayerRemoved, PlayerRankChanged handler
* Message Sender Job
* Metrics Collector Job

Doesn't contain MQ/SignalR. Events are generated from endpoints, final SignalR events (see `LeaderboardMessage` class) are logged to console.
