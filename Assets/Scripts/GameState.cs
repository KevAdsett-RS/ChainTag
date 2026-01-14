using System;
using System.Collections.Generic;
using Events;
using PurrNet;
using UnityEngine;

public class GameState : NetworkIdentity
{
    public GameObject PlayerStatePrefab;
    public GameObject PlayerPrefab;

    public bool IsReady;
    
    public readonly SyncVar<string> TimeRemainingString = new();
    public readonly SyncVar<int> ChainPlayerCount = new();
    public readonly SyncVar<int> FreePlayerCount = new();
    public readonly SyncDictionary<PlayerID, PlayerState> Players = new();
    public readonly SyncVar<PlayerTeam> WinningTeam = new();
    
    private readonly List<PlayerID> _serverChainedPlayerIds = new();
    private readonly List<PlayerID> _serverFreePlayerIds = new();
    private readonly List<GameObject> _serverPlayerGameObjects = new();

    private enum MatchPhase
    {
        InLobby,
        InMatch,
        MatchEnd,
    }

    private readonly SyncVar<MatchPhase> _currentPhase = new();
    
    private MatchPhase _previousPhase;

    private float _timeRemaining = 60f;
    protected override void OnSpawned()
    {
        Debug.Log("GameState::OnSpawned");
        base.OnSpawned();
        
        IsReady = true;
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
        Debug.Log($"GameState::OnPlayerLeft: player: {player}, asServer: {asServer}");
        if (asServer)
        {
            Players.Remove(player);
            if (networkManager && Players.Count == 0)
            {
                networkManager.StopServer();
            }
        }
    }

    [ServerRpc(requireOwnership:false)]
    public void Server_AddPlayerState(string deviceId, PlayerID playerId, string displayName, PlayerTeam team)
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
            
            _serverPlayerGameObjects.Add(player);
        }
    }

    protected override void OnDestroy()
    {
        Debug.Log("GameState::OnDestroy");
        GameEvents.OnPlayerChangedTeam -= Server_OnPlayerTeamChanged;
        GameEvents.OnStartGame -= Server_OnStartGame;
        if (networkManager)
        {
            networkManager.onPlayerLeft -= OnPlayerLeft;
        }
        
        base.OnDestroy();
    }

    private void Update()
    {
        if (!isServer)
        {
            return;
        }

        if (_previousPhase != _currentPhase.value)
        {
            _previousPhase = _currentPhase.value;
            OnPhaseChanged();
        }

        if (_currentPhase.value != MatchPhase.InMatch)
        {
            return;
        }
        
        if (_serverFreePlayerIds.Count <= 0)
        {
            WinningTeam.value = PlayerTeam.ChainTeam;
            _currentPhase.value = MatchPhase.MatchEnd;
            return;
        }

        _timeRemaining -= Time.deltaTime;
        TimeRemainingString.value = "" + Math.Ceiling(_timeRemaining);
        if (_timeRemaining <= 0f)
        {
            if (_serverFreePlayerIds.Count > 0)
            {
                WinningTeam.value = PlayerTeam.FreeTeam;
                _currentPhase.value = MatchPhase.MatchEnd;
            }
        }
    }

    private void OnPhaseChanged()
    {
        Debug.Log($"GameState::OnPhaseChanged: new phase: {_currentPhase.value}");
        switch (_currentPhase.value)
        {
            case MatchPhase.MatchEnd:
                Server_OnMatchEnd();
                break;
            case MatchPhase.InLobby:
            case MatchPhase.InMatch:
            default:
                break;
        }
    }

    [ServerOnly]
    private void Server_OnMatchEnd()
    {
        Debug.Log($"GameState::Server_RemovePlayerBodies");
        foreach (var playerGameObject in _serverPlayerGameObjects)
        {
            Destroy(playerGameObject);
        }

        _serverPlayerGameObjects.Clear();
        
        GameEvents.OnMatchFinished?.Invoke();
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
        Debug.Log($"GameState::Server_OnPlayerTeamChanged {playerId} -> {newTeam}");
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
                if (_serverChainedPlayerIds.Count > 0)
                {
                    player.Server_SetLinkedPlayer(Players[_serverChainedPlayerIds[^1]]);
                }
                Server_SwapTeams(playerId, _serverFreePlayerIds, _serverChainedPlayerIds);
                break;
            }
            case PlayerTeam.FreeTeam:
            {
                Server_SwapTeams(playerId, _serverChainedPlayerIds, _serverFreePlayerIds);
                break;
            }
            case PlayerTeam.Unset:
            default: break;
        }

        ChainPlayerCount.value = _serverChainedPlayerIds.Count;
        FreePlayerCount.value = _serverFreePlayerIds.Count;
    }

    [ServerOnly]
    private void Server_OnStartGame()
    {
        Debug.Log("GameState::Server_OnStartGame");
        Server_InstantiatePlayers();
        _currentPhase.value = MatchPhase.InMatch;
    }
}
