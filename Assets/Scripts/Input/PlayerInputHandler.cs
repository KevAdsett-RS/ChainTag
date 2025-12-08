using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Input
{
    public class PlayerInputHandler : MonoBehaviour
    {
        public UnityEvent<Vector2> OnMove = new();
        public UnityEvent OnBoost = new();
        
        private InputSystem_Actions _inputActions;
        private void Awake()
        {
            _inputActions = new InputSystem_Actions();

            _inputActions.Player.Move.performed += ctx => HandleMove(ctx.ReadValue<Vector2>());
            _inputActions.Player.Move.canceled += ctx => HandleMove(Vector2.zero);

            _inputActions.Player.Boost.performed += ctx => HandleBoost();
        }

        private void OnEnable()
        {
            _inputActions.Player.Enable();
        }

        private void OnDisable()
        {
            _inputActions.Player.Disable();
        }

        private void OnDestroy()
        {
            _inputActions.Dispose();
        }

        private void HandleMove(Vector2 input)
        {
            OnMove.Invoke(input);
        }

        private void HandleBoost()
        {
            OnBoost.Invoke();
        }
    }
}