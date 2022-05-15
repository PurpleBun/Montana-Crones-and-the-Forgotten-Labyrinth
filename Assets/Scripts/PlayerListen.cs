using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

//This script is attached to an invisible game object that "observes" the state of the scene. It handles the UI of the game and the announcements that the player can see.
public class PlayerListen : MonoBehaviour
{
    private PlayerActionScript playerScript;
    private GameObject player;
    private Scene thisScene;
    private GameObject[] doors;
    private GameObject exitDoor;
    private DoorValues doorVals;
    [SerializeField]
    private float waitTimer;
    [SerializeField]
    private int desiredDoorLayer;
    private bool announcementMade = false;
    private GameObject[] inventorySlots;
    [SerializeField]
    private GameObject exitKey;
    private SpriteRenderer exitKeyRend;
    [SerializeField]
    private Text bombCounter, loseWinState;
    [SerializeField]
    private GameObject loseWinPanel, panelChild;


    //Start grabs all necessary objects. Most importantly it finds and stores the active scene and the player object.
    void Start()
    {
        if (exitKey.GetComponent<SpriteRenderer>() != null)
        {
            exitKeyRend = exitKey.GetComponent<SpriteRenderer>();
        }
        inventorySlots = GameObject.FindGameObjectsWithTag("InventorySlot");        
        playerScript = FindObjectOfType<PlayerActionScript>();
        player = playerScript.gameObject;
        thisScene = SceneManager.GetActiveScene();
        FindExitDoor();        
    }

    //Update updates the bomb counter in the UI every frame. This method also handles the announcements of victory, defeat and the minotaur waking up. Additionally, if the player has any keys in the keychain, then the inventory UI is updated to show that.
    void Update()
    {
        bombCounter.text = playerScript.bombsAvailable.ToString();
        if (player.activeSelf == false && announcementMade == false)
        {
            announcementMade = true;
            StartCoroutine(WaitBeforeReloadDefeat(waitTimer)); //start a timer before the scene restarts
        }
        else if (exitDoor.layer == desiredDoorLayer && announcementMade == false)
        {
            announcementMade = true;
            StartCoroutine(WaitBeforeReloadVictory(waitTimer));
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        if (playerScript.keyChain.Count > 0 && playerScript.keyChain.Count <= inventorySlots.Length)
        {
            UpdateInventory();
        }
        else if (playerScript.keyChain.Count > 0 && playerScript.keyChain.Count > inventorySlots.Length)
        {
            Debug.Log("Not enough inventory slots.");
        }

        if (loseWinState.enabled == true && loseWinState.text == "Minotaur is awake.")
        {
            StartCoroutine(WaitShort(waitTimer * 2f));
        }
    }

    //FindExitDoor first finds every door in the scene and then checks which door is the exit.
    void FindExitDoor()
    {
        doors = GameObject.FindGameObjectsWithTag("Door");
        foreach (GameObject door in doors)
        {
            if (door.GetComponent<DoorValues>() != null)
            {
                doorVals = door.GetComponent<DoorValues>();
                if (doorVals.isExit == true)
                {
                    exitDoor = door;
                }
            }
        }
    }


    //With more development time, the following two methods would be refactored into one.
    //Both methods display the panel with win/lose announcement for the player and then waits for waitSecs amount of seconds before reloading the game scene.
    public IEnumerator WaitBeforeReloadDefeat(float waitSecs)
    {
        Debug.Log("You lost.");
        loseWinState.enabled = true;
        loseWinState.text = "You lost.";
        if (loseWinPanel.GetComponent<Image>().enabled == false)
        {
            loseWinPanel.GetComponent<Image>().enabled = true;
        }
        if (panelChild.GetComponent<Image>().enabled == false)
        { 
            panelChild.GetComponent<Image>().enabled = true;
        }
        yield return new WaitForSeconds(waitSecs);
        SceneManager.LoadScene(thisScene.name);
    }

    public IEnumerator WaitBeforeReloadVictory(float waitSecs)
    {
        Debug.Log("You won!");
        loseWinState.enabled = true;
        loseWinState.text = "You won!";
        if (loseWinPanel.GetComponent<Image>().enabled == false)
        {
            loseWinPanel.GetComponent<Image>().enabled = true;
        }
        if (panelChild.GetComponent<Image>().enabled == false)
        {
            panelChild.GetComponent<Image>().enabled = true;
        }
        yield return new WaitForSeconds(waitSecs);
        SceneManager.LoadScene(thisScene.name);
    }

    //UpdateInventory counts the amount of keys that the character has in her keychain and then displays the same amount of keys in the inventory UI for the player.
    void UpdateInventory()
    {
        DoorValues exitDoorVals = exitDoor.GetComponent<DoorValues>();
        for (int i = 0; i < playerScript.keyChain.Count; i++)
        {
            if (playerScript.keyChain[i] == exitDoorVals.doorNumber)
            {
                if (inventorySlots[i].GetComponent<Image>() != null)
                {
                    Image invSlotImg = inventorySlots[i].GetComponent<Image>();
                    if (invSlotImg.enabled == false)
                    {
                        invSlotImg.enabled = true;
                    }
                }
            }
        }
    }

    //WaitShort waits for waitSecs amount of seconds before hiding the announcement for the player.
    public IEnumerator WaitShort(float waitSecs)
    {
        yield return new WaitForSeconds(waitSecs);
        HideAnnouncement();
    }

    //HideAnnouncement clears the announcement text from the panel and then disables the panel Image component.
    void HideAnnouncement()
    {
        loseWinState.enabled = false;
        loseWinState.text = "";
        loseWinPanel.GetComponent<Image>().enabled = false;
        panelChild.GetComponent<Image>().enabled = false;
    }
}
