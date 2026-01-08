using Events;
using PurrNet;
using UnityEngine;
using UnityEngine.SceneManagement;
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
            _networkManager.sceneModule.LoadSceneAsync(SceneName, LoadSceneMode.Additive);
        }

        protected override void OnExit()
        {
            if (_networkManager)
            {
                _networkManager.sceneModule.onPostSceneLoaded -= OnSceneLoaded;
                _networkManager.sceneModule.UnloadSceneAsync(SceneName);
            }

            base.OnExit();
        }

        private void OnSceneLoaded(SceneID sceneId, bool asServer)
        {
            Debug.Log($"GameRunningState::OnSceneLoaded: {SceneName}");
            if (_networkManager.isServer)
            {
                GameEvents.OnStartGame?.Invoke();
            }
        }
    }
}