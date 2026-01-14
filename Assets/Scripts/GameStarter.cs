using PurrNet;
using UnityEngine;
using Events;
using Match;

public class GameStarter : NetworkIdentity
{
    private MatchState _matchState;

    private string _uniqueDeviceId;
    private PlayerID _localPlayerId;
    private string _displayName;
    private NetworkManager _networkManager;
    private bool _asHost;
    
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
        if (!_networkManager.isClient)
        {
            return;
        }
        
        _matchState = FindAnyObjectByType<MatchState>();
        if (!_matchState || !_matchState.IsReady)
        {
            Debug.Log("GameStarter::StartGame: Waiting for game state to be ready");
            GameEvents.OnGameStateReady += Client_AddLocalPlayerState;
        }
        else
        {
            Debug.Log("GameStarter::StartGame: Game state is ready");
            Client_AddLocalPlayerState();
        }
    }

    [Client]
    private void Client_AddLocalPlayerState()
    {
        if (!this)
        {
            Debug.LogError($"Trying to add local player state to {nameof(GameStarter)} when it doesn't exist.");
            return;
        }
        Debug.Log($"GameStarter::Client_AddLocalPlayerState: {_uniqueDeviceId}, {_localPlayerId}, {_displayName}, {_asHost}");
        
        GameEvents.OnGameStateReady -= Client_AddLocalPlayerState;
        if (!_matchState)
        {
            _matchState = FindAnyObjectByType<MatchState>();
        }
        if (_matchState != null && _matchState.isSpawned)
        {
            // TODO: We're not going to _always_ want the host to be the starter for the chain team
            PlayerTeam starterTeam = _asHost ? PlayerTeam.ChainTeam : PlayerTeam.FreeTeam;
            
            _matchState.Server_AddPlayerState(_uniqueDeviceId, _localPlayerId, _displayName, starterTeam);
        }
    }
    
}
