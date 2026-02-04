using PurrNet;
using UnityEngine;

public class StateMachineSpawner : NetworkIdentity
{
    public GameObject MatchStateMachinePrefab;

    public void Spawn()
    {
        Instantiate(MatchStateMachinePrefab);
    }
}
