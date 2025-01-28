using System;
using UnityEngine;

public class ProjectileImpact : MonoBehaviour {
    [SerializeField] private float cleanupTime = 1f;

    private void Start() {
        Invoke(nameof(TriggerDestroy), cleanupTime);
    }

    private void TriggerDestroy() {
        Destroy(gameObject);
    }
    
}
