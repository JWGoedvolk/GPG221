using System;
using UnityEngine;

namespace JW.Grid.GOAP
{
    [RequireComponent(typeof(SphereCollider))]
    public class Sensor : MonoBehaviour
    {
        [SerializeField] float radius = 5f;
        SphereCollider sphereCollider;
        
        [SerializeField] private float detectionInterval = 1f;
        float detectionTimer = 0f;
        
        public event System.Action OnDetected = delegate { };

        private GameObject target;
        Vector3 lastKnownPosition;
        public Vector3 TargetPosition => target ? target.transform.position : Vector3.zero; // Target position will be the positions of the target game object if set, otherwise zero
        public bool IsTargetInRange => TargetPosition != Vector3.zero;

        private void Awake()
        {
            sphereCollider = GetComponent<SphereCollider>();
            sphereCollider.isTrigger = true; // Make sure the collider is a trigger in case we forget to set it up as such
            sphereCollider.radius = radius; // Adjust collider size automatically
        }

        private void Update()
        {
            detectionTimer += Time.deltaTime;
            if (detectionTimer >= detectionInterval)
            {
                detectionTimer = 0f;
                
                UpdateTargetPosition(target ? target : null); 
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.gameObject.CompareTag("Player")) return;
            UpdateTargetPosition(other.gameObject); // Pass in the player game object to have its position updated
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player")) return; // TODO: Maybe add support for black/white listing tags later
            UpdateTargetPosition(); // Beccause the default value is null and we set the target to the passed in object, this clears the target object
        }

        void UpdateTargetPosition(GameObject target = null)
        {
            this.target = target;
            if (IsTargetInRange && (lastKnownPosition != TargetPosition || lastKnownPosition != Vector3.zero))
            {
                lastKnownPosition = TargetPosition;
                OnDetected?.Invoke();
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = IsTargetInRange ? Color.red : Color.green;
            Gizmos.DrawSphere(transform.position, radius);
        }
    }
}