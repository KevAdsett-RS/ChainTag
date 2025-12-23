using System;
using System.Collections.Generic;
using PurrNet;
using TMPro;
using UnityEngine;

public class LobbyUi : ReactiveUi
{

    [SerializeField]
    private GameObject playerListContainer;
    [SerializeField]
    private GameObject playerListEntryPrefab;

    private readonly Dictionary<string, GameObject> _playerEntries = new();
    private readonly Dictionary<string, SyncVar<string>> _playerNames = new();
    private readonly Dictionary<string, Action<string>> _nameChangeHandlers = new();
    
    protected override void RegisterBindings(GameState gameState, List<IStateBinding> stateBindings)
    {
        stateBindings.Add(new DictionaryStateBinding<string, PlayerState>(gameState.Players, OnPlayerListChanged));
    }

    private void OnPlayerListChanged(SyncDictionaryChange<string, PlayerState> change)
    {
        switch (change.operation)
        {
            case SyncDictionaryOperation.Added:
            {
                AddPlayerEntry(change);
                break;
            }
            case SyncDictionaryOperation.Removed:
            {
                Destroy(_playerEntries[change.key]);
                break;
            }
            case SyncDictionaryOperation.Set:
            {
                if (_playerEntries.TryGetValue(change.key, out var entry))
                {
                    Destroy(entry);
                }
                AddPlayerEntry(change);
                break;
            }
            case SyncDictionaryOperation.Cleared:
            {
                break;
            }
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void AddPlayerEntry(SyncDictionaryChange<string, PlayerState> change)
    {
        string playerKey = change.key;
        _playerNames.Add(playerKey, change.value.Name);
        
        var newEntry = Instantiate(playerListEntryPrefab, playerListContainer.transform);
        
        Action<string> handler = (newName) => OnPlayerNameChanged(playerKey, newName);
        _nameChangeHandlers.Add(playerKey, handler);
        change.value.Name.onChanged += handler;
        
        _playerEntries.Add(change.key, newEntry);
        
        handler(change.value.Name.value);
    }

    private void OnDestroy()
    {
        foreach (var keyValuePair in _nameChangeHandlers)
        {
            _playerNames[keyValuePair.Key].onChanged -= keyValuePair.Value;
        }
        _nameChangeHandlers.Clear();
    }

    private void OnPlayerNameChanged(string playerKey, string newValue)
    {
        var entry = _playerEntries[playerKey];
        entry.GetComponentInChildren<TMP_Text>().text = newValue;
    }
}
