using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatingObject : PersistableObject
{
    [SerializeField] private Vector3 angularVelocity;

    private void Update()
    {
        transform.Rotate(angularVelocity * Time.deltaTime);
    }
}
