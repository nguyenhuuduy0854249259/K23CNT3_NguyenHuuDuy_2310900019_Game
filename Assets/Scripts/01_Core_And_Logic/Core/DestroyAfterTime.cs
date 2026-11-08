using UnityEngine;

namespace ActionRPG.Core
{
    public class DestroyAfterTime : MonoBehaviour
    {
        public float timeToDestroy = 1f;

        void Start()
        {
            Destroy(gameObject, timeToDestroy);
        }
    }
}
