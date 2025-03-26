using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;

public class SwipeBar : MonoBehaviour
{
    private PlayerInput playerInput;
    private InputAction touchPositionAction;
    private InputAction jumpAction;

    public RectTransform swipeBarRect;

    // Our player
    public Player player;

    // Start is called before the first frame update
    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();

        if (playerInput == null)
        {
            Debug.LogError("Cannot find player input");
        }

        touchPositionAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];

        if (touchPositionAction == null)
        {
            Debug.LogError("Cannot get touch action from player input");
        }

        if (swipeBarRect == null)
        {
            swipeBarRect = GetComponent<RectTransform>();
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
    // Enabling and disabling for touch -------------------
    void OnEnable()
    {
        EnhancedTouchSupport.Enable();
        touchPositionAction.Enable();
        touchPositionAction.performed += TouchPressed;
        jumpAction.Enable();
        jumpAction.performed += OnRelease;

    }

    void OnDisable()
    {
        EnhancedTouchSupport.Disable();
        touchPositionAction.Disable();
        touchPositionAction.performed -= TouchPressed;
        jumpAction.Disable();
        jumpAction.performed -= OnRelease;

    }
    // ----------------------------------------------------

    // Callback function for touch input ------------------
    void TouchPressed(InputAction.CallbackContext context)
    {
        //Debug.Log("Tap Triggered");
        Vector2 touchPosition = context.ReadValue<Vector2>();

        // Move the player if the touch is within the green bar.
        if (RectTransformUtility.RectangleContainsScreenPoint(swipeBarRect, touchPosition))
        {

            // Start game
            if (Time.timeScale == 0)
            {

                Time.timeScale = 1;

            }

            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(touchPosition.x, touchPosition.y, 0));
            player.transform.position = new Vector2(worldPosition.x, player.transform.position.y);

        } else
        {

            //Debug.Log("not within box");

        }

        
    }
    // ----------------------------------------------------

    // Callback function for jumping ----------------------
    void OnRelease(InputAction.CallbackContext context)
    {

        //Debug.Log("Release!");
        if (player.onJumpTile)
        {

            StartCoroutine(ToggleJump());

        }


    }

    IEnumerator ToggleJump()
    {
        player.inJumpState = true;

        yield return new WaitForSeconds(2f);

        player.inJumpState = false;


    }
    // ---------------------------------------------------
}
