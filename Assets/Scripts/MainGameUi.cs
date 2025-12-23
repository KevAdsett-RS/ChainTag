using System;
using System.Collections.Generic;
using DefaultNamespace;
using TMPro;
using UnityEngine;

public class MainGameUi : MonoBehaviour
{
    [SerializeField] private TMP_Text ChainPlayerCount;
    [SerializeField] private TMP_Text FreePlayerCount;
    [SerializeField] private TMP_Text TimeRemaining;

    private GameState _gameState;
    private readonly List<IStateBinding> _stateBindings = new();
    
    void Awake()
    {
        _gameState = FindAnyObjectByType<GameState>();
        _stateBindings.Add(new StateBinding<int>(_gameState.ChainPlayerCount, OnChainPlayerCountChanged));
        _stateBindings.Add(new StateBinding<int>(_gameState.FreePlayerCount, OnFreePlayerCountChanged));
        _stateBindings.Add(new StateBinding<string>(_gameState.TimeRemainingString, OnTimeRemainingChanged));
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

    private void OnChainPlayerCountChanged(int newValue)
    {
        ChainPlayerCount.text = "" + newValue;
    }

    private void OnFreePlayerCountChanged(int newValue)
    {
        FreePlayerCount.text = "" + newValue;
    }
    
    private void OnTimeRemainingChanged(string newValue)
    {
        TimeRemaining.text = newValue;
    }

}
