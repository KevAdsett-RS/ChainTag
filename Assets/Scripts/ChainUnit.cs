using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ChainUnit : MonoBehaviour
{
    
    public ChainUnit PreviousUnit;
    
    private Rigidbody2D _rigidbody;
    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    public void FixedUpdate()
    {
        
    }
}
