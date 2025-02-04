using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using KBCore.Refs;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class BodyguardBrain : MonoBehaviour {
    [SerializeField, Self] private EnemyHealth enemyHealth;
    [SerializeField, Child] private SpriteRenderer spriteRenderer;
    [SerializeField, Self] private Collider2D collider;
    [SerializeField, Self] private Animator animator;
    [SerializeField, Self] private NavMeshAgent agent;
    
    [SerializeField] private List<Sprite> sprites;
    [SerializeField] private List<Sprite> pistolSprites;
    [SerializeField] private SpriteRenderer gunSpriteRenderer, gunSpriteRendererFlipped;
    [SerializeField] private GameObject projectilePrefab;

    private Transform target;

    private AudioClip gunshotSound;
    
    private float originalMoveSpeed = 2f, originalStoppingDistance = 4;
    
    private float breathingRoom = 1f;
    private bool inRange = false;
    [SerializeField] private float shootCooltime = 1f;
    private float shootCooldownTimer = 0f;
    private int attackDamage = 1;
    private float attackRange = 3f;
    [SerializeField] private float minAttackRange = 6f, maxAttackRange = 15f;

    private bool dead = false;
    private bool active = false;

    private int initialSpriteIndex = -1;
    private Vector3 tempDestination;
    
    private void OnValidate() { this.ValidateRefs(); }

    private void Awake() {
        collider.enabled = false;
        // assign skin color
        initialSpriteIndex = 2 * Random.Range(1, 4) - 1;
        spriteRenderer.sprite = sprites[initialSpriteIndex];
    }

    private void Start() {
        attackRange = Random.Range(minAttackRange, maxAttackRange);
        gunshotSound = Resources.Load<AudioClip>("Sounds/Gunshot");
        StartCoroutine(Awaken());
        agent.updateUpAxis = false;
        agent.updateRotation = false;
        SetMoveSpeed(0f);
        AudioManager.Instance.PlaySFXAtPoint(transform.position, Resources.Load<AudioClip>("Audio/yesmrpresident"), Random.Range(.8f, 1.2f));
    }

    private IEnumerator Awaken() {
        collider.enabled = true;
        yield return new WaitForSeconds(1f);
        SetMoveSpeed(originalMoveSpeed);
        tempDestination = transform.position + new Vector3(Random.Range(-2f, 2f), Random.Range(-2f, 2f), 0f);
        yield return new WaitForSeconds(3f);
        active = true;
        agent.stoppingDistance = originalStoppingDistance;
    }


    private void Update() {
        if (enemyHealth.CurrentHealth == 0 && !dead) {
            dead = true;
            collider.enabled = false;
            spriteRenderer.sprite = sprites[initialSpriteIndex - 1];
            AudioManager.Instance.PlaySFXAtPoint(transform.position, Resources.Load<AudioClip>("Audio/screamshort"), Random.Range(.8f, 1.2f));
            spriteRenderer.sortingOrder = 2;
            CancelInvoke(nameof(RestoreMoveSpeed));
            animator.enabled = false;
            spriteRenderer.color = Color.gray;
            agent.speed = 0f;
            agent.enabled = false;
            gunSpriteRenderer.enabled = false;
            gunSpriteRendererFlipped.enabled = false;
            enabled = false;
            return;
        }
        
        if (!active) return;
        
        float distance = Vector3.Distance(transform.position, PlayerInput.Instance.gameObject.transform.position); 
        inRange = distance < attackRange;
        target = PlayerInput.Instance.transform;
        if (distance > breathingRoom) 
        {
            if (target == null) return;
            agent.SetDestination(target.position - ((target.position - transform.position).normalized * breathingRoom) );    
        }
    }
    

    private void FixedUpdate() {
        if (!active) {
            agent.SetDestination(tempDestination);
            agent.stoppingDistance = 0f;
            if (Vector3.Distance(transform.position, tempDestination) < 1f) {
                SetMoveSpeed(0f);
            }
        }
        
        if (!active) return;
        shootCooldownTimer = Mathf.Clamp(shootCooldownTimer - Time.fixedDeltaTime, 0, shootCooltime);
        spriteRenderer.flipX =
            transform.position.x < PlayerInput.Instance.gameObject.transform.position.x ? false : true;
        
        gunSpriteRendererFlipped.enabled = spriteRenderer.flipX;
        gunSpriteRenderer.enabled = !spriteRenderer.flipX;
        
        if (!inRange) {
            
            
            
        } else {
            if (shootCooldownTimer > 0f) return;
            // Shoot!!
            gunSpriteRenderer.sprite = pistolSprites[0];
            gunSpriteRendererFlipped.sprite = pistolSprites[0];
            Invoke(nameof(ResetPistolSprite), .15f);
            SetMoveSpeed(0f);
            agent.stoppingDistance = Random.Range(1f, 6f);
            attackRange = Random.Range(minAttackRange, maxAttackRange);
            AudioManager.Instance.PlaySFXAtPoint(transform.position, Resources.Load<AudioClip>("Audio/gunshot"), Random.Range(.8f, 1.2f));
            shootCooldownTimer = shootCooltime;
            // Raycast forward point destination
            GameObject muzzle = gunSpriteRenderer.enabled ?
                gunSpriteRenderer.gameObject :
                gunSpriteRendererFlipped.gameObject;
            Vector3 direction = (PlayerInput.Instance.gameObject.transform.position - muzzle.transform.position).normalized;
            float centerAngle = (Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg) - 90f;
            float projectileSpeed = Random.Range(5f, 6f);
            GameObject bullet = Instantiate(projectilePrefab, muzzle.transform.position, Quaternion.identity);
            bullet.transform.rotation = Quaternion.Euler(0, 0, centerAngle);
            bullet.GetComponent<EnemyBulletProjectile>().SetOwner(gameObject);
            bullet.GetComponent<EnemyBulletProjectile>().ProjectileSpeed = projectileSpeed;
            
            Invoke(nameof(RestoreMoveSpeed), Random.Range(.5f, 1.5f));

        }
    }


    private void ResetPistolSprite() {
        gunSpriteRenderer.sprite = pistolSprites[1];
        gunSpriteRendererFlipped.sprite = pistolSprites[1];
    }
    
    private void OnCollisionEnter2D(Collision2D other) {
        if (!active) return;
        if (enemyHealth.CurrentHealth == 0) return;
        if (other.gameObject.layer != 6) return; // Player layer
        if (PlayerInput.Instance.IsDodging) return;

        PlayerHealth playerHealth = other.gameObject.GetComponent<PlayerHealth>();
        if (playerHealth.Debounce) return;
        playerHealth.TakeDamage(attackDamage);
    }

    private void RestoreMoveSpeed() {
        SetMoveSpeed(originalMoveSpeed);
    }
    
    public void SetMoveSpeed(float speed) {
        agent.speed = speed;
        animator.StopPlayback();
        animator.Play(agent.speed == 0 ? "BossIdleTest" : "BossWalkTest");
    }
    
    public void OnHurtEnd() {
        if (agent.speed > 0f) {
            animator.Play(agent.speed == 0 ? "BossIdleTest" : "BossWalkTest");
        }
    }
    
    
}