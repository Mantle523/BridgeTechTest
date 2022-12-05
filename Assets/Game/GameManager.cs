using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] GameObject playerObject;
    
    [SerializeField] GameObject collectablePrefab;
    [SerializeField] GameObject obstaclePrefab;

    [SerializeField] GameObject gameOverScreen;

    int gridSize = 5;
    bool[,] grid; //a record of *roughly* where in the space objects have been placed

    [SerializeField] float spawnPeriod = 3f;
    float timeSinceLastSpawn = 0f;

    [SerializeField] float obstaclePeriod = 10f;
    float timeSinceLastObs = 0f;

    [SerializeField] float collectTimeLimit;
    float timeSinceLastCollect = 0f;

    [Space]
    [Header("Scoring")]

    int currentScore = 0;

    [SerializeField] int[] capsuleScoring = new int[] { 2, 12, 22 };
    [SerializeField] int[] sphereScoring = new int[] { 1, 10, 20 };

    [SerializeField] int[] levelThresholds = new int[] { 100, 200, 300 };

    [SerializeField] Text scoreText;
    [SerializeField] Text levelText;

    bool hasCollectedAnyObject = false;
    bool lastCollectedWasCapsule = false;

    int totalCollected = 0;
    //ok so the specified points to win implies the existance of levels beyond 3,
    //but since there are no scores listed for further levels, I'll end it at 300 points
    
    // Start is called before the first frame update
    void Start()
    {
        grid = new bool[gridSize, gridSize];
        UpdateScore(0);
        gameOverScreen.SetActive(false);

        SpawnCollectable(true, new Vector2Int(2,3));
    }

    // Update is called once per frame
    void Update()
    {
        //WorldSpaceToGridSpace(playerObject.transform.position);
        
        float time = Time.time;
        
        if (time > timeSinceLastSpawn + spawnPeriod)
        {
            timeSinceLastSpawn = time;

            SpawnCollectable();
        }

        if (time > timeSinceLastObs + obstaclePeriod)
        {
            timeSinceLastObs = time;
            SpawnObstacle();
        }

        if (time > timeSinceLastCollect + collectTimeLimit)
        {
            EndGame(false);
        }
    }

    Collectable SpawnCollectable(bool overridePosition = false, Vector2Int positionOverride = default)
    {
        //Spawncollectable in free space
        //get free space
        Vector2Int space;

        if (overridePosition && positionOverride.x <= gridSize && positionOverride.y <= gridSize) //we have asked for a specific position (and that position fits)
        {
            space = positionOverride;
        }
        else
        {
            space = FindFreeSpaceOnGrid();
        }

        if (space == Vector2Int.one * -1)
        {
            return null; //No space available 
        }

        //update the grid array to show that this space will now be occupied
        grid[space.x, space.y] = true;

        GameObject spawn = (GameObject)Instantiate(collectablePrefab, GridSpaceToWorldSpace(space), Quaternion.identity);

        Collectable collectable = spawn.GetComponent<Collectable>();
        if (collectable == null)
        {
            Debug.Log("collectable prefab was instanciated without component");
        }

        collectable.OnCollect.AddListener(OnPlayerCollected);
        collectable.gridPosition = space;
        collectable.SetState(Random.Range(0,2) == 1); //50/50 chance for capsule or sphere

        return collectable;
    }

    void SpawnObstacle()
    {
        Vector2Int space = FindFreeSpaceOnGrid();

        if (space == Vector2Int.one * -1)
        {
            return; //no space available
        }

        grid[space.x, space.y] = true;
        
        Instantiate(obstaclePrefab, GridSpaceToWorldSpace(space), Quaternion.identity);
    }

    Vector2Int FindFreeSpaceOnGrid()
    {
        //Get the grid square the player is closest to (to prevent spawning object on top of them)
        Vector2Int PlayerPos = WorldSpaceToGridSpace(playerObject.transform.position);
        
        //Get all the free spaces on the grid into a list
        List<Vector2Int> freeSpaces = new List<Vector2Int>();
        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                if (grid[i,j] == false && !(i == PlayerPos.x && j == PlayerPos.y))
                {
                    freeSpaces.Add(new Vector2Int(i,j));
                }
            }
        }

        int index = Random.Range(0, freeSpaces.Count -1);

        //Randomly choose one of those spaces
        Vector2Int selection = Vector2Int.one * -1;
        if (index < freeSpaces.Count)
        {
            selection = freeSpaces[index];
        }
        //Debug.Log("Randomly selected " + index + " out of " + (freeSpaces.Count-1));

        Debug.Log(selection);
        
        return selection;
    }

    Vector3 GridSpaceToWorldSpace(Vector2Int gridSpace)
    {
        //Takes the x and y and works out where in the world each point needs to be.

        float _x = gridSpace.x - (Mathf.Floor((gridSize - 1) / 2));
        if (gridSize % 2 == 0)
        {
            _x -= 0.5f;
        }

        float _y = gridSpace.y - (Mathf.Floor((gridSize - 1) / 2));
        if (gridSize % 2 == 0)
        {
            _y -= 0.5f;
        }

        Vector3 unscaledVector = new Vector3(_x += transform.position.x, transform.position.y, (_y) += transform.position.z);

        return unscaledVector * 10;
    }

    Vector2Int WorldSpaceToGridSpace(Vector3 worldSpace)
    {
        //Undo the scaling
        Vector3 unscaledVector = worldSpace / 10;
        //Undo the transformation of the grid's transform, and the transformation that moves the center of the grid to the objects origin
        float offset = Mathf.Floor((gridSize - 1) / 2);
        if (gridSize % 2 == 0)
        {
            offset += 0.5f;
        }

        Vector3 untransformedVector = new Vector3(unscaledVector.x -= (transform.position.x - offset), transform.position.y, unscaledVector.z -= (transform.position.z - offset));

        Vector2Int result = new Vector2Int(Mathf.RoundToInt(untransformedVector.x), Mathf.RoundToInt(untransformedVector.z));

        //Debug.Log(result);
        
        return result;
    }

    int GetCurrentLevel()
    {
        //Work out which level the player is at based on their current score and the level thresholds
        // 0 = lvl1
        // 1 = lvl2
        // 2 = lvl3
        // 3 = Game Over

        for (int i = 0; i < 3; i++)
        {
            if (currentScore < levelThresholds[i])
            {
                return i;
            }
        }
        
        return 3;
    }

    void OnPlayerCollected(Vector2Int pos, bool capsule)
    {
        if (!grid[pos.x, pos.y])
        {
            Debug.Log("This space should be empty!");
        }

        totalCollected++;

        grid[pos.x, pos.y] = false;

        //Increment score etc...
        int value;

        if (capsule)
        {
            value = capsuleScoring[GetCurrentLevel()];
        }
        else
        {
            value = sphereScoring[GetCurrentLevel()];
        }

        if (hasCollectedAnyObject)
        {
            if (capsule == lastCollectedWasCapsule) //collected object was same type as last collected
            {
                value *= -2;
            }
        }
        else
        {
            hasCollectedAnyObject = true;
        }

        lastCollectedWasCapsule = capsule;
        timeSinceLastCollect = Time.time;

        UpdateScore(value);
    }

    void UpdateScore(int delta)
    {
        int level = GetCurrentLevel();
        
        currentScore += delta;
        scoreText.text = "Score: " + currentScore;
        levelText.text = "Level: " + (level + 1);

        playerObject.transform.localScale = new Vector3(1 + (level / 10), 1, 1 + (level / 10));

        if (level >= 3)
        {
            EndGame(true);
        }
    }

    void EndGame(bool win)
    {
        //Disable Input - this is called exaclty once, so I'm not worried about it not being cached anywhere
        PlayerMovement movement = playerObject.GetComponent<PlayerMovement>();
        movement.allowInput = false;

        GameResult _result = new GameResult(Time.time, currentScore, totalCollected);
        string result = JsonUtility.ToJson(_result);

        System.IO.File.WriteAllText(Application.persistentDataPath + "/GameResult.json", result);

        //Display button to close game
        gameOverScreen.SetActive(true);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
