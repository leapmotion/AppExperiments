using UnityEngine;

namespace Leap.Unity.ARTesting
{
    public class ToggleTriggerOnKey : MonoBehaviour
    {

        public Collider toggleCollider;

        public KeyCode toggleKey = KeyCode.C;

        private void OnValidate()
        {
            if (toggleCollider == null) toggleCollider = GetComponent<Collider>();
        }
        private void Reset()
        {
            if (toggleCollider == null) toggleCollider = GetComponent<Collider>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(toggleKey))
            {
                if (toggleCollider != null) toggleCollider.isTrigger = !toggleCollider.isTrigger;
            }
        }
    }
}