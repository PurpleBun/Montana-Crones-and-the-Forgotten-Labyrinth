using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This script is attached to the game's antagonist - the minotaur. It handles most of theminotaur's actions.
public class MinotaurMovement : MonoBehaviour
{
    private Vector2 minotaurPosition;
    private GameObject minotaur;
    [SerializeField]
    private LayerMask wallLayer, objectLayer, oldWallLayer;
    //[SerializeField]
    //private float raycastDistance;
    private bool obstacleFound = true;
    private List<string> directionsToGo;
    public float distanceMult;
    private string previousPosition;
    public bool minoIsAwake = false;
    private PlayerActionScript playerScript;
    private GameObject player;
    private Vector2 playerPosition;

    //This class lacks a constructor. Instead of setting variables in the class, most variables should be set in a constructor.

    //Start fetches and sets all necessary variables. 
    void Start()
    {
        minotaur = this.gameObject;
        minotaurPosition = minotaur.transform.position;
        directionsToGo = new List<string>();
        playerScript = FindObjectOfType<PlayerActionScript>();
        player = playerScript.gameObject;
    }

    //Update checks every frame whether the minotaur stands on the same tile as the player character.
    //This could be improved by moving this check to the end of the player's turn instead of in the Update method.
    void Update()
    {
        if (minoIsAwake == true)
        {
            playerPosition = player.transform.position;
            if (minotaurPosition == playerPosition)
            {
                player.SetActive(false);
            }
        }
    }

    //SearchForPaths first looks for obstacles in the four cardinal directions and then adds any unobstructed paths to directionsToGo.
    //Afterwards the method makes the minotaur choose which way to go based on available paths and clears the list of old available paths.
    public void SearchForPaths(float distance)
    {        
        for (int i = 0; i < 4; i++)
        {
            switch (i)
            {
                case 0:
                    obstacleFound = Raycast2DFindObstacle(Vector2.up, distance);
                    if (obstacleFound == false)
                    {
                        directionsToGo.Add("up");
                    }
                    break;

                case 1:
                    obstacleFound = Raycast2DFindObstacle(Vector2.right, distance);
                    if (obstacleFound == false)
                    {
                        directionsToGo.Add("right");
                    }
                    break;

                case 2:
                    obstacleFound = Raycast2DFindObstacle(Vector2.down, distance);
                    if (obstacleFound == false)
                    {
                        directionsToGo.Add("down");
                    }
                    break;

                case 3:
                    obstacleFound = Raycast2DFindObstacle(Vector2.left, distance);
                    if (obstacleFound == false)
                    {
                        directionsToGo.Add("left");
                    }
                    break;
            }
        }
        ChoosePath(directionsToGo, distance);        
        directionsToGo.Clear();
    }

    //Raycast2DFindObstacle checks whether there is supposed to be a wall in the vector direction. If there is a wall (visible or not) in the direction of the raycast, the method returns true. Otherwise, it returns false.
    bool Raycast2DFindObstacle(Vector2 vector, float rayDistance)
    {
        RaycastHit2D hit2Da = Physics2D.Raycast(minotaurPosition, vector, rayDistance, wallLayer);
        //RaycastHit2D[] hit2Db = Physics2D.RaycastAll(minotaurPosition, vector, rayDistance, objectLayer);
        RaycastHit2D hit2Db = Physics2D.Raycast(minotaurPosition, vector, rayDistance, oldWallLayer);
        
        if (hit2Da.collider != null || hit2Db.collider != null)
        {
            return true;
        }
        else
        {
            return false;
        }

    }

    //ChoosePath first checks whether the minotaur reached a dead end or not. If yes, the minotaur goes back to its previous position.
    //If the minotaur can walk more than one way, it will walk in a random direction.
    //If it somehow cannot move anywhere, the method logs an error.
    void ChoosePath(List<string> path, float distance)
    {
        if (path.Count == 1)
        {
            if (path[0] == "up")
            {
                //Debug.Log("Minotaur goes " + path[0] + ".");
                MinoMovementVert(distance);
            }
            else if (path[0] == "right")
            {
               // Debug.Log("Minotaur goes " + path[0] + ".");
                MinoMovementHoriz(distance);
            }
            else if (path[0] == "left")
            {
                //Debug.Log("Minotaur goes " + path[0] + ".");
                MinoMovementHoriz(-distance);
            }
            else if (path[0] == "down")
            {
                //Debug.Log("Minotaur goes " + path[0] + ".");
                MinoMovementVert(-distance);
            }
        }
        else if (path.Count > 1)
        {
            int randomIndex = Random.Range(0, path.Count);

            while (path[randomIndex] == previousPosition)
            {
                randomIndex = Random.Range(0, path.Count);
            }

            if (path[randomIndex] == "up")
            {
                //Debug.Log("Minotaur goes " + path[0] + ".");
                MinoMovementVert(distance);
            }
            else if (path[randomIndex] == "right")
            {
                //Debug.Log("Minotaur goes " + path[0] + ".");
                MinoMovementHoriz(distance);
            }
            else if (path[randomIndex] == "left")
            {
                //Debug.Log("Minotaur goes " + path[0] + ".");
                MinoMovementHoriz(-distance);
            }
            else if (path[randomIndex] == "down")
            {
                //Debug.Log("Minotaur goes " + path[0] + ".");
                MinoMovementVert(-distance);
            }
        }
        else
        {
            Debug.Log("Minotaur couldn't find a path.");
        }
    }

    //The following two methods perform the same tasks, but in different directions.
    //These methods move the minotaur onto a new tile and then remember which direction the minotaur came from.
    //They could most likely could be merged into one, given more time.
    void MinoMovementVert(float direction)
    {
        minotaurPosition += new Vector2(0f, direction);
        gameObject.transform.position = minotaurPosition;
        if (direction > 0)
        {
            previousPosition = "down";
        }
        else if (direction < 0)
        {
            previousPosition = "up";
        }
    }

    void MinoMovementHoriz(float direction)
    {
        minotaurPosition += new Vector2(direction, 0f);
        gameObject.transform.position = minotaurPosition;
        if (direction > 0)
        {
            previousPosition = "left";
        }
        else if (direction < 0)
        {
            previousPosition = "right";
        }
    }
    
}
