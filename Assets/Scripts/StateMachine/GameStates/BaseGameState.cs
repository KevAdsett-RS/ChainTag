using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace StateMachine.GameStates
{
    public abstract class BaseGameState
    {
        protected GameStateMachine Owner { get; private set; }
        public string SceneName { get; private set; }

        protected static string PersistentSceneName => "ConnectedPersistent";

        private AsyncOperation _sceneLoadOperation;
        private Scene _loadedScene;
        private bool _exitPending;

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
            Debug.Log($"BaseGameState::Enter: {SceneName}, UseDefaultSceneLoading: {UseDefaultSceneLoading()}");
            
            if (UseDefaultSceneLoading())
            {
                SceneManager.sceneLoaded += SceneLoaded;
                if (string.IsNullOrEmpty(SceneName))
                {
                    Debug.LogError($"Can't enter scene that has no name set");
                }

                Debug.Log($"BaseGameState::Enter: Attempting to load {SceneName} additively");
                _sceneLoadOperation = SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Additive);
            }
            else
            {
                OnEnter();
            }
        }

        public void Update()
        {
            OnUpdate();
        }

        public void Exit()
        {
            Debug.Log($"BaseGameState::Exit: {SceneName}: UseDefaultSceneLoading: {UseDefaultSceneLoading()}");
            if (UseDefaultSceneLoading())
            {
                if (_loadedScene.IsValid())
                {
                    SceneManager.UnloadSceneAsync(_loadedScene);
                    Resources.UnloadUnusedAssets();
                }
                else
                {
                    _exitPending = true;
                    return;
                }
            }
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

        private void SceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            Debug.Log($"BaseGameState::SceneLoaded: {scene.name}");

            if (_exitPending)
            {
                Exit();
                return;
            }
            _loadedScene = scene;
            Debug.Log($"BaseGameState::SceneLoaded: Scene is valid: {_loadedScene.IsValid()}");
            SceneManager.sceneLoaded -= SceneLoaded;
            SceneManager.SetActiveScene(_loadedScene);
            OnEnter();
        }
    }
}