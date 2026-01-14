using Events;
using PurrNet;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace StateMachine.GameStates
{
    public class GameEndState : BaseGameState
    {
        private NetworkManager _networkManager;
        protected override bool UseDefaultSceneLoading() => false;
        protected override void OnEnter()
        {
            _networkManager = Object.FindAnyObjectByType<NetworkManager>();
            _networkManager.sceneModule.onPostSceneLoaded += OnSceneLoaded;
            GameEvents.OnLeaveGamePressed += OnPlayerLeavingGame;
            if (!_networkManager.isServer)
            {
                return;
            }
            _networkManager.sceneModule.LoadSceneAsync(SceneName, LoadSceneMode.Additive);
        }

        protected override void OnExit()
        {
            GameEvents.OnLeaveGamePressed -= OnPlayerLeavingGame;
            if (_networkManager)
            {
                _networkManager.sceneModule.onPostSceneLoaded -= OnSceneLoaded;
                
                
            }
            // We're no longer connected to Purrnet, so unload using Unity
            SceneManager.UnloadSceneAsync(SceneName);
            Resources.UnloadUnusedAssets();
            
            base.OnExit();
        }

        private void OnSceneLoaded(SceneID sceneId, bool asServer)
        {
            Debug.Log($"GameEndState::OnSceneLoaded: {SceneName}");
        }

        private void OnPlayerLeavingGame()
        {
            Debug.Log($"GameEndState::OnPlayerLeavingGame: {_networkManager}");
            
            if (_networkManager.isServer)
            {
                _networkManager.StopServer();
            }

            if (_networkManager.isClient)
            {
                _networkManager.StopClient();
            }
            Owner.ChangeState("GameLoadingState");
        }
    }
}