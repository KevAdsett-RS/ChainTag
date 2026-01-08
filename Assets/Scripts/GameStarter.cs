using System;
using PurrNet;
using UnityEngine;
using Events;

public class GameStarter : NetworkIdentity
{
    private GameState _gameState;

    private string _uniqueDeviceId;
    private PlayerID _localPlayerId;
    private string _displayName;
    private NetworkManager _networkManager;
    private bool _asHost;

    private void Awake()
    {
        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        Debug.Log("GameStarter::Start");
        LobbyEvents.AddPlayerToGame += AddPlayerToGame;
        _networkManager = FindAnyObjectByType<NetworkManager>();
    }

    protected override void OnDestroy()
    {
        Debug.Log("GameStarter::OnDestroy");
        LobbyEvents.AddPlayerToGame -= AddPlayerToGame;
        base.OnDestroy();
    }

    public void AddPlayerToGame(string uniqueDeviceId, PlayerID localPlayerId, string displayName, bool asHost)
    {
        Debug.Log($"GameStarter::AddPlayerToGame {localPlayerId}: {displayName}");
     
        _uniqueDeviceId = uniqueDeviceId;
        _localPlayerId = localPlayerId;
        _displayName = displayName;
        _asHost = asHost;

        if (!_networkManager)
        {
            return;
        }
        if (!_networkManager.isHost && !_networkManager.isClient)
        {
            return;
        }
        
        _gameState = FindAnyObjectByType<GameState>();
        if (!_gameState || !_gameState.IsGameStateReady)
        {
            Debug.Log("GameStarter::StartGame: Waiting for game state to be ready");
            GameEvents.OnGameStateReady += AddLocalPlayerState;
        }
        else
        {
            Debug.Log("GameStarter::StartGame: Game state is ready");
            AddLocalPlayerState();
        }
    }

    private void AddLocalPlayerState()
    {
        Debug.Log($"GameStarter::AddLocalPlayerState: {_uniqueDeviceId}, {_localPlayerId}, {_displayName}, {_asHost}");
        
        GameEvents.OnGameStateReady -= AddLocalPlayerState;
        if (!_gameState)
        {
            _gameState = FindAnyObjectByType<GameState>();
        }
        // TODO: We're not going to _always_ want the host to be the starter for the chain team
        _gameState.AddPlayerState(_uniqueDeviceId, _localPlayerId, _displayName, _asHost ? PlayerTeam.ChainTeam : PlayerTeam.FreeTeam);
    }
    
}
