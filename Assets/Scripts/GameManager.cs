using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    // Class for managing the UI
    public UIManager ui;

    // Defining a class to hold different types of objects:
    public ObjectPool multiObjectPool;

    // Our player
    public Player player;




    private float screenWidth;
    public float initial_spawn_height;

    // Current player score
    public float score = 0f;

    // Variables for determining map generation
    public float elapsed_time = 0;
    public float wave_index = 0;
    public float spawn_interval;

    // Current open path space
    public int curr_path_ind = 3; // Starts at 3 (middle)
    // Previous open path space
    public int prev_path_ind = 3;

    // Constants for representation of directions
    const int NULL_DIR = 3;
    const int FORWARD = 0;
    const int LEFT = 1;
    const int RIGHT = 2;    

    // Start is called before the first frame update
    void Start()
    {
        

        // Start the game paused
        Time.timeScale = 0;
        
        // Get object pool from unity.
        multiObjectPool = FindObjectOfType<ObjectPool>();
        // Get player from unity.
        player = FindObjectOfType<Player>();

        screenWidth = Camera.main.orthographicSize * Camera.main.aspect * 2;
        initial_spawn_height = multiObjectPool.spawnHeight;

        // ADDING CALLBACKS FOR COLLISION BEHAVIOR ------------------------------------------------
        multiObjectPool.AddCollisionCallback(multiObjectPool.COLLISION_BLOCK_PREFAB, onDeathCollision);
        multiObjectPool.AddCollisionCallback(multiObjectPool.WATER_TILE_PREFAB, onRiverCollision);
        multiObjectPool.AddCollisionCallback(multiObjectPool.JUMP_TILE_PREFAB, onJumpTileCollision);
        multiObjectPool.AddCollisionCallback(multiObjectPool.BOOSTER_SMALL_PREFAB, onSmallBoostCollision);
        multiObjectPool.AddCollisionCallback(multiObjectPool.BOOSTER_INVULNERABLE_PREFAB, onInvulnerableBoostCollision);
        // -----------------------------------------------------------------------------------------

        // Determine spawning interval of rectangles:
        // We want to another rectangle to spawn in the time it takes the rectangle to travel
        // the distance of the height of the rectangle.
        float rectHeight = multiObjectPool.GetPrefabSize(multiObjectPool.COLLISION_BLOCK_PREFAB).y;
        Debug.Log($"Rectangle height is {rectHeight}");
        float speed = 5;
        spawn_interval = (rectHeight / speed) - 0.01f;

        SpawnInitialWaves();

            

    }

    // Update is called once per frame
    void Update()
    {

        score += Time.deltaTime * 5; 
        ui.UpdateScore(Mathf.FloorToInt(score));

        CheckCollisions();

        elapsed_time += Time.deltaTime;

        if (elapsed_time > spawn_interval)
        {

            GenerateNextRow();
            elapsed_time = 0;

        }

    }

    void GenerateNextRow()
    {

        ++wave_index;
        // Spawn a river every ~30 blocks

        if (wave_index == 30)
        {
            SpawnRiverArea();
            wave_index = 0;
            return;
        } else if (wave_index > 30 && wave_index < 34)
        {

            return;
        } else if (wave_index >= 34)
        {

            wave_index = 0;
        }

        // Otherwise just spawn the next path
        else
        {
            SpawnRectangleGroup();

        }

        // Path addons:

        if (wave_index == 29)
        {
            SpawnJumpTile();


        }


        // Boosters, spawn halfway through wave.
        if (wave_index == 15)
        {
            // Equal probability of each booster.
            SpawnBooster(multiObjectPool.boosterList[UnityEngine.Random.Range(0, multiObjectPool.boosterList.Count)]);

        }


    }

    // Returns a integer representing the block that the pathfinding must move to in the next sequence.
    public int get_next_square()
    {
        // Start on the current square
        int next_square = curr_path_ind;

        // Conditions of previous move
        bool prev_move_left = curr_path_ind - prev_path_ind < 0;
        bool prev_move_right = curr_path_ind - prev_path_ind > 0;

        // Probability distribution of each possibility for next path
        double[] prob = new double[3];


        float choice;
        int next_direction = NULL_DIR; // Start at null direction.
        while (next_direction != FORWARD)
        {

            // DETERMINE PROBABILITY DISTRIBUTION -----------------------
            // If on furthermost left, OR the previous turn was leftwards
            // move forward 80% and right 20%
            if (next_square <= 1 || (prev_move_left))
            {
                prob[FORWARD] = 0.8; prob[LEFT] = 0; prob[RIGHT] = 0.2;
            }
            // If on furthermost right, OR the previous turn was rightwards
            // move forward 80% and left 20%
            else if (next_square >= 7 || (prev_move_right))
            {
                prob[FORWARD] = 0.8; prob[LEFT] = 0.2; prob[RIGHT] = 0;
            }
            // Otherwise, forward 60%, left 20% and right 20%
            else
            {
                prob[FORWARD] = 0.6; prob[LEFT] = 0.2; prob[RIGHT] = 0.2;
            }
            // ----------------------------------------------------------



            choice = UnityEngine.Random.Range(0f, 1f);
            // Forward, ending and return
            if (choice <= prob[FORWARD])
            {
                next_direction = FORWARD;
                prev_path_ind = curr_path_ind;
                curr_path_ind = next_square;
                //Debug.Log(next_square);
                return next_square;

            }

            // Left turn
            else if ((choice <= (prob[FORWARD] + prob[LEFT])) && next_direction != RIGHT && !prev_move_right && next_square > 1)
            {
                next_direction = LEFT;
                next_square--;
                if (next_square <= 0)
                {
                    Debug.Log("failed");
                }




            }
            // Right turn
            else if (next_square < 7)
            {
                next_direction = RIGHT;
                next_square++;
                if (next_square > 7)
                {
                    Debug.Log("failed");
                }

            }
        }



        // In case of odd failure, return the middle of the path.
        return 3;

    }

    void SpawnInitialWaves()
    {
        // Calculate how many waves fit on screen
        float spawnHeight = multiObjectPool.spawnHeight;
        int initialWaves = 4; // Start with 3 waves.

       
        for (int i = initialWaves - 1; i >= 0; i--)
        {
            SpawnRectangleGroup(spawnHeight - i * multiObjectPool.GetPrefabSize(multiObjectPool.COLLISION_BLOCK_PREFAB).y);
        }


    }

    void SpawnRectangleGroup(float y_location = float.MaxValue)
    {
        // Arbitrarily large number for default parameter.
        if (y_location > 10000)
        {
            y_location = multiObjectPool.spawnHeight;

        }

        List<GameObject> group = multiObjectPool.GetPooledGroup();

        // Here's where we are
        int prev_position = curr_path_ind - 1;
        // Here's the next position we need to navigate to.
        int next_position = get_next_square() - 1;

        // All the squares forward and moving towards the position must be removed

        if (group.Count == 7)
        {
            float startX = -screenWidth / 2 + multiObjectPool.GetPrefabSize(multiObjectPool.COLLISION_BLOCK_PREFAB).x / 2 + multiObjectPool.spacing;
            for (int i = 0; i < 7; i++)
            {
                // If the movement is leftwards:
                if (prev_position > next_position)
                {

                    // Only deactivate elements within the bounds, treating the destination position as the left-most bound
                    if (i < next_position || i > prev_position)
                    {
                        group[i].SetActive(true);
                    }
                    else
                    {
                        multiObjectPool.ReturnToPool(group[i]);
                    }

                }
                // If the movement is rightwards
                else if (prev_position <= next_position)
                {
                    // Only deactivate elements within the bounds, treating the origin position as the left-most bound
                    if (i > next_position || i < prev_position)
                    {
                        group[i].SetActive(true);
                    }
                    else
                    {
                        multiObjectPool.ReturnToPool(group[i]);
                    }

                }

                float xPos = startX + i * multiObjectPool.GetPrefabSize(multiObjectPool.COLLISION_BLOCK_PREFAB).x + multiObjectPool.spacing * i;
                group[i].transform.position = new Vector3(xPos, y_location, 0);
            }
        } else
        {
            Debug.Log("Can't spawn, count less than 7!");

        }

    }

    void SpawnRiverArea()
    {
        GameObject river = multiObjectPool.GetPooledObject(multiObjectPool.WATER_TILE_PREFAB);
        river.transform.position = new Vector3(0, multiObjectPool.spawnHeight, 0);
    }

    void SpawnJumpTile()
    {
        GameObject jumpTile = multiObjectPool.GetPooledObject(multiObjectPool.JUMP_TILE_PREFAB);
        // Determine the x position based on the current path and booster width.
        float startX = -screenWidth / 2 + multiObjectPool.GetPrefabSize(multiObjectPool.COLLISION_BLOCK_PREFAB).x / 2 + multiObjectPool.spacing;
        float xPos = startX + ((curr_path_ind - 1) * multiObjectPool.GetPrefabSize(multiObjectPool.COLLISION_BLOCK_PREFAB).x);

        jumpTile.transform.position = new Vector3(xPos, multiObjectPool.spawnHeight);
    }

    void SpawnBooster(GameObject booster_prefab) 
    {
        // Determine the x position based on the current path and booster width.
        float startX = -screenWidth / 2 + multiObjectPool.GetPrefabSize(multiObjectPool.COLLISION_BLOCK_PREFAB).x / 2 + multiObjectPool.spacing;
        float xPos = startX + ((curr_path_ind - 1) * multiObjectPool.GetPrefabSize(multiObjectPool.COLLISION_BLOCK_PREFAB).x);

        GameObject booster = multiObjectPool.GetPooledObject(booster_prefab);
        booster.transform.position = new Vector3(xPos, multiObjectPool.spawnHeight);

    }

    
    
    


    // Checks if any collision exists between the player and any rectangular collision object.
    void CheckCollisions()
    {
        // Since we only have have to check a single colliding object (the player)
        // Pruning the collision tree won't have any advantage over just doing a linear check between the player
        // and all objects. Thus we will simply loop through and check collision which every active object.

        // Loop through the array to access each active game object

        Dictionary<GameObject, Action<GameObject>> callbacks = new Dictionary<GameObject, Action<GameObject>>();

        foreach (GameObject gameObject in multiObjectPool.activeObjects)
        {
                
                if (gameObject.activeInHierarchy && isColliding(gameObject))
                {

                    callbacks.Add(gameObject, gameObject.GetComponent<PooledObject>().onCollision);

                }

        }

        // Make all the calls.
        foreach (var item in callbacks) {

            item.Value.Invoke(item.Key);
        }

        // Once iteration is complete, invoke all collision callbacks
    }

    // Checks for a specific collision between the player and the GameObject
    // Since all other game objects are boxes, we can assume circle-box collision
    bool isColliding(GameObject collidingObject)
    {

        // A slight optimization here is to only check collisions of objects that are within one height block of the player center.
        if (collidingObject.transform.position.y > player.transform.position.y + multiObjectPool.GetPrefabSize(multiObjectPool.COLLISION_BLOCK_PREFAB).y) { return false; }

        // Start by getting the height and width of the object being checked
        float objectWidth = 0f;
        float objectHeight = 0f;
        SpriteRenderer spriteRenderer = collidingObject.GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            objectWidth = spriteRenderer.bounds.size.x;
            objectHeight = spriteRenderer.bounds.size.y;

        } else
        {
            Renderer renderer = collidingObject.GetComponent<Renderer>();
            objectWidth = renderer.bounds.size.x;
            objectHeight = renderer.bounds.size.y;
        }

        // Then determine the bounds of our object
        float lower_bound_y = collidingObject.transform.position.y - objectHeight / 2;
        float upper_bound_y = collidingObject.transform.position.y + objectHeight / 2;
        float lower_bound_x = collidingObject.transform.position.x - objectWidth / 2;
        float upper_bound_x = collidingObject.transform.position.x + objectWidth / 2;

        // Find the point closes to the center of the circle.
        float closest_x = Mathf.Max(lower_bound_x, Mathf.Min(player.transform.position.x, upper_bound_x));
        float closest_y = Mathf.Max(lower_bound_y, Mathf.Min(player.transform.position.y, upper_bound_y));

        // Find the change in position
        float dx = player.transform.position.x - closest_x;
        float dy = player.transform.position.y - closest_y;

        if (player.radius * player.radius > (dx * dx) + (dy * dy))
        {
            return true;

        }

        return false;
    }

    // COLLISION FUNCTIONS -----------------------------------

    // Called upon collision with a path collision rectangle.
    void onDeathCollision(GameObject collisionObject)
    {

        if (player.inInvulnerableState)
        {
            multiObjectPool.ReturnToPool(collisionObject);
            return;
        }

        // We don't want multiple collision calls if a player has already lost
        if (!player.isAlive)
        {
            return;

        }


        Thread.Sleep(2000);
        // Game Over!
        Time.timeScale = 0;
        ui.GameOverScreen(Mathf.FloorToInt(score));

    }

    // Called upon collision with a river tile
    void onRiverCollision(GameObject collidingObject)
    {
        if (!player.inJumpState)
        {
            Thread.Sleep(2000);
            Time.timeScale = 0;
            ui.GameOverScreen(Mathf.FloorToInt(score));

        }
    }

    // Called upon collision with the jump tile:
    void onJumpTileCollision(GameObject collidingObject)
    {

        StartCoroutine(ToggleJumpTile());

    }

 
    IEnumerator ToggleJumpTile()
    {
        player.onJumpTile = true;

        yield return new WaitForSeconds(spawn_interval);

        player.onJumpTile = false;

    }

    // Called upon collision with a small boost tile
    void onSmallBoostCollision(GameObject collidingObject)
    {

        StartCoroutine(ToggleSmall());
        multiObjectPool.ReturnToPool(collidingObject);

    }

    IEnumerator ToggleSmall()
    {
        // Halves player size.
        player.transform.localScale = new Vector3(0.25f, 0.25f, transform.localScale.z);
        player.radius = player.radius / 2;

        yield return new WaitForSeconds(10f); // 10 Seconds of being small;

        // Return to original scale.
        player.transform.localScale = new Vector3(0.5f, 0.5f, transform.localScale.z);
        player.radius = player.radius * 2;

    }

    // Called upon collision with a invulnerable boost tile
    void onInvulnerableBoostCollision(GameObject collisionObject)
    {

        StartCoroutine(ToggleInvulnerable());
        multiObjectPool.ReturnToPool(collisionObject);

    }

    IEnumerator ToggleInvulnerable()
    {
        player.inInvulnerableState = true;

        yield return new WaitForSeconds(5f);

        player.inInvulnerableState = false;

    }

    // --------------------------------------------------------

}
