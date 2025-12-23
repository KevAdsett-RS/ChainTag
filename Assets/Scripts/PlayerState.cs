using System;
using Events;
using Input;
using PurrNet;
using TMPro;
using UnityEngine;

public enum PlayerTeam
{
    Unset,
    ChainTeam,
    FreeTeam
}

[RequireComponent(typeof(ChainUnit)), RequireComponent(typeof(PlayerMovement))]
public class PlayerState : NetworkIdentity
{
    public readonly SyncVar<string> Name = new();
    public readonly SyncVar<PlayerTeam> Team = new();
    public readonly SyncVar<PlayerID> Id = new();
    
    private readonly SyncVar<bool> _isHead = new();
    

    private void Awake()
    {
        Debug.Log("PlayerState::Awake");
        Name.onChanged += OnNameChanged;
    }

    [ServerOnly]
    public void Server_Initialise(PlayerID target, string displayName, PlayerTeam team)
    {
        Id.value = target;
        Name.value = displayName;
        ChangeTeam(team);
        
        _isHead.value = team == PlayerTeam.ChainTeam;
        Client_Initialise(target);
    }

    [TargetRpc]
    public void Client_Initialise(PlayerID target)
    {
        Debug.Log($"PlayerState::Client_Initialise: isOwner: {isOwner}");
        if (!isOwner)
        {
            return;
        }
        
        gameObject.AddComponent<PlayerInputHandler>();
        gameObject.GetComponent<PlayerMovement>().enabled = true;
    }

    [ServerOnly]
    public void ChangeTeam(PlayerTeam newTeam)
    {
        var prevTeam = Team.value;
        Team.value = newTeam;
        GameEvents.OnPlayerChangedTeam?.Invoke(this, prevTeam, newTeam);
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
    }
    
    public void OnCollisionEnter2D(Collision2D other)
    {
        if (!isServer || !_isHead.value)
        {
            return;
        }

        other.gameObject.GetComponent<PlayerState>().ChangeTeam(PlayerTeam.ChainTeam);
    }

    public void SetLinkedPlayer(PlayerState chainedPlayer)
    {
        var chainUnit = GetComponent<ChainUnit>();
        if (!chainUnit)
        {
            return;
        }
        chainUnit.SetLinkedUnit(chainedPlayer.GetComponent<ChainUnit>());
    }
}
