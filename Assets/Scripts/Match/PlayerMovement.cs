using Input;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    public int Speed = 10;
    private Vector2 _velocity;

    private Rigidbody2D _rigidbody;
    private PlayerInputHandler _inputManager;

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _inputManager = GetComponent<PlayerInputHandler>();

        if (!_inputManager)
        {
            return;
        }

        _inputManager.OnMove.AddListener(OnMove);
    }

    private void OnDestroy()
    {
        if (_inputManager)
        {
            _inputManager.OnMove.RemoveListener(OnMove);
        }
    }

    private void FixedUpdate()
    {
        _rigidbody.MovePosition(_rigidbody.position + _velocity * Time.fixedDeltaTime);
    }

    public void OnMove(Vector2 moveVector)
    {
        _velocity = moveVector * Speed;
    }
}