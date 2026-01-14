using UnityEngine;

public class SingleInstanceOnly<T> : MonoBehaviour where T : MonoBehaviour
{
    protected bool DestroyedOnAwake { get; private set; }
    private static T _instance;

    protected virtual void Awake()
    {
        var decisionString = _instance != null ? "Instance already exists. Destroying this instance" : "Creating first instance";
        Debug.Log($"{name} is awake, {decisionString}");
        if (_instance != null)
        {
            DestroyedOnAwake = true;
            Destroy(gameObject);
            return;
        }

        _instance = this as T;
        DontDestroyOnLoad(this);
    }
}
