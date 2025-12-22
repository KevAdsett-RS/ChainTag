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

    private GameRunningState _currentState = GameRunningState.InLobby;

    private float _timeRemaining = 60f;
    private readonly SyncVar<string> _timeRemainingString = new();
    private readonly SyncVar<int> _chainPlayerCount = new();
    private readonly SyncVar<int> _freePlayerCount = new();
    
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
        switch (_currentState)
        {
            case GameRunningState.InLobby:
                break;
            case GameRunningState.InMatch:
                
                if (_freePlayerCount.value <= 0)
                {
                    _currentState = GameRunningState.ChainVictory;
                    return;
                }
                _timeRemaining -= Time.deltaTime;
                _timeRemainingString.value = "" + Math.Ceiling(_timeRemaining);
                if (_timeRemaining <= 0f)
                {
                    if (_freePlayerCount.value > 0)
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

    private void OnPlayerTeamChanged(PlayerState player, PlayerTeam newTeam)
    {
        switch (newTeam)
        {
            case PlayerTeam.ChainTeam:
            {
                _chainPlayerCount.value++;
                _freePlayerCount.value--;

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
                _chainPlayerCount.value--;
                _freePlayerCount.value++;
                break;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(newTeam), newTeam, null);
        }
    }
}
