using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//This script is attached to every bomb object that the player character creates in the scene. It handles bomb explosion and the destruction of all objects surrounding the bomb.
public class BombValuesNActions : MonoBehaviour
{
    public int turnsPassed;
    private float boomDistStraight, boomDistDiagonal;
    private GameObject player, bomb;
    private PlayerActionScript playerMovement;
    private GameObject mino;
    private MinotaurMovement minoScript;
    private Vector2 bombPosition;
    [SerializeField]
    private LayerMask wallsLayer, objectsLayer;
    [SerializeField]
    private string wallTagExplored, wallTagUnexplored;
    [SerializeField]
    private int oldWallsLayer;
    [SerializeField]
    private float waitTimer = 0.1f;
    [SerializeField]
    private float boomDistMargin;
    List<GameObject> wallsOnFire, doorsOnFire;
    private GameObject announcePanel, announceTextObj, announceChildObj;
    private Transform announceChild;
    private Text announceText;

    //This class lacks a constructor. Instead of setting variables in the class, most variables should be set in a constructor.

    //Start first creates two lists for walls and doors near the bomb, then it sets the turn timer to zero.
    //Afterwards the method fetches and sets all necessary variables.
    //Setting up many of these variables could be done in a constructor.
    void Start()
    {
        wallsOnFire = new List<GameObject>();
        doorsOnFire = new List<GameObject>();
        turnsPassed = 0;
        bomb = this.gameObject;
        bombPosition = bomb.transform.position;
        playerMovement = GameObject.FindObjectOfType<PlayerActionScript>();
        player = playerMovement.gameObject;
        mino = GameObject.FindGameObjectWithTag("Minotaur");
        if (mino.GetComponent<MinotaurMovement>() != null)
        {
            minoScript = mino.GetComponent<MinotaurMovement>();
        }        
        boomDistStraight = PlayerActionScript.RaycastDistance;
        boomDistDiagonal = PlayerActionScript.RaycastDistance + boomDistMargin;
        announcePanel = GameObject.FindWithTag("AnnouncePanel");
        announceChild = announcePanel.transform.GetChild(0);
        announceChildObj = announceChild.gameObject;
        announceTextObj = GameObject.FindWithTag("AnnounceText");
        if (announceTextObj.GetComponent<Text>() != null)
        {
            announceText = announceTextObj.GetComponent<Text>();
        }
        Debug.Log(announcePanel);
        Debug.Log(announceText);
        Debug.Log(announceChild);
        Debug.Log(announceChildObj);
    }

    //Update checks every fame whether the bomb has been active for more than 2 turns. If true, it triggers a coroutine before exploding the bomb.
    //This could be improved by making the check trigger at the end of each turn instead of every frame. Secondly, the turn value could be replaced with a variable, which could be adjusted in the Unity editor.
    void Update()
    {
        if (turnsPassed >= 2)
        {
            //Explode(boomDistStraight, boomDistDiagonal);
            StartCoroutine(Wait(waitTimer, boomDistStraight, boomDistDiagonal));
        }
    }

    //Explode first awakens the minotaur and then checks in twelve directions (every multiple of 30 degrees) for walls and objects to destroy.
    //Afterwards it destroys all objects that are in the way of the explosion. Lastly it destroys the bomb itself.
    void Explode(float distStraight, float distDiagonal)
    {
        AwakenMinotaur();
        for (int i = 0; i < 13; i++)
        {
            switch (i)
            {
                case 0:
                    DoRaycast2DBoom(new Vector2(-1f, 2f), distDiagonal);
                    break;

                case 1:
                    DoRaycast2DBoom(new Vector2(1f, 2f), distDiagonal);
                    break;

                case 2:
                    DoRaycast2DBoom(new Vector2(2f, 1f), distDiagonal);
                    break;

                case 3:
                    DoRaycast2DBoom(new Vector2(-2f, 1f), distDiagonal);
                    break;

                case 4:
                    DoRaycast2DBoom(new Vector2(2f, -1f), distDiagonal);
                    break;

                case 5:
                    DoRaycast2DBoom(new Vector2(1f, -2f), distDiagonal);
                    break;

                case 6:
                    DoRaycast2DBoom(new Vector2(-2f, -1f), distDiagonal);
                    break;

                case 7:
                    DoRaycast2DBoom(new Vector2(-1f, -2f), distDiagonal);
                    break;

                case 8:
                    DoRaycast2DBoom(Vector2.right, distStraight);                    
                    break;

                case 9:
                    DoRaycast2DBoom(Vector2.left, distStraight);
                    break;

                case 10:
                    DoRaycast2DBoom(Vector2.down, distStraight);                    
                    break;

                case 11:
                    DoRaycast2DBoom(Vector2.up, distStraight);
                    break;

                case 12:
                    TryDestroyPlayer(distDiagonal);
                    break;

            }
        }
        DestroyWallsNDoors();       
        Destroy(bomb);
    }

    //DoRaycast2DBoom first shoots a ray in the set direction the walls layer. If there is a wall in the way, the method checks if it is a wall or door.
    //If there are no walls in the way, the methods then shoots another ray in the same direction in the objects layer.
    //If there is an object in the way that is not the bomb itself, the object gets checked by CheckForObject.
    void DoRaycast2DBoom(Vector2 vector, float distance)
    {
        RaycastHit2D boomHitWall = Physics2D.Raycast(bombPosition, vector, distance, wallsLayer);
        if (boomHitWall.collider != null)
        {
            //Debug.Log(boomHitWall.transform.gameObject.ToString());
            CheckForWall(boomHitWall);
        }
        else if (boomHitWall.collider == null)
        {
            RaycastHit2D[] boomHitObjects = Physics2D.RaycastAll(bombPosition, vector, distance, objectsLayer);
            foreach (RaycastHit2D boomHitObject in boomHitObjects)
            {
                if (boomHitObject.collider != null && boomHitObject.transform != null)
                {
                    GameObject explodeyObject = boomHitObject.transform.gameObject;
                    if (explodeyObject != bomb)
                    {
                        CheckForObject(boomHitObject, vector);
                    }
                }
            }
        }       
    }

    //TryDestroyPlayer checks whether the player stands on an adjacent tile to the bomb. If yes, then the player gets killed.
    void TryDestroyPlayer(float distance)
    {
        Vector2 playerPosition = player.transform.position;
        float distToPlayerX = Mathf.Abs(bombPosition.x - playerPosition.x);
        float distToPlayerY = Mathf.Abs(bombPosition.y - playerPosition.y);
        if (!(distToPlayerX > 2f || distToPlayerY > 2f))
        {
            player.SetActive(false);
        }
    }

    //CheckForWall first checks whether the hit object has a Transform component, then it checks whether the hit object is a door or a wall.
    //Afterwards the method checks whether the object can be destroyed. If so, the object is then marked for destruction.
    void CheckForWall(RaycastHit2D boomHit2Da)
    {
        if (boomHit2Da.transform != null)
        {
            GameObject boomWall = boomHit2Da.transform.gameObject;
            if (boomWall.tag == wallTagExplored || boomWall.tag == wallTagUnexplored)
            {
                if (boomWall.GetComponent<WallValues>() != null)
                {
                    WallValues wallVals = boomWall.GetComponent<WallValues>();
                    if (wallVals.isDestructible == true)
                    {
                        MarkWallForDestruction(wallVals);
                    }
                }
            }
            else if (boomWall.tag == "Door" && boomWall.GetComponent<DoorValues>() != null)
            {
                DoorValues doorVals = boomWall.GetComponent<DoorValues>();
                if (doorVals.isExit == false)
                {
                    MarkDoorForDestruction(doorVals);
                }
            }
        }        
    }

    //The following two methods perform the same actions for walls and doors: they access the value scripts of the set objects and mark them for destruction.
    //Afterwards, they add the wall or the door to the list of walls and doors that are on fire.
    void MarkWallForDestruction(WallValues wVals)
    {
        wVals.isBeingDestroyed = true;
        wallsOnFire.Add(wVals.gameObject);
    }

    void MarkDoorForDestruction(DoorValues dVals)
    {
        dVals.isBeingDestroyed = true;
        doorsOnFire.Add(dVals.gameObject);
    }

    //DestroyWallsNDoors hides walls and doors from the player and then empties the lists of walls and doors on fire.
    void DestroyWallsNDoors()
    {
        foreach (GameObject wallOnFire in wallsOnFire)
        {
            HideWallForPlayer(wallOnFire);
        }
        foreach (GameObject doorOnFire in doorsOnFire)
        {
            DisableDoor(doorOnFire);
        }
        wallsOnFire.Clear();
        doorsOnFire.Clear();
    }

    //HideWallForPlayer first tries to reset isBeingDestroyed variable to false, then it moves the wall to a different layer so the player can't interact with it.
    //Afterwards the method tries to disable the SpriteRenderer component of the wall to make it invisible.
    void HideWallForPlayer(GameObject wall)
    {
        if (wall.GetComponent<WallValues>() != null)
        {
            WallValues wValues = wall.GetComponent<WallValues>();
            wValues.isBeingDestroyed = false;
        }
        wall.layer = oldWallsLayer;
        if (wall.GetComponent<SpriteRenderer>() != null)
        {
            SpriteRenderer wallRenderer = wall.GetComponent<SpriteRenderer>();
            wallRenderer.enabled = false;
        }
    }

    //DisableDoor first sets the isBeingDestroyed variable to false if it has it, then disables the door object.
    void DisableDoor(GameObject door)
    {
        if (door.GetComponent<DoorValues>() != null)
        {
            DoorValues dValues = door.GetComponent<DoorValues>();
            dValues.isBeingDestroyed = false;
        }
        door.SetActive(false);
    }

    //CheckForObject first checks whether the set object has a Transform component and then checks what kind of object it is.
    //It then tries to destroy the object. If the object couldn't be destroyed, then it is logged.
    void CheckForObject(RaycastHit2D boomHit2Db, Vector2 directionVector)
    {
        if (boomHit2Db.transform != null)
        {
            GameObject boomObject = boomHit2Db.transform.gameObject;
            if (boomObject.tag == "Key" || boomObject.tag == "Player" || boomObject.tag == "Minotaur" || boomObject.tag == "Lever")
            {
                //Debug.Log(directionVector);
                if (boomObject.tag == "Minotaur")
                {
                    DisableMinotaur();
                }
                boomObject.SetActive(false);
            }
            else if (boomObject.tag == "Bomb")
            {
                //Debug.Log(boomObject.tag);
                Destroy(boomObject);
            }
            else
            {
                Debug.Log("Could not explode " + boomObject.tag + ".");
            }
        }
    }

    //Wait waits for waitSecs seconds before calling Explode method.
    public IEnumerator Wait(float waitSecs, float distStr, float distDiag)
    {
        yield return new WaitForSeconds(waitSecs);
        Explode(distStr, distDiag);
    }

    //AwakenMinotaur awakens the minotaur and displays the warning announcement for the player.
    void AwakenMinotaur()
    {
        if (minoScript.minoIsAwake == false)
        {
            minoScript.minoIsAwake = true;
            //announcePanel.SetActive(true);
            //announceText.text = "Minotaur is awake.";
            announceText.enabled = true;
            announceText.text = "Minotaur is awake.";
            announceChild.GetComponent<Image>().enabled = true;
            announcePanel.GetComponent<Image>().enabled = true;
        }
    }

    //DisableMinotaur ensures that the minotaur stops performing any actions and then disables its game object.
    void DisableMinotaur()
    {
        minoScript.minoIsAwake = false;
        minoScript.enabled = false;
    }
}
