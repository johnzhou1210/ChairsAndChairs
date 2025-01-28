using System;
using KBCore.Refs;
using Unity.VisualScripting;
using UnityEngine;

public class DamageZone : MonoBehaviour {
   [SerializeField, Self] private Collider2D collider;

   [SerializeField] private int damage = 1;
   [SerializeField] private bool destroyOnHit = false;

   private void OnValidate() {
      this.ValidateRefs();
   }

   private void OnDisable() {
      damage = 0;
   }

   public void Disable() {
      collider.enabled = false;
   }

   private void OnTriggerStay2D(Collider2D other) {
      if (other.gameObject.layer == 6) { // Player layer
         PlayerHealth plrHealth = other.GetComponent<PlayerHealth>();
         if (plrHealth.Debounce) return;
         if (PlayerInput.Instance.IsDodging) return;
         other.GetComponent<PlayerHealth>().TakeDamage(damage);
         if (destroyOnHit) {
            Destroy(gameObject);
         }
      }
   }
}
