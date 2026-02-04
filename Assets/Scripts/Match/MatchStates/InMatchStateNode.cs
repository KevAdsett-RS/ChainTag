using System;
using System.Collections.Generic;
using PurrNet;
using UnityEngine;

namespace Match.MatchStates
{
    public class InMatchStateNode : BaseMatchStateNode
    {
        public GameObject playerPrefab;
        
        protected override string SceneName => "InMatch";
        
        private MatchState _matchState;
        private readonly List<GameObject> _serverPlayerGameObjects = new();

        private float _timeRemaining = 60f;

        public override void Enter(bool asServer)
        {
            
            _matchState = FindAnyObjectByType<MatchState>();
            base.Enter(asServer);
        }

        public override void StateUpdate(bool asServer)
        {
            base.StateUpdate(asServer);
            if (!asServer)
            {
                return;
            }

            if (_matchState.FreePlayerCount.value <= 0)
            {
                EndMatch(PlayerTeam.ChainTeam);
                return;
            }
            
            _timeRemaining -= Time.deltaTime;
            _matchState.TimeRemainingString.value = "" + Math.Ceiling(_timeRemaining);
            if (_timeRemaining > 0f)
            {
                return;
            }
            
            EndMatch(PlayerTeam.FreeTeam);
        }

        public override void Exit(bool asServer)
        {
            base.Exit(asServer);

            if (!asServer)
            {
                return;
            }
            
            
            foreach (var playerGameObject in _serverPlayerGameObjects)
            {
                Destroy(playerGameObject);
            }

            _serverPlayerGameObjects.Clear();
        }

        [ServerOnly]
        protected override void Server_OnSceneLoaded(SceneID scene, bool asServer)
        {
            base.Server_OnSceneLoaded(scene, asServer);
            if (asServer)
            {
                Server_InstantiatePlayers();
            }
        }

        [ServerOnly]
        public void Server_InstantiatePlayers()
        {
            var chainTeamSpawnPoint = GameObject.Find("ChainTeamSpawnPoint");
            var freeTeamSpawnPoint = GameObject.Find("FreeTeamSpawnPoint");
            Debug.Log("InMatchStateNode::Server_InstantiatePlayers");
            foreach (var keyValuePair in _matchState.Players)
            {
                var player = Instantiate(playerPrefab);
                keyValuePair.Value.Server_SetBody(player);
                player.name = keyValuePair.Value.Name.value;
        
                Debug.Log($"InMatchStateNode::Server_InstantiatePlayers: player {player.name} is on team {keyValuePair.Value.Team.value}");

                if (keyValuePair.Value.Team.value == PlayerTeam.ChainTeam)
                {
                    Debug.Log($"InMatchStateNode::Server_InstantiatePlayers: Setting {player.name}'s position to {chainTeamSpawnPoint.transform.position}");
                    player.transform.position = chainTeamSpawnPoint.transform.position;
                }
                else
                {
                    Debug.Log($"InMatchStateNode::Server_InstantiatePlayers: Setting {player.name}'s position to {freeTeamSpawnPoint.transform.position}");
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

        private void EndMatch(PlayerTeam winningTeam)
        {
            Debug.Log($"InMatchStateNode::EndMatch: {winningTeam}");
            _matchState.WinningTeam.value = winningTeam;
            machine.Next();
        }
    }
}
