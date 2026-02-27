using System;
using System.Collections.Generic;
using Events;
using PurrNet;
using UnityEditor;
using UnityEngine;

namespace Match
{
    public class MatchState : NetworkIdentity
    {
        public GameObject playerStatePrefab;
        private struct PendingPlayerData
        {
            public string uniqueDeviceId;
            public PlayerID localPlayerId;
            public string displayName;
            public bool asHost;
        }
        
        public bool IsReady;
        public Guid Guid { get; private set; }

        public readonly SyncVar<string> TimeRemainingString = new();
        public readonly SyncVar<int> ChainPlayerCount = new();
        public readonly SyncVar<int> FreePlayerCount = new();
        public readonly SyncDictionary<PlayerID, PlayerState> Players = new();
        public readonly SyncVar<PlayerTeam> WinningTeam = new();
        
        private readonly List<PlayerID> _serverChainedPlayerIds = new();
        private readonly List<PlayerID> _serverFreePlayerIds = new();
        
        private PendingPlayerData _pendingPlayer;
        private bool _hasPendingPlayer;

        private void Awake()
        {
            LobbyEvents.AddPlayerToGame += Client_OnAddPlayerToGame;
            MatchEvents.OnPlayerChangedTeam += Server_OnPlayerTeamChanged;

            Guid = Guid.NewGuid();
            Debug.Log($"MatchState_{Guid}::Awake");
        }

        [Client]
        public void Client_OnAddPlayerToGame(string uniqueDeviceId, PlayerID localPlayerId, string displayName, bool asHost)
        {
            Debug.Log($"MatchState_{Guid}::Client_OnAddPlayerToGame {localPlayerId}: {displayName}");

            _pendingPlayer = new PendingPlayerData
            {
                uniqueDeviceId = uniqueDeviceId,
                localPlayerId = localPlayerId,
                displayName = displayName,
                asHost = asHost
            };
            _hasPendingPlayer = true;
            if (IsReady)
            {
                Client_AddLocalPlayerState();
            }
        }

        protected override void OnSpawned()
        {
            Debug.Log($"MatchState_{Guid}::OnSpawned: isServer: {isServer}");
            base.OnSpawned();

            networkManager.onPlayerLeft += OnPlayerLeft;
    
            IsReady = true;
            MatchEvents.OnMatchStateReady?.Invoke();
            if (_hasPendingPlayer)
            {
                Client_AddLocalPlayerState();
            }
        }

        private void OnPlayerLeft(PlayerID player, bool asServer)
        {
            Debug.Log($"MatchState_{Guid}::OnPlayerLeft: player: {player}, asServer: {asServer}");
            if (!asServer)
            {
                return;
            }
            Players.Remove(player);
            if (networkManager && Players.Count == 0)
            {
                networkManager.StopServer();
            }
        }

        protected override void OnDestroy()
        {
            Debug.Log($"MatchState_{Guid}::OnDestroy");
            LobbyEvents.AddPlayerToGame -= Client_OnAddPlayerToGame;
            MatchEvents.OnPlayerChangedTeam -= Server_OnPlayerTeamChanged;
            
            if (networkManager)
            {
                networkManager.onPlayerLeft -= OnPlayerLeft;
            }
    
            base.OnDestroy();
        }

        [Client]
        private void Client_AddLocalPlayerState()
        {
            if (!this)
            {
                Debug.LogError($"Trying to add local player state to MatchState when MatchState_{Guid} no longer exists.");
                return;
            }

            // TODO: We're not going to _always_ want the host to be the starter for the chain team
            PlayerTeam starterTeam = _pendingPlayer.asHost ? PlayerTeam.ChainTeam : PlayerTeam.FreeTeam;

            Debug.Log($"MatchState_{Guid}::Client_AddLocalPlayerState: {starterTeam}, name: {_pendingPlayer.displayName}, id: {_pendingPlayer.localPlayerId.id}");
            Server_AddPlayerState(
                _pendingPlayer.uniqueDeviceId,
                _pendingPlayer.localPlayerId,
                _pendingPlayer.displayName, 
                starterTeam);
        }

        [ServerRpc(requireOwnership:false)]
        public void Server_AddPlayerState(string deviceId, PlayerID playerId, string displayName, PlayerTeam team)
        {
            Debug.Log($"MatchState_{Guid}::Server_AddPlayerState {playerId.id} - {displayName} ({team})");
            var player = Instantiate(playerStatePrefab, gameObject.transform);
            player.name = displayName + "State";
            var playerState = player.GetComponent<PlayerState>();
    
            Players.Add(playerId, playerState);
        
            playerState.GiveOwnership(playerId);
            playerState.Server_Initialise(playerId, displayName, team);
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
            Debug.Log($"MatchState_{Guid}::Server_OnPlayerTeamChanged {playerId} -> {newTeam} (Match state guid: {Guid})");
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
    }
}