using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionBlockController : MonoBehaviour
{
    // To be adjusted
    public float speed = 5f;

    private ObjectPool objectPool;


    // Start is called before the first frame update
    void Start()
    {
        objectPool = FindObjectOfType<ObjectPool>(); 
        
    }
    
    // Update is called once per frame
    void Update()
    {
        // Movement downward of block.
        transform.Translate(Vector3.down * speed * Time.deltaTime);

        // Move to top if moved off screen.
        if (transform.position.y < objectPool.despawnHeight)
        {
            objectPool.ReturnToPool(gameObject);

        }
        
    }
}
