using System;
using PurrNet;
using Unity.VisualScripting;
using UnityEngine;

public class GameState : NetworkIdentity
{
    public GameObject PlayerPrefab;

    public bool IsGameStateReady;
    
    [SerializeField]
    private SyncDictionary<string, PlayerState> _players = new();

    protected override void OnSpawned()
    {
        Debug.Log("GameState::OnSpawned");
        base.OnSpawned();
        IsGameStateReady = true;
        Events.GameEvents.OnGameStateReady?.Invoke();
    }

    [ServerRpc]
    public void AddPlayer(string deviceId, PlayerID playerId, string displayName)
    {
        Debug.Log("GameState::AddPlayer");
        var player = Instantiate(PlayerPrefab);
        player.name = displayName;
        var playerState = player.GetComponent<PlayerState>();
        playerState.GiveOwnership(playerId);
        playerState.Name.value = displayName;
        playerState.Initialise(playerId);
        _players.Add(deviceId, playerState);
    }
}
