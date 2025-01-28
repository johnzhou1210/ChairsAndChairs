using System;
using System.Collections.Generic;
using KBCore.Refs;
using Unity.VisualScripting;
using UnityEngine;
using TMPro;


 public class EnemyBulletProjectile : Projectile {
    public int ProjectileDamage = 1;
    [SerializeField, Self] private Collider2D collider;
    private GameObject ownerObj;

    private void OnValidate() {
        this.ValidateRefs();
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.CompareTag("Boss")) { // Ignore Boss itself
            return;
        }
        if (collision.gameObject.layer == 10) return; // bullet detector layer
        if (collision.gameObject.layer == 7) { // if enemy layer
            if (!collision.gameObject.CompareTag("Spawner")) return;
            if (ownerObj != null && Vector3.Distance(transform.position, ownerObj.transform.position) < 1f) return;
        }

        if (collision.gameObject.CompareTag("Player")) {
            if (collision.gameObject.GetComponent<PlayerInput>().IsDodging) {
                return;
            }
            collision.gameObject.GetComponent<PlayerHealth>().TakeDamage(ProjectileDamage);
        }
        Disintegrate();
    }

    private void Start() {
        Invoke(nameof(Disintegrate), 10f);
    }
    
    public void SetOwner(GameObject owner) {
        ownerObj = owner;
        collider.enabled = true;
    }


}
 