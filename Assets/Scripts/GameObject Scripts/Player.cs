using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;

public class Player : MonoBehaviour
{


    public bool inJumpState;
    public bool onJumpTile;
    public bool inInvulnerableState;

    public bool isAlive;

    public float radius = 0.25f;

    void Awake()
    {
        // Set up tap input for player movement.
        inJumpState = false;
        onJumpTile = false;
        inInvulnerableState = false;
        isAlive = true;
    }

    // Called before the first frame update.
    private void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {

    }
}
