using System;
using PurrNet;
using UnityEngine;
using Object = UnityEngine.Object;

public class GameStarter : NetworkIdentity
{

    private GameState _gameState;

    private string _uniqueDeviceId;
    private PlayerID _localPlayerId;
    private string _displayName;
    private NetworkManager _networkManager;

    private void Start()
    {
        Debug.Log("GameStarter::Start");
        Events.GameEvents.StartGame += StartGame;
        _networkManager = FindAnyObjectByType<NetworkManager>();
    }

    protected override void OnDestroy()
    {
        Debug.Log("GameStarter::OnDestroy");
        Events.GameEvents.StartGame -= StartGame;
        base.OnDestroy();
    }

    public void StartGame(string uniqueDeviceId, PlayerID localPlayerId, string displayName)
    {
        Debug.Log("GameStarter::StartGame");
     
        _uniqueDeviceId = uniqueDeviceId;
        _localPlayerId = localPlayerId;
        _displayName = displayName;

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
            Events.GameEvents.OnGameStateReady += AddLocalPlayerState;
        }
        else
        {
            Debug.Log("GameStarter::StartGame: Game state is ready");
            AddLocalPlayerState();
        }
    }

    private void AddLocalPlayerState()
    {
        Debug.Log($"GameStarter::AddLocalPlayerState: {_uniqueDeviceId}, {_localPlayerId}, {_displayName}");
        
        Events.GameEvents.OnGameStateReady -= AddLocalPlayerState;
        if (!_gameState)
        {
            _gameState = FindAnyObjectByType<GameState>();
        }
        _gameState.AddPlayer(_uniqueDeviceId, _localPlayerId, _displayName);
    }
    
}
