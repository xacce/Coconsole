using UnityEngine;

namespace Coconsole
{
    public class SpamMono : MonoBehaviour
    {
        private long i;

        private void Update()
        {
            Debug.Log($"{GetHashCode()} {i++}");
        }
    }
}