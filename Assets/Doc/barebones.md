
<center>

# Bare-Bones Framework 0.1

<img src="logo.png" alt="BareBones logo" width="128" height="128" style="image-rendering:pixelated">
</center>

The BareBones framework is a minimal bare-bones set of code to get a Unity-based game up and running quickly. The intention is to have an end-to-end solution which does as little as possible in a clear and consise manner, making it easy to configure, add or remove assets as needed. ... Provided the user is somewhat fluent in Unity and coding. 

BareBones provides the following basic elements:

* Minimal scenes, a 'lobby' and a 'game' scene.
* Some basic prefabs.
* A very (very) simple, local player shoot em up.
* A set of services (discoverable via a resource locator), such as:
	* A Message Bus to broadcast messages providing a 
	* An Object Pool
	* A Player Registry, tracking which players are in the game.
	* A TimeService to set timeouts
* Some common code such as datastructures, enumeration support, utilities for game objects, physics and common interfaces.
* Unit test configuration & minimal test coverage
* Text mesh pro configuration (aka it's downloaded and added to the project)

## Known Limitations 

* No online game play in this version.
* Serialization is not guaranteed to work. A custom serialization solution is necessary.
* Documentation is extremely sparse at this point...

## Random implementation notes

### Scene Setup
Each scene contains a top level 'Scene' and 'System' object. The Scene object contains objects related to the specific scene (eg camera, scene logic and so on). The System contains services such as the Message Bus, Object Pool, Player Registry or TimeService. The System implements a DoNotDestroy Pattern only instantiating when no other System is present in . The Object Pool (Collection) may appear in both the system object (shared/common object pools) and scene object (scene specific object pools). If an Object Pool already exists when a scene is loaded, it's merged into the existing pool. 

The current only contains two scenes. The Lobby scene and the InGame scene. 

### The Message Bus
To avoid hard coupling between components and gameobjects a message bus is used (thus implementing a 'soft' coupling). Components can subscribe to this message bus if they implement the IMessageListener interface and/or send (broadcast) messages to this bus. One thing to note is that the message bus does not make any guarantees about when a message will be delivered to its listeners, DO NOT make implementations relying on guaranteed order of delivery. 

### The Player & PlayerRegistry
A 'player' is a two part composition in a parent-child relationship namely:
	* The player GameObject containing a Player Component.  
	* The player's controlled 'physical' object, it's agent, implementing the IAgentController interface.

The player's ontains the entry point for Player specific data (lives, score and so on) and the Player Input. The input will be translated into actions which will be transferred to the player's controlled body (IAgentController). The player does not have a physical manifestation in the game but a controlled 'avatar' or 'agent'. If the avatar dies, the player dies. 

The Players are stored in the Player Registry (Service) and added in the lobby via player join actions (mouse button, gamepad button) or when starting with the GameScene determined via the 'Initial Active Players' in the Player Registry Behaviour.

### Timer Service

This is a very minimal service which allows for setting (and canceling) time out callbacks. 

## Detailed Intentions

Best intentions, roads paved...

* Easy to understand & pick up (for the inevitable situation where the project has to be shelved for an extended amount of time). 
* Minimal, up to date documentation.
* KISS, avoid overly smart implementations, avoid behavioural quirks or setup. 
* Provide warnings (Asserts All Over) as quick as possible when dependencies are not met. 
* Make code testable. 
* Flexible (where that makes sense). 
* Decouple code (use interfaces, resource locator). 
* Make code stand-alone (minimal unity dependencies where possible). 
* Be able to start a functional game from every scene (no need to start with the lobby then enter gameplay). Standard scene setup with a 'scene' and 'system' game object. 
* Support local multiplayer (and deal with some of the functionality trivial, but implementation wise annoying issues). 
* Use standard Unity solutions where available (unless they seems to behave in an 'unexpected' fashion -- eg PlayerInputManager).
* Use and provide common service implementations (message bus, object pool...).
* Clear, simle action list (in doc/todo.md)
* Know the memory requirements in advance, avoid new(), use ObjectPools. 
