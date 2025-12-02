using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public int Speed = 10;
    private Vector2 _velocity;

    private Rigidbody2D _rigidbody;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
    }
    private void FixedUpdate()
    {
        // _rigidbody.AddForce(new Vector3(_velocity.x, _velocity.y, 0));
        _rigidbody.MovePosition(_rigidbody.position + _velocity * Time.fixedDeltaTime);
    }

    public void OnMove(InputValue value)
    {
        Vector2 moveVector = value.Get<Vector2>();
        _velocity = moveVector * Speed;
    }
}
