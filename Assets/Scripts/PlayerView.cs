using System;
using TMPro;
using UnityEngine;

public class PlayerView : MonoBehaviour
{
    [SerializeField] private TMP_Text DisplayName;

    private PlayerState _state;
    
    void Start()
    {
        Debug.Log("PlayerView::Start");
        _state = GetComponent<PlayerState>();
        _state.Name.onChanged += OnNameChanged;
        OnNameChanged(_state.Name.value);
    }

    private void OnDestroy()
    {
        Debug.Log("PlayerView::OnDestroy");
        if (!_state)
        {
            return;
        }
        _state.Name.onChanged -= OnNameChanged;
    }


    void OnNameChanged(string newName)
    {
        Debug.Log($"PlayerView::OnNameChanged: {newName}");
        DisplayName.text = newName;
    }
}
