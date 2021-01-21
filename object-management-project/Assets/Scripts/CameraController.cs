using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private bool rotateOnX, rotateOnY, rotateOnZ;
    [Range(-1f,1f)][SerializeField] private float xSpeed, ySpeed, zSpeed;

    void Update()
    {
        if (rotateOnX)
        {
            transform.RotateAround(Vector3.zero, Vector3.right, xSpeed);
        }
        
        if (rotateOnY)
        {
            transform.RotateAround(Vector3.zero, Vector3.up, ySpeed);   
        }

        if (rotateOnZ)
        {
            transform.RotateAround(Vector3.zero, Vector3.forward, zSpeed);
        }
    }
}
