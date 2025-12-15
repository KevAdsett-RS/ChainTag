using UnityEngine;
using UnityEngine.SceneManagement;

namespace StateMachine
{
    public abstract class GameState: BaseState
    {
        private string _sceneName;
        private Scene _loadedScene;

        public GameState(StateMachine stateMachine) : base(stateMachine)
        {
        }

        protected void setSceneName(string sceneName)
        {
            _sceneName = sceneName;
        }
        protected override void OnEnter()
        {
            if (string.IsNullOrEmpty(_sceneName))
            {
                Debug.LogError($"Can't enter scene that has no name set");
            }
            LoadSceneParameters parameters = new LoadSceneParameters(LoadSceneMode.Additive);
            _loadedScene = SceneManager.LoadScene(_sceneName, parameters);
        }

        protected override void OnUpdate()
        {
            
        }

        protected override void OnExit()
        {
            SceneManager.UnloadSceneAsync(_loadedScene);
        }
    }
}