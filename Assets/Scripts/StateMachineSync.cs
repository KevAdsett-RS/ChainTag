using System;
using Events;
using PurrNet;
using StateMachine;
using StateMachine.GameStates;
using UnityEngine;

public class StateMachineSync : NetworkIdentity
{
    private GameStateMachine _stateMachine;
    
    private SyncVar<string> _currentStateKey = new();

    private void Awake()
    {
        Debug.Log($"StateMachineSync::Awake");
        _stateMachine = FindAnyObjectByType<GameStateMachine>();
        _currentStateKey.onChanged += SyncState;
    }

    protected override void OnSpawned(bool asServer)
    {
        Debug.Log($"StateMachineSync::OnSpawned: {asServer}");
        if (networkManager.isServer)
        {
            GameEvents.OnGameStateChanged += OnStateChanged;
        }
    }

    protected override void OnDestroy()
    {
        Debug.Log($"StateMachineSync::OnDestroy");
        _currentStateKey.onChanged -= SyncState;
        
        GameEvents.OnGameStateChanged -= OnStateChanged;
        
        base.OnDestroy();
    }

    [ServerOnly]
    private void OnStateChanged(string stateKey)
    {
        if (!this || !isSpawned)
        {
            return;
        }

        Debug.Log($"StateMachineSync::OnStateChanged {stateKey}");
        _currentStateKey.value = stateKey;
    }

    private void SyncState(string stateKey)
    {
        if (networkManager.isServer)
        {
            return;
        }
        Debug.Log($"StateMachineSync::SyncState {stateKey}");
        _stateMachine.ChangeState(stateKey);
    }
}
