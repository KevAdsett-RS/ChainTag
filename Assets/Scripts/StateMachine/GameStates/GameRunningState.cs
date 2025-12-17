using PurrNet;
using UnityEngine;
using Object = UnityEngine.Object;

namespace StateMachine.GameStates
{
    public class GameRunningState : BaseGameState
    {
        private NetworkManager _networkManager;

        private GameState _gameState;

        protected override bool UseDefaultSceneLoading() => false;

        protected override void OnEnter()
        {
            _networkManager = Object.FindAnyObjectByType<NetworkManager>();
            _networkManager.sceneModule.onPostSceneLoaded += OnSceneLoaded;
            if (!_networkManager.isServer)
            {
                return;
            }
            _networkManager.sceneModule.LoadSceneAsync(SceneName);
        }

        protected override void OnExit()
        {
            if (_networkManager)
            {
                _networkManager.sceneModule.onPostSceneLoaded -= OnSceneLoaded;
            }

            base.OnExit();
        }

        private void OnSceneLoaded(SceneID sceneId, bool asServer)
        {
            Debug.Log($"GameRunningState::OnSceneLoaded: {SceneName}");
            StartGame();
        }

        private void StartGame()
        {
            Events.GameEvents.StartGame?.Invoke(Owner.UniqueDeviceId,
                Owner.GetStatePacket<PlayerID>("localPlayerId"),
                Owner.GetStatePacket<string>("displayName"));
        }
    }
}