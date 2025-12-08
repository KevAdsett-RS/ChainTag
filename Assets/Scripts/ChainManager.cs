using System.Collections.Generic;
using PurrNet;
using UnityEngine;

public class ChainManager : NetworkIdentity
{
    
    private readonly SyncVar<bool> _isHead = new();
    private readonly SyncVar<bool> _isChained = new();

    private readonly List<ChainUnit> _chainedUnits = new();
    
    protected override void OnSpawned()
    {
        if (!isServer)
        {
            return;
        }

        if (isOwner)
        {
            _isHead.value = true;
        }
        
        if (_isHead)
        {
            _chainedUnits.Add(GetComponent<ChainUnit>());
        }
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
}
