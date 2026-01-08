using System;
using System.Collections.Generic;
using Events;
using PurrNet;
using UnityEngine;

public class GameState : NetworkIdentity
{
    public GameObject PlayerStatePrefab;
    public GameObject PlayerPrefab;

    public bool IsGameStateReady;
    
    public readonly SyncVar<string> TimeRemainingString = new();
    private readonly List<PlayerID> Server_ChainedPlayerIds = new();
    private readonly List<PlayerID> Server_FreePlayerIds = new();
    public readonly SyncVar<int> ChainPlayerCount = new();
    public readonly SyncVar<int> FreePlayerCount = new();
    public readonly SyncDictionary<PlayerID, PlayerState> Players = new();

    private enum GameRunningState
    {
        InLobby,
        InMatch,
        ChainVictory,
        FreeVictory
    }

    private GameRunningState _currentState = GameRunningState.InMatch;

    private float _timeRemaining = 60f;

    protected override void OnSpawned()
    {
        Debug.Log("GameState::OnSpawned");
        base.OnSpawned();
        
        IsGameStateReady = true;
        GameEvents.OnGameStateReady?.Invoke();

        if (isServer)
        {
            GameEvents.OnStartGame += Server_OnStartGame;
            GameEvents.OnPlayerChangedTeam += Server_OnPlayerTeamChanged;
        }

        networkManager.onPlayerLeft += OnPlayerLeft;
    }

    private void OnPlayerLeft(PlayerID player, bool asServer)
    {
        if (asServer)
        {
            Debug.Log($"GameState::OnPlayerLeft: player: {player}, asServer: {asServer}");
            Players.Remove(player);
        }
    }

    [ServerRpc]
    public void AddPlayerState(string deviceId, PlayerID playerId, string displayName, PlayerTeam team)
    {
        Debug.Log($"GameState::AddPlayer {playerId} - {displayName} ({team})");
        var player = Instantiate(PlayerStatePrefab, gameObject.transform);
        player.name = displayName + "State";
        var playerState = player.GetComponent<PlayerState>();
        
        Players.Add(playerId, playerState);
            
        playerState.GiveOwnership(playerId);
        playerState.Server_Initialise(playerId, displayName, team);

    }

    [ServerOnly]
    public void Server_InstantiatePlayers()
    {
        var chainTeamSpawnPoint = GameObject.Find("ChainTeamSpawnPoint");
        var freeTeamSpawnPoint = GameObject.Find("FreeTeamSpawnPoint");
        Debug.Log("GameState::Server_InstantiatePlayers");
        foreach (var keyValuePair in Players)
        {
            var player = Instantiate(PlayerPrefab);
            keyValuePair.Value.Server_SetBody(player);
            player.name = keyValuePair.Value.Name.value;
            
            Debug.Log($"GameState::Server_InstantiatePlayers: player {player.name} is on team {keyValuePair.Value.Team.value}");

            if (keyValuePair.Value.Team.value == PlayerTeam.ChainTeam)
            {
                Debug.Log($"GameState::Server_InstantiatePlayers: Setting {player.name}'s position to {chainTeamSpawnPoint.transform.position}");
                player.transform.position = chainTeamSpawnPoint.transform.position;
            }
            else
            {
                Debug.Log($"GameState::Server_InstantiatePlayers: Setting {player.name}'s position to {freeTeamSpawnPoint.transform.position}");
                player.transform.position = freeTeamSpawnPoint.transform.position;
            }
            
            var playerController = player.GetComponent<PlayerController>();
            playerController.GiveOwnership(keyValuePair.Key);
            playerController.Server_SetInitialPosition(keyValuePair.Value.Team.value == PlayerTeam.ChainTeam
                ? chainTeamSpawnPoint.transform.position
                : freeTeamSpawnPoint.transform.position);
            playerController.Server_LinkState(keyValuePair.Value);
        }
    }

    protected override void OnDestroy()
    {
        Debug.Log("GameState::OnDestroy");
        GameEvents.OnPlayerChangedTeam -= Server_OnPlayerTeamChanged;
        GameEvents.OnStartGame -= Server_OnStartGame;
        base.OnDestroy();
    }

    private void Update()
    {
        if (!isServer)
        {
            return;
        }
        switch (_currentState)
        {
            case GameRunningState.InLobby:
                break;
            case GameRunningState.InMatch:
                
                if (Server_FreePlayerIds.Count <= 0)
                {
                    // _currentState = GameRunningState.ChainVictory;
                    // return;
                }
                _timeRemaining -= Time.deltaTime;
                TimeRemainingString.value = "" + Math.Ceiling(_timeRemaining);
                if (_timeRemaining <= 0f)
                {
                    if (Server_FreePlayerIds.Count > 0)
                    {
                        _currentState = GameRunningState.FreeVictory;
                    }
                }

                break;
            case GameRunningState.ChainVictory:
                break;
            case GameRunningState.FreeVictory:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    [ServerOnly]
    private void Server_SwapTeams(PlayerID playerId, List<PlayerID> from, List<PlayerID> to)
    {
        if (from.Contains(playerId))
        {
            from.Remove(playerId);
        }
                
        if (to.Contains(playerId) == false)
        {
            to.Add(playerId);
        }
    }
    
    [ServerOnly]
    private void Server_OnPlayerTeamChanged(PlayerID playerId, PlayerTeam newTeam)
    {
        var player = Players[playerId];
        if (!player)
        {
            Debug.LogError($"Player with id {playerId} not found.");
            return;
        }

        switch (newTeam)
        {
            case PlayerTeam.ChainTeam:
            {
                if (Server_ChainedPlayerIds.Count > 0)
                {
                    player.Server_SetLinkedPlayer(Players[Server_ChainedPlayerIds[^1]]);
                }
                Server_SwapTeams(playerId, Server_FreePlayerIds, Server_ChainedPlayerIds);
                break;
            }
            case PlayerTeam.FreeTeam:
            {
                Server_SwapTeams(playerId, Server_ChainedPlayerIds, Server_FreePlayerIds);
                break;
            }
            case PlayerTeam.Unset:
            default: break;
        }

        ChainPlayerCount.value = Server_ChainedPlayerIds.Count;
        FreePlayerCount.value = Server_FreePlayerIds.Count;
    }

    [ServerOnly]
    private void Server_OnStartGame()
    {
        Debug.Log("GameState::Server_OnStartGame");
        Server_InstantiatePlayers();
    }
}
