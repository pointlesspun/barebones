# Change List v0.1

* [done] Finish Timerservice - add behaviour
* [done] Scenelogic - add time out
* [done] Add timer service to start scene
* [done] Activate player on scene begin
* [done] Fix player movement
* [done] Add player spawn positions
* [done] Restore PlayerLobbyStatus
* [done] Stage listener addition / removal
* [done] attach gamepad movement
* [fixed] bug, Joining and leaving more than Max Players causes player to lose controls and other weird stuff
* [fixed] bug: the third controller causing in game spawning to break
* [fixed] bug: when attaching two controllers, cancel removes both players in the lobby. Both players are controlled for the same devices.
* [done] not happy with how gamelistener is exposing something internal to the implementation (state) find a way to fix that
	* [done] complete messagebus tests
	* [done] replace gamemessage bus with messagebus
	* [done] document messagebus tests
	* [done] Message bus behaviour
	* [done] Replace message bus in scene & message bus listeners
	* [done] bug: player died is not picked up the second playthrough
	* [done] bug: in stand alone game, player does not own the device
	* [later] Not worth it for now. Allow sending messages to specific listeners (already there via something like topic=address, id = handle, payload == actual message)
	* [done] Listener update move try catch around the actual update
	* [done] Add optional logging
* [later] Micro optimization not worth it for now. device ids in playeroot is now an open ended array, can be limited to two properties dev1 dev2
* [done] replace timers / player registry parts with SlotArray?
	* [done] update timers
	* [done] test timer service
	* [done] check player registry
	* [done] add tests for enumeration & clear on SlotArray
* [later] adjust slotarray to allow for object pooling ? it's possible but I'm not sure if it will add anything and 
it requires the slotarray to work differently than it does now. I do want (eventually) a generic, tested object pool.
* [in-progress] general clean up & unit test & documentation & publish
	* [done] move code to services and add namespaces
	* [done] fix for player bullets moving erratically over time. prolly because of impacts with enemies before they were recycled. reset
	  movement direction, angular velocity on reset. 
	* [done] Is ObjectPoolBehaviour used ??
	* [done] Update to latest unity version 
	* [done] Replace all Linq references with Enumerations
	* [done] Organize Scene hierarchy -> /Scene/... /System/...
	* [done] add document outlining
		* intentions
		* high level operation
		* services
	* [done]add to github