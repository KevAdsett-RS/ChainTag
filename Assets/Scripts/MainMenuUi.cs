using TMPro;
using UnityEngine;

public class MainMenuUi : MonoBehaviour
{
    public TMP_InputField Input;

    private void Start()
    {
        Debug.Log("MainMenuUi::Start");
        Input.onEndEdit.AddListener(OnInputEdited);
    }

    private void OnDestroy()
    {
        Input?.onEndEdit.RemoveListener(OnInputEdited);
    }

    public void OnStartButtonPressed()
    {
        Events.MainMenuEvents.OnStartButtonPressed?.Invoke();
    }
    
    public void SetDisplayName(string displayName)
    {
        Debug.Log($"MainMenuUi::SetDisplayName: {displayName}");
        Input.text = displayName;
        Events.MainMenuEvents.OnUsernameEdited?.Invoke(Input.text);
    }

    private void OnInputEdited(string newValue)
    {
        Events.MainMenuEvents.OnUsernameEdited?.Invoke(Input.text);
        
    }
}
