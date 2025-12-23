using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerView : MonoBehaviour
{
    [SerializeField] private Sprite freeSprite;
    [SerializeField] private Sprite chainedSprite;
    
    [SerializeField] private TMP_Text displayName;

    private PlayerState _state;

    private readonly List<IStateBinding> _stateBindings = new();
    
    void Awake()
    {
        Debug.Log("PlayerView::Start");
        _state = GetComponent<PlayerState>();
        RegisterStateBindings();
    }

    private void RegisterStateBindings()
    {
        _stateBindings.Add(new VarStateBinding<string>(_state.Name, OnNameChanged));
        _stateBindings.Add(new VarStateBinding<PlayerTeam>(_state.Team, OnTeamChanged));
    }
    
    private void OnEnable()
    {
        foreach (var stateBinding in _stateBindings)
        {
            stateBinding.Bind();
        }
    }

    private void OnDisable()
    {
        foreach (var stateBinding in _stateBindings)
        {
            stateBinding.Unbind();
        }
    }

    void OnNameChanged(string newName)
    {
        if (string.IsNullOrEmpty(newName))
        {
            return;
        }
        Debug.Log($"PlayerView::OnNameChanged: {newName}");
        name = newName;
        displayName.text = newName;
    }

    void OnTeamChanged(PlayerTeam newTeam)
    {
        GetComponent<SpriteRenderer>().sprite = newTeam switch
        {
            PlayerTeam.ChainTeam => chainedSprite,
            PlayerTeam.FreeTeam => freeSprite,
            _ => throw new Exception($"PlayerState::OnTeamChanged: Unknown team {newTeam}")
        };
    }
}
