# To do v0.2

* [done] Rewrite ObjectPool as SlotArray (by simply inversing the use case)
    - [done] ... and tested
    - [done] check what happens if the version loops
    - [done] Test objPool clear
    - [done] Test release with state = released
    - [done] Increase version both on obtain and release
    - [done] Add sweep to objPool
    - [done] add obj pool behaviour, add sweep
    - [done] update collection 
    - [done] complete awake test
    - [done] test obtain
    - [done] test sweep
    - [done] Implement a scene demonstrating the object pool
        - have three object pools
        - spawn randomly objects from the pool
        - deactivate objects after some time, letting sweep put them back into the pool
    - [done] Replace old object pool
    - [done] Remove Nullable PoolObjectHandle in favor of a static NULL_HANDLE
    - [done] Document object pool & demo

* [in-progress]
    - [done] Add more tests to nested structure
    - [done] general clean up of the property file      
    - don't let the list and structure content fail on an end of file, let the composite deal with that
    - test marked with xxx should after the previous point now succeed
    - provide propertytableparser with a config instead of all the different whitespace, separator things
    - config should include a logger
    - have whitespace tracker object which also tracks end of lines
    - allow for comments
    - allow for non scoped strings
    - allow for ints
    - allow for case not being an issue when parsing booleans

* Add Score
* Add title screen 
* Use correct button to join on game pad
* Test enumerations
* Add freeze frame
* Add high score
* Add fade in - fade out
* Add logo
* Add Game over message
* Add enemy bullets 
* Add a wave concept (ie a number of commands send to spawners) 
* Add more assertions.
* Add more tests.
* Add more documentation.
* Check Serialization