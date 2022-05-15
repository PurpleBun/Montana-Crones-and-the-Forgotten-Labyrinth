using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//This script is attached to the player character. This script handles all of player character's actions. It also performs several other actions that should instead be placed into other game objects.
public class PlayerActionScript : MonoBehaviour
{
    private Vector2 playerPosition;
    [SerializeField]
    private float moveParamY = 1f;
    [SerializeField]
    private float moveParamX = 1f;
    [SerializeField]
    private LayerMask wallLayer, objectsLayer;
    public const float RaycastDistance = 1.2f;
    private const float DistMultiplier = 1.7f;
    private const float Margin = 0.6f;
    private float direction = 1f;
    [SerializeField]
    private float waitTime = 0.2f;
    public List<int> keyChain;
    [SerializeField]
    private string WallTagExplored, WallTagUnexplored;
    private GameObject[] walls;
    private DoorValues[] doors;
    private KeyValues[] keys;
    private LeverBehaviour[] leverScripts;
    private GameObject minotaur;
    private MinotaurMovement minotaurScript;
    public GameObject bombPrefab;
    public int bombsAvailable;
    [SerializeField]
    private GameObject annPanel, annChild;
    [SerializeField]
    private Text annText;
    [SerializeField]
    private Image leverIndicator;
    [SerializeField]
    private Color leverUpColor, leverDownColor;

    //This class lacks a constructor. Instead of setting variables in the class, most variables should be set in a constructor.

    // Start finds all necessary objects that the player will interact with.
    void Start()
    {
        keyChain = new List<int>();
        doors = FindObjectsOfType<DoorValues>();
        keys = FindObjectsOfType<KeyValues>();
        walls = GameObject.FindGameObjectsWithTag(WallTagUnexplored);
        leverScripts = FindObjectsOfType<LeverBehaviour>();
        playerPosition = gameObject.transform.position;
        CheckSurroundings(RaycastDistance);
        minotaur = GameObject.FindGameObjectWithTag("Minotaur");
        if (minotaur.GetComponent<MinotaurMovement>() != null)
        {
            minotaurScript = minotaur.GetComponent<MinotaurMovement>();
        }
    }

    // Update calls PlayerListener function every frame.
    void Update()
    {
        PlayerListener();        
    }

    //comment!
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<KeyValues>() != null)
        {
            KeyValues key = collision.gameObject.GetComponent<KeyValues>();
            keyChain.Add(key.keyNumber);
            key.gameObject.SetActive(false);
            foreach (int a in keyChain)
            {
                Debug.Log(a + " " + keyChain.Count);
            }
        }        
    }

    //PlayerListener listens to keyboard inputs and depending on the input it calls other functions that different actions.
    void PlayerListener()
    {
        //If the player presses W, S, "up" or "down" arrow keys the character will try to move vertically.
        //At the same time the game will also check if the minotaur needs to move or the bomb has to explode.
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            CheckDirectionVertical(direction, RaycastDistance);
            ChechForMinotaur();
            CheckForBombs();
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            CheckDirectionVertical(-direction, RaycastDistance);
            ChechForMinotaur();
            CheckForBombs();
        }

        //Same as above, except here character tries to move horizontally.
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            CheckDirectionHorizontal(direction, RaycastDistance);
            ChechForMinotaur();
            CheckForBombs();
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            CheckDirectionHorizontal(-direction, RaycastDistance);
            ChechForMinotaur();
            CheckForBombs();
        }

        //When the player presses the Z key, the script tries to find a door near the tile the character is standing on.
        if (Input.GetKeyDown(KeyCode.Z))
        {
            SearchForDoor(RaycastDistance);
        }

        //If the player presses the X key, the script will look for a bomb on the character's current tile.
        //If there is a bomb on the current tile, the character will grab it and defuse it.
        //If there is no bomb on the tile and the character still has some bombs left in her inventory, the character will drop a bomb.
        if (Input.GetKeyDown(KeyCode.X))
        {
            bool bombWasHere = TryFetchBomb();
            if (bombWasHere == false && bombsAvailable > 0)
            {
                Instantiate(bombPrefab, new Vector3 (playerPosition.x, playerPosition.y, 0), Quaternion.identity);
                bombsAvailable -= 1;
                Debug.Log(bombsAvailable);
            }
            else if (bombWasHere == true)
            {
                Debug.Log("Bomb should be gone.");
                bombsAvailable += 1;
                Debug.Log(bombsAvailable);
            }
        }

        //If the player presses the C key, the game will search for all levers on the level. If the character is standing on a tile with a lever, she will pull the lever.
        if (Input.GetKeyDown(KeyCode.C))
        {
            foreach (LeverBehaviour leverScript in leverScripts)
            {
                GameObject lever = leverScript.gameObject;
                Vector2 leverPos = lever.transform.position;
                float distToLeverX = Mathf.Abs(leverPos.x - playerPosition.x);
                float didtToLeverY = Mathf.Abs(leverPos.y - playerPosition.y);
                if (didtToLeverY < 1f && distToLeverX < 1f)
                {
                    ToggleDoors();
                    ToggleLeverColors();
                }
            }
        }
    }

    //The following two methods could be merged into one. In both methods a direction is checked for obstacles - vertically in the first method and horizontally in the second one.
    //If there is an obstacle, the character stays on the same tile and the player is informed that there is an obstacle in the way. If there are no obstacles - the character moves forward. 
    void CheckDirectionVertical(float directionMult, float distance)
    {
        RaycastHit2D hit2D = Physics2D.Raycast(playerPosition, (Vector2.up * directionMult), distance, wallLayer);
        if (hit2D.collider != null)
        {
            Debug.Log("Object is in the way.");
            //If the obstacle was hidden from player until now, it is revealed to the player.
            ShowHidden(hit2D);
            IdentifyObject(hit2D);
        }
        else
        {
            MovementVertical(moveParamY * directionMult);
            //When the player moves to a new tile, all tiles and objects that are no longer adjacent to the player become hidden.
            HideAll();
            RecallSurroundings(RaycastDistance);
            CheckSurroundings(RaycastDistance);            
        }
    }

    void CheckDirectionHorizontal(float directionMult, float distance)
    {
        RaycastHit2D hit2D = Physics2D.Raycast(playerPosition, (Vector2.right * directionMult), distance, wallLayer);
        if (hit2D.collider != null)
        {
            Debug.Log("Object is in the way.");
            ShowHidden(hit2D);
            IdentifyObject(hit2D);
        }
        else
        {
            MovementHorizontal(moveParamX * directionMult);
            HideAll();
            RecallSurroundings(RaycastDistance);
            CheckSurroundings(RaycastDistance);            
        }
    }

    //The following two methods could also be compressed into one. In both methods the character is moved to a new tile.
    void MovementVertical(float directionY)
    {
        playerPosition = playerPosition + new Vector2(0f, directionY);
        gameObject.transform.position = playerPosition;
        //Debug.Log("Moved");
    }

    void MovementHorizontal(float directionX)
    {
        playerPosition = playerPosition + new Vector2(directionX, 0f);
        gameObject.transform.position = playerPosition;
        //Debug.Log("Moved");
    }

    //HideAll searches for all known objects that have a SpriteRenderer component (except the player character) and disables the SpriteRenderer component for all of them.
    //This method should not be in player script, but rather in a separate object script that would "monitor" all non-player objects in the scene.
    void HideAll()
    {
        foreach (DoorValues valD in doors)
        {
            GameObject doorObject = valD.gameObject;
            if (doorObject.GetComponent<SpriteRenderer>() != null)
            {
                SpriteRenderer doorSprite = doorObject.GetComponent<SpriteRenderer>();
                doorSprite.enabled = false;
            }            
        }
        foreach (KeyValues valK in keys)
        {
            GameObject keyObject = valK.gameObject;
            if (keyObject.GetComponent<SpriteRenderer>() != null)
            {
                SpriteRenderer keySprite = keyObject.GetComponent<SpriteRenderer>();
                keySprite.enabled = false;
            }
        }
        foreach (GameObject wall in walls)
        {
            if (wall.GetComponent<SpriteRenderer>() != null)
            {
                SpriteRenderer wallSprite = wall.GetComponent<SpriteRenderer>();
                wallSprite.enabled = false;
            }
        }
        foreach (LeverBehaviour levB in leverScripts)
        {
            if (levB.gameObject.GetComponent<SpriteRenderer>() != null)
            {
                SpriteRenderer leverRenderer = levB.gameObject.GetComponent<SpriteRenderer>();
                leverRenderer.enabled = false;
            }
        }
        if (minotaur.GetComponent<SpriteRenderer>() != null)
        {
            SpriteRenderer minoSprite = minotaur.GetComponent<SpriteRenderer>();
            minoSprite.enabled = false;
        }
    }

    //CheckSurroundings searches for walls and objects on the tile where the character currently stands. It searches for walls and objects by looking in the four cardinal direction.
    void CheckSurroundings(float distance)
    {
        for (int i = 0; i < 4; i++)
        {
            switch (i)
            {
                case 0:
                    DoRaycast2DShort(Vector2.up, distance, "upwards.");
                    break;

                case 1:
                    DoRaycast2DShort(Vector2.right, distance, "to the right.");
                    break;

                case 2:
                    DoRaycast2DShort(Vector2.down, distance, "downwards.");
                    break;

                case 3:
                    DoRaycast2DShort(Vector2.left, distance, "to the left.");
                    break;

            }
        }        
    }

    //DoRaycast2DShort shoots two short rays in the same direction, but in two different layers: the layer for walls and the layer for objects. If there is an object or a wall in the direction of a ray, the object is then revealed to the player.
    void DoRaycast2DShort(Vector2 vector, float distance, string keyWords)
    {
        RaycastHit2D hit2Da = Physics2D.Raycast(playerPosition, vector, distance, wallLayer);
        if (hit2Da.collider != null)
        {
            //Debug.Log("Found something " + keyWords);
            ShowHidden(hit2Da);
            IdentifyObject(hit2Da);
        }
        RaycastHit2D hit2Db = Physics2D.Raycast(playerPosition, vector, distance, objectsLayer);
        if (hit2Db.collider != null && hit2Da.collider == null)
        {
            //Debug.Log("Found something else!");
            ShowHidden(hit2Db);
            IdentifyObject(hit2Db);
        }
    }
    
    //IdentifyObject reads a tag of a revealed object or a wall and then remembers the position of that object.
    void IdentifyObject(RaycastHit2D hitObj)
    {
        var hitGameObject = hitObj.transform.gameObject;
        if (hitGameObject.tag == WallTagUnexplored)
        {
            MemorizeWallPosition(hitGameObject);
        }
        else if (hitGameObject.tag == "Door" || hitGameObject.tag == "Key" || hitGameObject.tag == "Lever")
        {
            MemorizeObjectPosition(hitGameObject);
        }
    }

    //ShowHidden displays the hidden object or wall to the player by re-enabling the SpriteRenderer component of this object or wall.
    //This method should also be in a script of an object that would "monitor" all non-player objects in the scene.
    void ShowHidden(RaycastHit2D hitObj)
    {
        if (hitObj.transform != null)
        {
            if (hitObj.transform.GetComponent<SpriteRenderer>() != null)
            {
                var hitSprite = hitObj.transform.GetComponent<SpriteRenderer>();
                hitSprite.enabled = true;
            }
        }
    }

    //MemorizeWallPostion only changes tags of the discovered wall.
    //This method also belongs in a separate object script that would perform "monitoring".
    void MemorizeWallPosition(GameObject hitWall)
    {
        hitWall.tag = WallTagExplored;
    }

    //MemorizeObjectPosition checks which object was discovered and sets "wasExplored" value in its respective value script to "true".
    //This method also belongs in a separate object script that would perform "monitoring".
    void MemorizeObjectPosition(GameObject hitObject)
    {
        if (hitObject.GetComponent<DoorValues>() != null)
        {
            var doorVals = hitObject.GetComponent<DoorValues>();
            doorVals.wasExplored = true;
        }
        else if (hitObject.GetComponent<KeyValues>() != null)
        {
            var keyVals = hitObject.GetComponent<KeyValues>();
            keyVals.wasExplored = true;
        }
        else if (hitObject.GetComponent<LeverBehaviour>() != null)
        {
            var leverBehav = hitObject.GetComponent<LeverBehaviour>();
            leverBehav.wasExplored = true;
        }
    }

    //RecallSurroundings method looks into twelve directions (30 degrees apart) and checks if the character "remembers" what the tiles adjacent to her look like.
    void RecallSurroundings(float distance)
    {
        float longDistance = distance * DistMultiplier;
        for (int i = 0; i < 12; i++)
        {
            switch (i)
            {
                //To make sure that the player can only see the walls and objects of the adjacent eight tiles and not further, the distances for checking in different directions vary.
                case 0:
                    DoRaycast2DLong(Vector2.up, (distance + Margin));
                    break;

                case 1:
                    DoRaycast2DLong(new Vector2(1f, 2f), longDistance);
                    break;

                case 2:
                    DoRaycast2DLong(new Vector2(2f, 1f), longDistance);
                    break;

                case 3:
                    DoRaycast2DLong(Vector2.right, (distance + Margin));
                    break;

                case 4:
                    DoRaycast2DLong(new Vector2(2f, -1f), longDistance);
                    break;

                case 5:
                    DoRaycast2DLong(new Vector2(1f, -2f), longDistance);
                    break;

                case 6:
                    DoRaycast2DLong(Vector2.down, (distance + Margin));
                    break;

                case 7:
                    DoRaycast2DLong(new Vector2(-1f, -2f), longDistance);
                    break;

                case 8:
                    DoRaycast2DLong(new Vector2(-2f, -1f), longDistance);
                    break;

                case 9:
                    DoRaycast2DLong(Vector2.left, (distance + Margin));
                    break;

                case 10:
                    DoRaycast2DLong(new Vector2(-2f, 1f), longDistance);
                    break;

                case 11:
                    DoRaycast2DLong(new Vector2(-1f, 2f), longDistance);
                    break;

            }
        }
    }

    //DoRaycast2DLong makes two raycats in the same direction, but on two different layers: layer for walls and layer for objects. However, in this method, the raycast distance is longer than in DoRaycast2DShort and the raycast goes through all walls and objects.
    //The method then checks whether the hit objects or walls were previously discovered by the player. If yes, then those objects and walls are revealed to the player once again.
    void DoRaycast2DLong(Vector2 vector, float distanceLong)
    {
        RaycastHit2D[] hit2DWalls = Physics2D.RaycastAll(playerPosition, vector, distanceLong, wallLayer);
        //Debug.Log(hit2DWalls.Length);
        foreach (RaycastHit2D hit2DWall in hit2DWalls)
        {
            if (hit2DWall.collider != null)
            {
                GameObject hitGameObject = hit2DWall.transform.gameObject;
                if (hitGameObject.tag == WallTagExplored)
                {
                    //Debug.Log(vector);
                    //Debug.Log("Remember wall: " + hitGameObject.ToString());
                    ShowHidden(hit2DWall);
                }
                else if (hitGameObject.tag == "Door")
                {
                    bool wasExplored = AccessExploreValue(hitGameObject, hitGameObject.tag);
                    if (wasExplored == true)
                    {
                        //Debug.Log(vector);
                        //Debug.Log("Remember object " + hitGameObject.ToString());
                        ShowHidden(hit2DWall);
                    }
                }
            }
        }

        RaycastHit2D[] hit2DObjects = Physics2D.RaycastAll(playerPosition, vector, distanceLong, objectsLayer);
        //Debug.Log(hit2DObjects.Length);
        foreach (RaycastHit2D hit2DObject in hit2DObjects)
        {
            if (hit2DObject.collider != null)
            {
                GameObject hitGameObject = hit2DObject.transform.gameObject;
                if (hitGameObject.tag == "Key" || hitGameObject.tag == "Lever")
                {
                    bool wasExplored = AccessExploreValue(hitGameObject, hitGameObject.tag);
                    if (wasExplored == true)
                    {
                        //Debug.Log(vector);
                        //Debug.Log("Remember object " + hitGameObject.ToString());
                        ShowHidden(hit2DObject);
                    }
                }
            }
        }
    }

    //AccessExploreValue reads the tag of the object found through the raycast, accesses its value script and then returns the "wasExplored" value stored in that script.
    //This method also belongs in a separate object script that would perform "monitoring".
    bool AccessExploreValue(GameObject objectFound, string tag)
    {
        if (tag == "Door")
        {
            DoorValues valsD = objectFound.GetComponent<DoorValues>();
            return valsD.wasExplored;
        }
        else if (tag == "Key")
        {
            KeyValues valsK = objectFound.GetComponent<KeyValues>();
            return valsK.wasExplored;
        }
        else
        {
            LeverBehaviour levBehav = objectFound.GetComponent<LeverBehaviour>();
            return levBehav.wasExplored;
        }
    }

    //SearchForDoor tries to find a door on a tile where the character currently stands by looking in the four cardinal directions.
    void SearchForDoor(float distance)
    {
        for (int j = 0; j < 4; j++)
        {
            switch (j)
            {
                case 0:
                    CheckForDoor(Vector2.up, distance);
                    break;

                case 1:
                    CheckForDoor(Vector2.right, distance);
                    break;

                case 2:
                    CheckForDoor(Vector2.down, distance);
                    break;

                case 3:
                    CheckForDoor(Vector2.left, distance);
                    break;
            }
        }
    }

    //CheckForDoor makes one raycast in the set direction in the walls layer. It then checks if the object that was hit is an exit door. If yes, then it checks the character's keychain.
    void CheckForDoor(Vector2 vector, float distance)
    {
        RaycastHit2D hit2Dc = Physics2D.Raycast(playerPosition, vector, distance, wallLayer);
        if (hit2Dc.collider != null)
        {
            GameObject hitObject = hit2Dc.collider.gameObject;
            if (hitObject.CompareTag("Door") == true && hitObject.GetComponent<DoorValues>() != null)
            {
                DoorValues doorScript = hitObject.GetComponent<DoorValues>();
                //Debug.Log("Found a door.");
                CheckForKeychainExit(doorScript);
            }
        }
    }

    //CheckForKeychainExit counts the amount of exit keys the character has in her keychain. If the charcter has all necessary keys, the exit door is unlocked. Otherwise, the door remains closed and the player is informed that some keys are missing.
    void CheckForKeychainExit(DoorValues exitVals)
    {
        int k = 0;
        foreach (int keyNumber in keyChain)
        { 
            if (keyNumber == exitVals.doorNumber)
            {
                k++;
            }
        }
        if (k == exitVals.keysRequired)
        {
            Debug.Log("Player has enough keys for this door.");
            exitVals.UnlockDoor();
        }
        else
        {
            AnnounceNotEnoughKeys(exitVals.keysRequired - k);
            Debug.Log("Player has not enough keys for this door.");
        }
    }

    //AnnounceNotEnoughKeys checks the amount of exit keys missing, then displays the announcement for the player and the starts the coroutine to wait for waitTime seconds before hiding the announcement.
    void AnnounceNotEnoughKeys(int keysMissing)
    {
        if (keysMissing > 1)
        {
            annText.text = "Missing " + keysMissing + " keys.";
        }
        else if (keysMissing == 1)
        {
            annText.text = "Missing " + keysMissing + " key.";
        }
        annText.enabled = true;
        annPanel.GetComponent<Image>().enabled = true;
        annChild.GetComponent<Image>().enabled = true;
        StartCoroutine(WaitBeforeHide(waitTime));
    }

    //WaitBeforeHide waits for waitSecs before hiding the announcement.
    public IEnumerator WaitBeforeHide(float waitSecs)
    {
        yield return new WaitForSeconds(waitSecs);
        HideAnnounce();
    }

    //HideAnnounce removes text from the announcement panel and then hides it.
    void HideAnnounce()
    {
        annText.text = "";
        annText.enabled = false;
        annPanel.GetComponent<Image>().enabled = false;
        annChild.GetComponent<Image>().enabled = false;
    }

    //CheckForBombs searches for all bombs within the scene and then ticks the timer for each bomb.
    //This method also belongs in a separate object script that would perform "monitoring".
    void CheckForBombs()
    {
        GameObject[] bombs = GameObject.FindGameObjectsWithTag("Bomb");
        if (bombs.Length != 0)
        {
            foreach (GameObject bomb in bombs)
            {
                BombTimerTick(bomb);
            }
        }
    }

    //TryFetchBomb searches for all bombs within the scene and then checks if any bomb is on the same tile as the player character. If there is a bomb on the same tile as the player charcter, the method returns true.
    //If there are no bombs in the scene or there are no bombs on the player character's tile, the method returns false.
    //This method also belongs in a separate object script that would perform "monitoring".
    bool TryFetchBomb()
    {
        GameObject[] bombs = GameObject.FindGameObjectsWithTag("Bomb");
        if (bombs.Length != 0)
        {
            int j = 0;
            foreach (GameObject bomb in bombs)
            {
                Vector2 bombPos = bomb.transform.position;
                //The line below translates to "if the distance between the bomb's position and the charcter's position is less than half a tile - then do this".
                if (Mathf.Abs(bombPos.x - playerPosition.x) < 0.6f && Mathf.Abs(bombPos.y - playerPosition.y) < (moveParamX / 2))
                {
                    Destroy(bomb);
                    return true;
                }
                else
                {
                    j++;
                }
            }
            if (bombs.Length == j)
            {
                return false;
            }
            else
            {
                Debug.Log("Couldn't destroy bomb.");
                return true;
            }
        }
        else
        {
            return false;
        }
    }

    //BombTimerTick tries to fetch the BombValuesNActions component in the set bomb and then ticks over the amount of turns this bomb has been active.
    //This method also belongs in a separate object script that would perform "monitoring".
    void BombTimerTick(GameObject bombObject)
    {
        if (bombObject.GetComponent<BombValuesNActions>() != null)
        {
            BombValuesNActions bombVals = bombObject.GetComponent<BombValuesNActions>();
            bombVals.turnsPassed += 1;
        }
        else
        {
            Debug.Log("Error. Bomb " + bombObject.ToString() + " has no BombValuesNActions script.");
        }
    }

    //CheckForMinotaur check whether the minotaur is awake. If yes, then the minotaur makes a move.
    //This belongs in the a minotaur script.
    void ChechForMinotaur()
    {
        if (minotaurScript.minoIsAwake == true)
        {
            minotaurScript.SearchForPaths(minotaurScript.distanceMult);
        }
    }

    //ToggleDoors accesses every door that is not an exit and switches its state between open and closed.
    //This method also belongs in a separate object script that would perform "monitoring".
    public void ToggleDoors()
    {
        foreach (DoorValues doorScript in doors)
        {
            if (doorScript.isExit == false)
            {
                if (doorScript.isOpen == true)
                {
                    doorScript.LockDoor();
                }
                else
                {
                    doorScript.UnlockDoor();
                }
            }
        }
    }

    //ToggleLeverColors accesses every lever in the scene and then tries to get its SpriteRenderer component. Then the method toggles the color of each lever depending on the new state of the levers.
    //This method belongs in LeverBehaviour.
    public void ToggleLeverColors()
    {
        foreach (LeverBehaviour leverBehScript in leverScripts)
        {
            if (leverBehScript.gameObject.GetComponent<SpriteRenderer>() != null)
            {
                SpriteRenderer leverRenderer = leverBehScript.gameObject.GetComponent<SpriteRenderer>();
                if (leverRenderer.color != leverDownColor)
                {
                    leverRenderer.color = leverDownColor;
                }
                else if (leverRenderer.color != leverUpColor)
                {
                    leverRenderer.color = leverUpColor;
                }
            }
        }
        ToggleIndicator();
    }

    //ToggleIndicator changes the colour of the indicator in the UI depending on the new state of the levers.
    //This method also belongs in a separate object script.
    public void ToggleIndicator()
    {
        if (leverIndicator.color != leverDownColor)
        {
            leverIndicator.color = leverDownColor;
        }
        else if (leverIndicator.color != leverUpColor)
        {
            leverIndicator.color = leverUpColor;
        }
    }
}
