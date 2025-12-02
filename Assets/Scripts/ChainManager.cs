using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ChainManager : MonoBehaviour
{
    public Transform RightHand;
    public Transform LeftHand;
    
    public bool IsHead;
    public bool IsChained;

    public List<ChainManager> ChainedUnits;
    
    private void Start()
    {
        ChainedUnits = new List<ChainManager>();
        if (IsHead)
        {
            ChainedUnits.Add(GetComponent<ChainManager>());
        }
    }
    public void OnCollisionEnter2D(Collision2D other)
    {
        if (!IsHead)
        {
            return;
        }
;
        var endChainManager = ChainedUnits[^1];
        
        var otherChainManager = other.gameObject.GetComponent<ChainManager>();
        if (otherChainManager.IsChained)
        {
            return;
        }

        otherChainManager.IsChained = true;
        ChainedUnits.Add(otherChainManager);

        var springJoint = other.gameObject.AddComponent<SpringJoint2D>();
        springJoint.connectedBody = endChainManager.GetComponent<Rigidbody2D>();
        springJoint.frequency = 3;
        springJoint.autoConfigureDistance = false;
        springJoint.distance = 0.2f;
    }
}
