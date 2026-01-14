using UnityEngine;

namespace Singletons
{
    public class SingleCamera : SingleInstanceOnly<SingleCamera>
    {
        protected override void Awake()
        {
            base.Awake();
            if (DestroyedOnAwake)
            {
                return;
            }

            GetComponentInChildren<AudioListener>().enabled = true;
        }
    }
}