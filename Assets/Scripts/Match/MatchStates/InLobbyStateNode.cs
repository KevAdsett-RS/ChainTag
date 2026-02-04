using Events;
#if UNITY_EDITOR
using ParrelSync;
#endif
using UnityEngine;

namespace Match.MatchStates
{
    public class InLobbyStateNode : BaseMatchStateNode
    {
        public GameObject matchStatePrefab;
        protected override string SceneName => "InLobby";
        
        private MatchState _matchState;

        public override void Enter(bool asServer)
        {
            base.Enter(asServer);
            
            if (!asServer)
            {
                return;
            }
            _matchState = Instantiate(matchStatePrefab).GetComponent<MatchState>();
            _matchState.name = "MatchState";
            
            Debug.Log($"InLobbyStateNode::Enter: Spawned match state prefab with guid: {_matchState.Guid}");
        }

        private void OnStartGame()
        {
            Debug.Log($"InLobbyStateNode::OnStartGame: Starting game with match state guid: {_matchState.Guid}");
            machine.Next();
        }

        protected override void RegisterEvents(bool asServer)
        {
            if (!asServer)
            {
                return;
            }
            LobbyEvents.OnStartGameButtonPressed += OnStartGame;
        }

        protected override void UnregisterEvents()
        {
            LobbyEvents.OnStartGameButtonPressed -= OnStartGame;
        }

        protected override void Client_OnSceneLoaded(bool asServer)
        {
            base.Client_OnSceneLoaded(asServer);
            
            AddPlayerToGame();
        }

        private void AddPlayerToGame()
        {
            var localPlayerId = networkManager.localPlayer;
            var displayName = FindAnyObjectByType<NakamaClient>().User.DisplayName;
            var suffix = "";
#if UNITY_EDITOR
            if (ClonesManager.IsClone())
            {
                var pathTokens = ClonesManager.GetCurrentProjectPath().Split('_');
                suffix = "_" + pathTokens[^1];
            }
#endif
            var uniqueDeviceId = SystemInfo.deviceUniqueIdentifier + suffix;
 
            Debug.Log($"InLobbyStateNode::AddPlayerToGame: {localPlayerId} - {displayName}");
            LobbyEvents.AddPlayerToGame?.Invoke(uniqueDeviceId,
                localPlayerId,
                displayName,
                networkManager.isHost);
        }
    }
}
