using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ChainUnit : MonoBehaviour
{
    private LineRenderer _lineRenderer;

    private ChainManager _chainManager;
    private void Start()
    {
        _chainManager = GetComponent<ChainManager>();
        _lineRenderer = gameObject.AddComponent<LineRenderer>();
        _lineRenderer.startWidth = 0.01f;
    }

    private void Update()
    {
        var handPositions = _chainManager.GetPrevLinkHandPositions();
        _lineRenderer.SetPosition(0, _chainManager.LeftHand.position);
        _lineRenderer.SetPosition(1, handPositions.RightHand);
    }
}
