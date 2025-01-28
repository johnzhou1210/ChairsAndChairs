using System;
using System.Collections.Generic;
using KBCore.Refs;
using UnityEngine;
using Random = UnityEngine.Random;


public class EnemyBodyProjectile : Projectile {
    public int ProjectileDamage = 3;
    [SerializeField, Self] private Collider2D collider;
    [SerializeField, Child] private SpriteRenderer spriteRenderer;
    [SerializeField, Child] private RandomRotate randomRotate;
    [SerializeField] private List<Sprite> sprites;

    private GameObject ownerObj;
    private int initialSpriteIndex = -1;
    private float lifeTime = 0f;
    
    private void OnValidate() {
        this.ValidateRefs();
    }

    private void Awake() {
        // assign skin color
        initialSpriteIndex = 2 * Random.Range(1, 4) - 1;
        spriteRenderer.sprite = sprites[initialSpriteIndex];
        collider.enabled = false;
    }

    private void Start() {
        lifeTime = 0f;
    }

    private void Update() {
        lifeTime += Time.deltaTime;
    }

    public void SetOwner(GameObject owner) {
        ownerObj = owner;
        collider.enabled = true;
    }
    

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject == ownerObj) return;
        if (collision.gameObject.CompareTag("Boss")) { // Ignore Boss itself
            return;
        }

        if (collision.gameObject.layer == 10) return; // bullet detector layer
        
        if (collision.gameObject.layer == 7) { // if enemy layer
            // deal damage to enemy, but don't destroy the bullet
            if (collision.gameObject.GetComponent<IDamageable>() != null) {
                if (lifeTime < .15f && collision.gameObject.CompareTag("Spawner")) return;
                collision.gameObject.GetComponent<IDamageable>().TakeDamage(ProjectileDamage);
                AudioManager.Instance.PlaySFXAtPoint(transform.position, Resources.Load<AudioClip>("Audio/hitgeneric"), Random.Range(.4f, .6f));
                HitEffect();
            }
            // stop body if this projectile hits a spawner
            if (collision.gameObject.CompareTag("Spawner")) {
                Deactivate();
            }
            //  stop body if it hits another large bodyguard
            if (collision.gameObject.name.Contains("LargeBodyguard")) {
                Deactivate();
            }
            return;
        }

        if (collision.gameObject.CompareTag("Player")) {
            if (collision.gameObject.GetComponent<PlayerInput>().IsDodging) {
                return;
            }
            collision.gameObject.GetComponent<PlayerHealth>().TakeDamage(ProjectileDamage);
            AudioManager.Instance.PlaySFXAtPoint(transform.position, Resources.Load<AudioClip>("Audio/hitgeneric"), Random.Range(.4f, .6f));
            HitEffect();
        }
        Deactivate();
    }

    public void Deactivate() {
        HitEffect();
        transform.position += transform.up * .2f;
        // instead of destroying projectile, just freeze it and disable the collider.
        AudioManager.Instance.PlaySFXAtPoint(transform.position, Resources.Load<AudioClip>("Audio/impactwet"), Random.Range(.8f, 1.2f));
        randomRotate.enabled = false;
        collider.enabled = false;
        ProjectileSpeed = 0f;
        spriteRenderer.sortingOrder = 2;
        spriteRenderer.color = Color.gray;
        if (initialSpriteIndex == -1) return;
        spriteRenderer.sprite = sprites[initialSpriteIndex - 1];
    }

    public override void Disintegrate() {
        Deactivate();
    }

    private void HitEffect() {
        if (DisintegrationEffect != null) {
            Instantiate(DisintegrationEffect, transform.position + (transform.up * .2f), Quaternion.identity);
        }
    }
    
}
 