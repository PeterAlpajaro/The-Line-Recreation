using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraAspectAdjuster : MonoBehaviour
{
    public float targetWidth = 7f; // 7 units wide for 7 blocks
    private Camera mainCamera;

    void Awake()
    {
        mainCamera = GetComponent<Camera>();
        AdjustCameraSize();
    }

    void AdjustCameraSize()
    {
        float targetAspect = targetWidth / (targetWidth * (Screen.height / (float)Screen.width));
        mainCamera.orthographicSize = targetWidth / (2 * targetAspect);
    }
}