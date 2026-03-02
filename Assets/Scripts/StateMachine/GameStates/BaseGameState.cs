using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace StateMachine.GameStates
{
    public abstract class BaseGameState
    {
        protected GameStateMachine Owner { get; private set; }
        public string SceneName { get; private set; }
        
        private TaskCompletionSource<bool> _sceneLoadTcs;

        public void SetOwner(GameStateMachine owner) => Owner = owner;
        public void SetSceneName(string sceneName) => SceneName = sceneName;

        public async Task Enter()
        {
            Debug.Log($"BaseGameState::Enter: {SceneName}, UseDefaultSceneLoading: {UseDefaultSceneLoading()}");
            
            if (UseDefaultSceneLoading())
            {
                _sceneLoadTcs = new TaskCompletionSource<bool>();
                SceneManager.sceneLoaded += SceneLoadedInternal;
                if (string.IsNullOrEmpty(SceneName))
                {
                    throw new Exception("Can't enter scene that has no name set");
                }

                Debug.Log($"BaseGameState::Enter: Attempting to load {SceneName} additively");
                SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Additive);
                await _sceneLoadTcs.Task;
            }
            
            OnEnter();
        }

        public void Update()
        {
            OnUpdate();
        }

        public async Task Exit()
        {
            Debug.Log($"BaseGameState::Exit: {SceneName}: UseDefaultSceneLoading: {UseDefaultSceneLoading()}. Triggered by {new System.Diagnostics.StackTrace().GetFrame(1).GetMethod().Name}");
            OnExit();
            if (UseDefaultSceneLoading())
            {
                var scene = SceneManager.GetSceneByName(SceneName);
                if (scene.IsValid() && scene.isLoaded)
                {
                    var op = SceneManager.UnloadSceneAsync(scene);
                    while (op is { isDone: false })
                    {
                        await Task.Yield();
                    }
                }
            }
        }

        private void SceneLoadedInternal(Scene scene, LoadSceneMode loadSceneMode)
        {
            Debug.Log($"BaseGameState::SceneLoadedInternal: {scene.name} - Scene is valid: {scene.IsValid()}, isLoaded: {scene.isLoaded}");
            if (scene.name != SceneName)
            {
                return;
            }
            SceneManager.sceneLoaded -= SceneLoadedInternal;
            SceneManager.SetActiveScene(scene);
            _sceneLoadTcs.SetResult(true);
        }

        protected virtual bool UseDefaultSceneLoading() => true;
        protected virtual void OnEnter() {}
        protected virtual void OnUpdate() {}
        protected virtual void OnExit() {}
    }
}