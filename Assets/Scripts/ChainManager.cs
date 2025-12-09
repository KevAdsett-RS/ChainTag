using System;
using System.Collections.Generic;
using PurrNet;
using Unity.VisualScripting;
using UnityEngine;

public class ChainManager : NetworkIdentity
{
    public Sprite FreeSprite;
    public Sprite ChainedSprite;
    
    [SerializeField]
    private SyncVar<bool> _isHead = new();
    [SerializeField]
    private SyncVar<bool> _isChained = new(false);

    private readonly List<ChainUnit> _chainedUnits = new();

    private void Awake()
    {
        _isChained.onChanged += OnIsChainedChanged;
        OnIsChainedChanged(false);
    }

    protected override void OnSpawned()
    {
        if (!isServer)
        {
            return;
        }

        if (isOwner)
        {
            // TODO: it shouldn't always be the server that starts off as the head
            _isChained.value = _isHead.value = true;
        }
        
        if (_isHead)
        {
            _chainedUnits.Add(GetComponent<ChainUnit>());
        }
        else
        {
            _isChained.value = false;
        }
    }

    protected override void OnDestroy()
    {
        _isChained.onChanged -= OnIsChainedChanged;
        base.OnDestroy();
    }

    public void OnCollisionEnter2D(Collision2D other)
    {
        if (!isServer || !_isHead.value)
        {
            return;
        }
        
        var finalLink = _chainedUnits[^1];
        
        var otherChainManager = other.gameObject.GetComponent<ChainManager>();
        if (otherChainManager._isChained.value)
        {
            return;
        }

        otherChainManager._isChained.value = true;
        var chainUnit = other.gameObject.GetComponent<ChainUnit>();
        chainUnit.SetLinkedUnit(finalLink);

        _chainedUnits.Add(chainUnit);
    }

    private void OnIsChainedChanged(bool isChained)
    {
        Debug.Log($"{name}'s IsChained value changed...");
        GetComponent<SpriteRenderer>().sprite = isChained ? ChainedSprite : FreeSprite;
    }
}
