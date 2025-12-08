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
        Debug.Log($"Updating {name}");
        if (!_linkedUnit)
        {
            Debug.Log($"{name} has no linked unit");
            return;
        }

        _lineRenderer.SetPosition(0,_linkedUnit.LeftHand.position);
        _lineRenderer.SetPosition(1, RightHand.position);
    }
}
