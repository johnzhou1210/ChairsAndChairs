using System;
using System.Collections.Generic;
using KBCore.Refs;
using Unity.VisualScripting;
using UnityEngine;
using TMPro;
using Random = UnityEngine.Random;


public class EnemyMissileProjectile : Projectile {
    public int ProjectileDamage = 1;
    [SerializeField, Self] private Collider2D collider;
    private GameObject ownerObj;
    private AudioClip explosionSound;

    private void OnValidate() {
        this.ValidateRefs();
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.CompareTag("Boss")) { // Ignore Boss itself
            return;
        }

        if (collision.gameObject.CompareTag("Window")) return;

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
        explosionSound = Resources.Load<AudioClip>("Audio/explosion");
    }

    public override void Disintegrate() {
        AudioManager.Instance.PlaySFXAtPoint(transform.position, explosionSound, Random.Range(0.8f,1.2f));
        PlayerHealth.Instance.CustomCameraShake(.33f);
        base.Disintegrate();
        
    }

    public void SetOwner(GameObject owner) {
        ownerObj = owner;
        collider.enabled = true;
    }


}
 