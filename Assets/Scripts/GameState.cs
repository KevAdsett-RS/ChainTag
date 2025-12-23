using System;
using Events;
using PurrNet;
using Unity.VisualScripting;
using UnityEngine;

public class GameState : NetworkIdentity
{
    public GameObject PlayerPrefab;

    public bool IsGameStateReady;

    private enum GameRunningState
    {
        InLobby,
        InMatch,
        ChainVictory,
        FreeVictory
    }

    private GameRunningState _currentState = GameRunningState.InMatch;

    private float _timeRemaining = 60f;
    public readonly SyncVar<string> TimeRemainingString = new();
    public readonly SyncVar<int> ChainPlayerCount = new();
    public readonly SyncVar<int> FreePlayerCount = new();
    
    private readonly SyncDictionary<string, PlayerState> _players = new();

    private readonly SyncList<PlayerState> _chainedPlayers = new();

    protected override void OnSpawned()
    {
        Debug.Log("GameState::OnSpawned");
        base.OnSpawned();
        GameEvents.OnPlayerChangedTeam += OnPlayerTeamChanged;
        
        IsGameStateReady = true;
        GameEvents.OnGameStateReady?.Invoke();
    }

    [ServerRpc]
    public void AddPlayer(string deviceId, PlayerID playerId, string displayName, PlayerTeam team)
    {
        Debug.Log("GameState::AddPlayer");
        var player = Instantiate(PlayerPrefab);
        player.name = displayName;
        var playerState = player.GetComponent<PlayerState>();
        playerState.GiveOwnership(playerId);
        playerState.Server_Initialise(playerId, displayName, team);

        _players.Add(deviceId, playerState);
    }

    protected override void OnDestroy()
    {
        GameEvents.OnPlayerChangedTeam -= OnPlayerTeamChanged;
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
                
                if (FreePlayerCount.value <= 0)
                {
                    // _currentState = GameRunningState.ChainVictory;
                    // return;
                }
                _timeRemaining -= Time.deltaTime;
                TimeRemainingString.value = "" + Math.Ceiling(_timeRemaining);
                if (_timeRemaining <= 0f)
                {
                    if (FreePlayerCount.value > 0)
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

    private void OnPlayerTeamChanged(PlayerState player, PlayerTeam oldTeam, PlayerTeam newTeam)
    {
        switch (newTeam)
        {
            case PlayerTeam.ChainTeam:
            {
                ChainPlayerCount.value++;
                if (oldTeam == PlayerTeam.FreeTeam)
                {
                    FreePlayerCount.value--;
                }

                if (_chainedPlayers.Count > 0)
                {
                    player.SetLinkedPlayer(_chainedPlayers[^1]);
                }

                _chainedPlayers.Add(player);

                break;
            }
            // Probably won't get used, but you never know...
            case PlayerTeam.FreeTeam:
            {
                FreePlayerCount.value++;
                if (oldTeam == PlayerTeam.ChainTeam)
                {
                    ChainPlayerCount.value--;
                }
                break;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(newTeam), newTeam, null);
        }
    }
}
