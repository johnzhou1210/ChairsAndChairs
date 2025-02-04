using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using KBCore.Refs;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class LargeBodyguardBrain : MonoBehaviour {
    [SerializeField, Self] private EnemyHealth enemyHealth;
    [SerializeField, Child] private SpriteRenderer spriteRenderer;
    [SerializeField, Self] private Animator animator;
    [SerializeField, Self] private NavMeshAgent agent;
    
    [SerializeField] private List<Sprite> sprites;
    [SerializeField] private GameObject bodyPrefab;
    [SerializeField] private Collider2D collider, swingColliderR, swingColliderL;

    public static event Action OnLargeBodyguardKilled;
    
    private Transform target;

    private AudioClip attackSound;
    
    private float originalMoveSpeed = 2f;
    
    private float breathingRoom = 1f;
    private bool inAttackRange = false, inThrowRange = false;
    [SerializeField] private float cooltime = 1f;
    private float cooldownTimer = 0f;
    private int attackDamage = 3;
    private float attackRange = 3f;
    [SerializeField] private float minAttackRange = 0f, maxAttackRange = 3f, minThrowRange = 5f, maxThrowRange = 15f;
    // if between maxAttackRange and minThrowRange, just chase player until within attack range.

    private bool dead = false;
    private bool active = false;

    private Vector3 tempDestination;
    
    private void OnValidate() { this.ValidateRefs(); }

    private void Awake() {
        collider.enabled = false;
        spriteRenderer.sprite = sprites[1];
    }

    private void Start() {
        attackRange = Random.Range(minAttackRange, maxAttackRange);
        attackSound = Resources.Load<AudioClip>("Sounds/heavyswing");
        StartCoroutine(Awaken());
        agent.updateUpAxis = false;
        agent.updateRotation = false;
        SetMoveSpeed(0f);
        AudioManager.Instance.PlaySFXAtPoint(transform.position, Resources.Load<AudioClip>("Audio/robotintro"), Random.Range(.7f, .8f));
    }

    private IEnumerator Awaken() {
        collider.enabled = true;
        yield return new WaitForSeconds(1f);
        RestoreMoveSpeed();
        tempDestination = transform.position + new Vector3(Random.Range(-2f, 2f), Random.Range(-2f, 2f), 0f);
        yield return new WaitForSeconds(3f);
        active = true;
        swingColliderL.enabled = false;
        swingColliderR.enabled = false;
    }


    private void Update() {
        if (enemyHealth.CurrentHealth == 0 && !dead) {
            dead = true;
            OnLargeBodyguardKilled?.Invoke();
            collider.enabled = false;
            spriteRenderer.sprite = sprites[0];
            spriteRenderer.color = Color.gray;
            AudioManager.Instance.PlaySFXAtPoint(transform.position, Resources.Load<AudioClip>("Audio/screamshort"), Random.Range(.4f, .6f));
            spriteRenderer.sortingOrder = 2;
            CancelInvoke(nameof(RestoreMoveSpeed));
            agent.speed = 0f;
            agent.enabled = false;
            animator.enabled = false;
            swingColliderL.enabled = false;
            swingColliderR.enabled = false;
            enabled = false;
            return;
        }
        
        if (!active) {
            agent.SetDestination(tempDestination);
            if (Vector3.Distance(transform.position, tempDestination) < 1f) {
                SetMoveSpeed(0f);
            }
        }
        
        if (!active) return;
        
        float distance = Vector3.Distance(transform.position, PlayerInput.Instance.gameObject.transform.position); 
        inAttackRange = distance > minAttackRange && distance < maxAttackRange;
        inThrowRange = distance > minThrowRange && distance < maxThrowRange;
        
       
    }
    

    private void FixedUpdate() {
        if (!active) return;

        cooldownTimer = Mathf.Clamp(cooldownTimer - Time.fixedDeltaTime, 0, cooltime);
        spriteRenderer.flipX =
            transform.position.x < PlayerInput.Instance.gameObject.transform.position.x ? false : true;
        
        if (!inAttackRange && !inThrowRange) {
            // follow player
            SetMoveSpeed(originalMoveSpeed);
            target = PlayerInput.Instance.transform;
            if (target == null) return;
            agent.SetDestination(target.position - ((target.position - transform.position).normalized * breathingRoom) );    
            return;
        } 
        
        if (cooldownTimer > 0f) return;

        cooldownTimer = cooltime;
        
        if (inThrowRange) { 
            // start animation that throws bodyguard at player
            SetMoveSpeed(0f);
            animator.Play("LargeBodyguardThrowBodyguard");
            // agent.stoppingDistance = Random.Range(1f, 6f);
            // attackRange = Random.Range(minAttackRange, maxAttackRange);
            return;
        }

        if (inAttackRange) {
            // swings arms at player
            animator.Play("LargeBodyguardSwing");
            return;
        }
    }

    public void ThrowBodyguard() {
        // Raycast forward point destination
        AudioManager.Instance.PlaySFXAtPoint(transform.position, Resources.Load<AudioClip>("Audio/throw"), Random.Range(.8f, 1.2f));
        Vector3 muzzlePosition = transform.position + Vector3.up;
        Vector3 direction = (PlayerInput.Instance.gameObject.transform.position - muzzlePosition).normalized;
        float centerAngle = (Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg) - 90f;
        float projectileSpeed = Random.Range(7f, 8f);
        GameObject body = Instantiate(bodyPrefab, muzzlePosition, Quaternion.identity);
        body.transform.rotation = Quaternion.Euler(0, 0, centerAngle);
        body.GetComponent<EnemyBodyProjectile>().SetOwner(gameObject);
        body.GetComponent<EnemyBodyProjectile>().ProjectileSpeed = projectileSpeed;
            
        Invoke(nameof(RestoreMoveSpeed), Random.Range(.5f, 1.5f));
    }

    public void EnableSwingCollider() {
        (!spriteRenderer.flipX ? swingColliderR : swingColliderL).enabled = true;
    }

    public void DisableSwingCollider() {
        swingColliderL.enabled = false;
        swingColliderR.enabled = false;
    }

    public void SwingSound() {
        AudioManager.Instance.PlaySFXAtPoint(transform.position, Resources.Load<AudioClip>("Audio/swingfast"), Random.Range(.3f, .5f));
    }

    private void OnTriggerStay2D(Collider2D other) {
        if (!active) return;
        if (enemyHealth.CurrentHealth == 0) return;
        if (other.GetComponent<IDamageable>() == null) return;
        if (other.gameObject.name.Contains("LargeBodyguard")) return;
        if (other.gameObject.layer == 6) {
            if (PlayerInput.Instance.IsDodging) return;
        } 
        other.GetComponent<IDamageable>().TakeDamage(2);
    }

    private void OnCollisionEnter2D(Collision2D other) {
        if (!active) return;
        if (enemyHealth.CurrentHealth == 0) return;
        if (other.gameObject.layer != 6) return; // Player layer
        if (PlayerInput.Instance.IsDodging) return;

        PlayerHealth playerHealth = other.gameObject.GetComponent<PlayerHealth>();
        if (playerHealth.Debounce) return;
        playerHealth.TakeDamage(1);
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
            if (dead) {
                animator.Play("BossDead");
                return;
            }
            animator.Play(agent.speed == 0 ? "BossIdleTest" : "BossWalkTest");
        }
    }
    
    
}