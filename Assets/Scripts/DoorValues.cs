using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This script is attached to every door in the game. It contains all necessary values of each door and it also handles locking and unlocking of the door.
public class DoorValues : MonoBehaviour
{
    public bool isExit = false;
    public int doorNumber = 11;
    public bool wasExplored = false;
    public bool isBeingDestroyed = false;
    public bool isOpen = false;
    private DoorValues[] allDoorScripts;
    private GameObject thisDoor;
    //private List<GameObject> doors;
    public int keysRequired;

    //Start initializes the thisDoor variable with a reference to the game object that this script is attached to.
    void Start()
    {        
        thisDoor = gameObject;
    }

    //UnlockDoor sets isOpen to open, moves this door to OLD walls layer and then disables the door's sprite so the player can not see it or interact with it.
    public void UnlockDoor()
    {
        isOpen = true;
        thisDoor.layer = 8;
        SpriteRenderer doorSprite = thisDoor.GetComponent<SpriteRenderer>();
        doorSprite.enabled = false;
    }

    //LockDoor sets isOpen to closed and moves this door to the walls layer so the player can see it and interact with it.
    public void LockDoor()
    {
        isOpen = false;
        thisDoor.layer = 6;
    }
    
}
