using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

[System.Serializable]
public class PoolConfig
{
    public GameObject prefab;
    public int poolSize;

}

public class ObjectPool : MonoBehaviour
{
    // PREFABS ----------------------------------------
    public GameObject COLLISION_BLOCK_PREFAB;
    public GameObject WATER_TILE_PREFAB;
    public GameObject JUMP_TILE_PREFAB;
    public GameObject BOOSTER_SMALL_PREFAB;
    public GameObject BOOSTER_INVULNERABLE_PREFAB;
    // ------------------------------------------------

    // BOOSTERS
    public List<GameObject> boosterList = new List<GameObject>();



    public List<PoolConfig> poolConfigs = new List<PoolConfig>();
    public Dictionary<int, Queue<GameObject>> poolDictionary;
    public List<GameObject> activeObjects = new List<GameObject>();

    // Size-based parameters
    public float spawnHeight;
    public float despawnHeight;
    public float spacing = 0f;



    // Called at the beginning of initialization
    void Awake()
    {
        // Create all our pools and initialize the objects within.
        InitializePools();

        spawnHeight = GetPrefabSize(COLLISION_BLOCK_PREFAB).y + (Camera.main.orthographicSize);
        despawnHeight = -spawnHeight;

    }

    // Creates the object pools for each type of object (power ups, collision rectangles, water)
    void InitializePools()
    {
        poolDictionary = new Dictionary<int, Queue<GameObject>>();

        // CONFIGS ----------------------------------------------

        // Collision rectangles:
        PoolConfig collisionRectangleConfig = new PoolConfig();
        collisionRectangleConfig.poolSize = 200;
        collisionRectangleConfig.prefab = COLLISION_BLOCK_PREFAB;
        poolConfigs.Add(collisionRectangleConfig);

        // Water Rows:
        PoolConfig waterRectangleConfig = new PoolConfig();
        waterRectangleConfig.poolSize = 3;
        waterRectangleConfig.prefab = WATER_TILE_PREFAB;
        poolConfigs.Add(waterRectangleConfig);

        // Jump Tiles:
        PoolConfig jumpTileConfig = new PoolConfig();
        jumpTileConfig.poolSize = 3;
        jumpTileConfig.prefab = JUMP_TILE_PREFAB;
        poolConfigs.Add(jumpTileConfig);


        // Boosters:
        // Booster (small):
        PoolConfig boosterSmallConfig = new PoolConfig();
        boosterSmallConfig.poolSize = 3;
        boosterSmallConfig.prefab = BOOSTER_SMALL_PREFAB;
        poolConfigs.Add(boosterSmallConfig);
        boosterList.Add(BOOSTER_SMALL_PREFAB); // Add prefab to booster list.

        // Booster (invulnerable):
        PoolConfig boosterInvulnerableConfig = new PoolConfig();
        boosterInvulnerableConfig.poolSize = 3;
        boosterInvulnerableConfig.prefab = BOOSTER_INVULNERABLE_PREFAB;
        poolConfigs.Add(boosterInvulnerableConfig);
        boosterList.Add(BOOSTER_INVULNERABLE_PREFAB); // Add prefab to booster list

        // ------------------------------------------------------

        // Store the prefab IDs before instantiation.
        // This needs to be done as the prefab IDs change over time with initalization
        Dictionary<GameObject, int> prefabKeys = new Dictionary<GameObject, int>();
        foreach (PoolConfig config in poolConfigs)
        {
            prefabKeys[config.prefab] = config.prefab.GetInstanceID();
        }

        foreach (PoolConfig config in poolConfigs)
        {
            int key = prefabKeys[config.prefab];
            var objectPool = new Queue<GameObject>();

            Debug.Log("Config Size " + config.poolSize);

            for (int i = 0; i < config.poolSize; ++i)
            {
                GameObject obj = Instantiate(config.prefab);
                // Creates the pooled object class, and attaches it to this instance of the object.
                obj.AddComponent<PooledObject>().prefabID = key;
                Vector2 prefab_size = GetPrefabSize(config.prefab);
                obj.GetComponent<PooledObject>().height = prefab_size.y;
                obj.GetComponent<PooledObject>().width = prefab_size.x;
                obj.SetActive(false);
                objectPool.Enqueue(obj);

            }

            poolDictionary.Add(key, objectPool);


        }

    }

    // Adds the collision callback of all object of the prefab type.
    public void AddCollisionCallback(GameObject prefab, Action<GameObject> callback)
    {
        poolDictionary.TryGetValue(prefab.GetInstanceID(), out Queue<GameObject> pool);

        foreach (GameObject obj in pool)
        {
            obj.GetComponent<PooledObject>().onCollision = callback;

        }

    }

    // Creates a pooled group of collision rectangles and returns the list.
    // Assumes that spawning implies the highest position in the screen, thus we also set the highest member to the first member of this group.
    public List<GameObject> GetPooledGroup()
    {
        List<GameObject> group = new List<GameObject>();
        while (group.Count < 7)
        {
            GameObject obj = GetPooledObject(COLLISION_BLOCK_PREFAB);
            if (obj != null)
            {
                group.Add(obj);
                obj.SetActive(true);


            } else
            {
                Debug.LogError($"Pool empty for {COLLISION_BLOCK_PREFAB.name}");
            }
        }
        return group;


    }

    // Gets the first element of that type from the top of the queue.
    public GameObject GetPooledObject(GameObject prefab)
    {

        int key = prefab.GetInstanceID();
        if (poolDictionary.TryGetValue(key, out Queue<GameObject> pool))
        {
            if (pool.Count > 0)
            {
                GameObject obj = pool.Dequeue();
                activeObjects.Add(obj);
                obj.SetActive(true);
                return obj;

            }


        } else
        {
            Debug.LogError("Cannot Get Pooled Object");

        }
        return null;
        

    }

    public void ReturnToPool(GameObject obj)
    {
        if (!obj.activeInHierarchy)
        {
            Debug.LogError("Attempting to return an object to the pool not active in the hierarchy.");
            return;
        }

        if (obj == null)
        {
            Debug.LogError("null call to ReturnToPool()");
            return;
        }

        var pooledComponent = obj.GetComponent<PooledObject>();
        if (pooledComponent == null)
        {
            Debug.LogError("No associated pool object");
            return;
        }

        if (!pooledComponent.TryReturnToPool())
        {
            Debug.LogError("Already attempting return");
            return;
        }

        //Debug.Log(pooledComponent.prefabID);
        //Debug.Log(COLLISION_BLOCK_PREFAB.GetInstanceID());
        if (pooledComponent != null &&
            poolDictionary.TryGetValue(pooledComponent.prefabID, out Queue<GameObject> pool))
        {
            obj.SetActive(false);
            pool.Enqueue(obj);
            activeObjects.Remove(obj);
            pooledComponent.isBeingReturned = false;
            //Debug.Log($"Total elements in queue: {pool.Count}");
        }
        else
        {
            Debug.LogError("Failed to reattach to object pool");
            Destroy(obj);
        }
    }

    // Gets a vector representing height and width of the prefab.
    public Vector2 GetPrefabSize(GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogError("Prefab is null!");
            return Vector2.zero;
        }

        SpriteRenderer renderer = prefab.GetComponent<SpriteRenderer>();

        if (renderer == null)
        {
            Debug.LogError("Prefab does not have a Renderer component!");
            return Vector2.zero;
        }

        return renderer.bounds.size;
    }
}




