using System;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyCoffeeProjectile : Projectile {
    public int ProjectileDamage = 1;
    private AudioClip shatterSound;

    private void Start() {
        shatterSound = Resources.Load<AudioClip>("Audio/porcelainbreak");
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.CompareTag("Boss")) { // Ignore Boss itself
            return;
        }

        Debug.Log(collision.gameObject.name);
        
        if (collision.gameObject.CompareTag("Player")) {
            if (collision.gameObject.GetComponent<PlayerInput>().IsDodging) {
                return;
            }
            collision.gameObject.GetComponent<PlayerHealth>().TakeDamage(ProjectileDamage);
        }

        
        Disintegrate();
    }

    public override void Disintegrate() {
        AudioManager.Instance.PlaySFXAtPointUI(shatterSound, Random.Range(.8f, 1.2f));
        GameObject puddle = Resources.Load<GameObject>("Prefabs/CoffeePuddle");
        Instantiate(puddle, transform.position, Quaternion.identity);
        base.Disintegrate();
    }
    
}