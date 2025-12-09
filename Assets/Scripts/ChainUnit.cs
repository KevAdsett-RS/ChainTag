using Input;
using PurrNet;
using UnityEngine;

[RequireComponent(typeof(LineRenderer)), RequireComponent(typeof(ChainManager))]
public class ChainUnit : NetworkIdentity
{
    public Transform RightHand;
    public Transform LeftHand;
    
    private LineRenderer _lineRenderer;

    [SerializeField]
    private ChainUnit _linkedUnit;

    [ServerOnly]
    public void SetLinkedUnit(ChainUnit unit)
    {
        Debug.Log($"Server call to link {name} to {unit.name}");
        SyncLinkedUnit(unit);
    }

    [ObserversRpc]
    private void SyncLinkedUnit(ChainUnit unit)
    {
        Debug.Log($"{name} is now linked to {unit.name}");
        
        _linkedUnit = unit;

        var springJoint = gameObject.GetComponent<SpringJoint2D>();
        springJoint.connectedBody = unit.GetComponent<Rigidbody2D>();
        springJoint.enabled = true;
        _lineRenderer.enabled = true;
    }

    protected override void OnSpawned()
    {
        _lineRenderer = gameObject.GetComponent<LineRenderer>();
    }

    private void Update()
    {
        if (!_linkedUnit)
        {
            return;
        }

        var distToLinkedLeft = Vector2.Distance(_linkedUnit.LeftHand.position, RightHand.position);
        var distToLinkedRight = Vector2.Distance(_linkedUnit.RightHand.position, LeftHand.position);
        if (distToLinkedRight > distToLinkedLeft)
        {
            _lineRenderer.SetPosition(0,_linkedUnit.LeftHand.position);
            _lineRenderer.SetPosition(1, RightHand.position);
        }
        else
        {
            _lineRenderer.SetPosition(0,_linkedUnit.RightHand.position);
            _lineRenderer.SetPosition(1, LeftHand.position);
        }

    }
}
