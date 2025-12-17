using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace StateMachine.GameStates
{
    public abstract class BaseGameState
    {
        protected GameStateMachine Owner { get; private set; }
        public string SceneName { get; private set; }

        private Scene _loadedScene;
        private bool _isReady;

        public void SetOwner(GameStateMachine owner)
        {
            Owner = owner;
        }

        public void SetSceneName(string sceneName)
        {
            SceneName = sceneName;
        }

        public void Enter()
        {
            Debug.Log($"Entering {SceneName} state");
            
            if (UseDefaultSceneLoading())
            {
                if (string.IsNullOrEmpty(SceneName))
                {
                    Debug.LogError($"Can't enter scene that has no name set");
                }

                LoadSceneParameters parameters = new LoadSceneParameters(LoadSceneMode.Additive);
                _loadedScene = SceneManager.LoadScene(SceneName, parameters);
            }
            else
            {
                OnEnter();
            }
        }

        public void Update()
        {
            
            if (_loadedScene.isLoaded)
            {
                if (!_isReady)
                {
                    Debug.Log($"BaseGameState::Update: {SceneName} is now loaded...");
                    SceneManager.SetActiveScene(_loadedScene);
                    _isReady = true;
                    OnSceneLoaded();
                    if (UseDefaultSceneLoading())
                    {
                        OnEnter();
                    }
                }
            }
            OnUpdate();
        }

        public void Exit()
        {
            Debug.Log($"Exiting {SceneName} state");
            // try
            // {
            if (UseDefaultSceneLoading())
            {
                SceneManager.UnloadSceneAsync(_loadedScene);
                Resources.UnloadUnusedAssets();
            }

            // }
            // catch (Exception e)
            // {
                // Debug.LogError($"BaseGameState::Exit: Error trying to exit state \"{SceneName}\": {e.Message}");
            // }
            OnExit();
        }
        
        protected virtual bool UseDefaultSceneLoading()
        {
            return true;
        }
        protected virtual void OnEnter()
        {
            Debug.Log($"BaseGameState::OnEnter: {SceneName}");
        }

        protected virtual void OnUpdate()
        {
        }

        protected virtual void OnExit()
        {
            Debug.Log($"BaseGameState::OnExit: {SceneName}");
        }

        protected virtual void OnSceneLoaded()
        {
            Debug.Log($"BaseGameState::OnSceneLoaded: {SceneName}");
        }
    }
}