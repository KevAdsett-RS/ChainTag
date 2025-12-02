using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public struct HandPositions
{
    public Vector2 LeftHand;
    public Vector2 RightHand;
}

public class ChainManager : MonoBehaviour
{
    public Transform RightHand;
    public Transform LeftHand;
    
    public bool IsHead;
    public bool IsChained;

    public List<ChainManager> ChainedUnits;
    private ChainManager _previousLink;
    private HandPositions _prevLinkHandPositions;
    
    private void Start()
    {
        ChainedUnits = new List<ChainManager>();
        if (IsHead)
        {
            ChainedUnits.Add(GetComponent<ChainManager>());
        }
    }

    private void Update()
    {
        if (!_previousLink)
        {
            return;
        }
        _prevLinkHandPositions.LeftHand = _previousLink.LeftHand.position;
        _prevLinkHandPositions.RightHand = _previousLink.RightHand.position;
    }

    public void OnCollisionEnter2D(Collision2D other)
    {
        if (!IsHead)
        {
            return;
        }
;
        var finalLink = ChainedUnits[^1];
        
        var otherChainManager = other.gameObject.GetComponent<ChainManager>();
        if (otherChainManager.IsChained)
        {
            return;
        }

        otherChainManager._previousLink = finalLink;

        otherChainManager.IsChained = true;
        ChainedUnits.Add(otherChainManager);
        other.gameObject.AddComponent<ChainUnit>();

        var springJoint = other.gameObject.AddComponent<SpringJoint2D>();
        springJoint.connectedBody = finalLink.GetComponent<Rigidbody2D>();
        springJoint.frequency = 3;
        springJoint.autoConfigureDistance = false;
        springJoint.distance = 0.2f;
    }

    public HandPositions GetPrevLinkHandPositions()
    {
        return _prevLinkHandPositions;
    }
}
