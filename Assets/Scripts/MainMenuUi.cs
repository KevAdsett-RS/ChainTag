using StateMachine;
using TMPro;
using UnityEngine;

public class MainMenuUi : MonoBehaviour
{
    public TMP_InputField Input;

    public void OnPlayButtonPressed()
    {
        GameStateMachine stateMachine = GameStateMachine.Instance;
        if (stateMachine == null)
        {
            Debug.LogError("MainMenuUi::OnPlayButtonPressed: No StateMachine set");
            return;
        }
        var mainMenuState = stateMachine.CurrentState as MainMenuState;
        mainMenuState?.OnPlay(Input.text);
    }
}
