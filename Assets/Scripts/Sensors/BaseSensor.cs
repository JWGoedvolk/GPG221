using System;
using System.Collections.Generic;
using UnityEngine;

namespace JW.Grid.Sensors
{
    [RequireComponent(typeof(SphereCollider))]
    public class BaseSensor : MonoBehaviour
    {
        [SerializeField] private List<string> tagWhitelist = new List<string>();
        [SerializeField] private float radius = 0.5f;
        [SerializeField] private SphereCollider sphereCollider;

        public bool IsTriggered = false;
        public GameObject TriggeredObject;

        private void Awake()
        {
            sphereCollider = GetComponent<SphereCollider>();
            sphereCollider.radius = radius;
            sphereCollider.isTrigger = true;
        }

        void OnTriggerEnter(Collider other)
        {
            if (tagWhitelist.Contains(other.tag))
            {
                IsTriggered = true;
                TriggeredObject = other.gameObject;
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (tagWhitelist.Contains(other.tag))
            {
                IsTriggered = false;
                TriggeredObject = null;
            }
        }
        
        private void OnDrawGizmos()
        {
            Gizmos.color = IsTriggered ? Color.green : Color.red;
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}