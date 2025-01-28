using System;
using Unity.VisualScripting;
using UnityEngine;


 public class EnemyCoffeeBelchProjectile : Projectile {
    public int ProjectileDamage = 1;

    private void OnTriggerStay2D(Collider2D collision) {
        if (collision.gameObject.CompareTag("Boss")) { // Ignore Boss itself
            return;
        }

        if (collision.gameObject.CompareTag("Player")) {
            if (collision.gameObject.GetComponent<PlayerInput>().IsDodging) {
                return;
            }
            collision.gameObject.GetComponent<PlayerHealth>().TakeDamage(ProjectileDamage);
        }
    }

    private void Start() {
        Invoke(nameof(Disintegrate), 10f);
    }

    private void OnTriggerEnter2D(Collider2D other) {
        return;
    }


}
 