using System;
using Input;
using PurrNet;
using TMPro;
using UnityEngine;

public class PlayerState : NetworkIdentity
{
    public readonly SyncVar<string> Name = new();

    private void Awake()
    {
        Debug.Log("PlayerState::Awake");
        Name.onChanged += OnNameChanged;
    }

    [TargetRpc]
    public void Initialise(PlayerID target)
    {
        Debug.Log($"PlayerState::Initialise: isOwner: {isOwner}");
        if (!isOwner)
        {
            return;
        }
        
        gameObject.AddComponent<PlayerInputHandler>();
        gameObject.GetComponent<PlayerMovement>().enabled = true;
    }
    
    protected override void OnSpawned()
    {
        Debug.Log("PlayerState::OnSpawned");
        base.OnSpawned();
    }

    protected override void OnDestroy()
    {
        Debug.Log("PlayerState::OnDestroy");
        Name.onChanged -= OnNameChanged;
        base.OnDestroy();
    }
    
    private void OnNameChanged(string newValue)
    {
        Debug.Log("PlayerState::OnNameChanged");
        name = newValue;
    }
}
