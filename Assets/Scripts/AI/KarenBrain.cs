using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using KBCore.Refs;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class KarenBrain : MonoBehaviour {
    [SerializeField, Self] private EnemyHealth enemyHealth;
    [SerializeField, Child] private SpriteRenderer spriteRenderer;
    [SerializeField, Self] private Collider2D collider;
    [SerializeField, Self] private Animator animator;
    [SerializeField, Child] private TextMeshPro dialogText;
    [SerializeField, Self] private NavMeshAgent agent;

    [SerializeField] private List<Sprite> sprites;
    [SerializeField] private SpriteRenderer fireSpriteRenderer;

    private bool enraged = false;

    private float originalSpeed = 2f;
    private bool inRange = false;

    private float tackleCooltime = 6f;
    private float tackleCooldownTimer = 0f;
    private float tackleDuration = 3f;
    private int attackDamage = 1;
    private float attackRange = 5f;

    private bool dead = false;
    private Tween activeTween;
    private bool stuck = false;
    private bool active = false;

   
    private List<string> lines = new List<string>() {
        "i would like to speak to the manager",
        "bring me your manager",
        "i'd like to talk to the manager",
        "i need to speak to the manager"
    }, angryLines = new List<string>() {
        "MANAGER?!",
        "THIS IS UNACCEPTABLE!",
        "I DEMAND BEST TREATMENT!",
        "I HAVE TO RIGHT TO SPEAK TO THE MANAGER!",
        "BRING ME YOUR MANAGER!!",
        "HIT ME! I DARE YOU!",
        "WHERE IS THE MANAGER?!!"
    };
    
    
    
    private void OnValidate() { this.ValidateRefs(); }

    private void Awake() {
        collider.enabled = false;
    }

    private void Start() {
        agent.updateUpAxis = false;
        agent.updateRotation = false;
        StartCoroutine(Awaken());
    }

    private IEnumerator Awaken() {
        yield return new WaitForSeconds(1f);
        AudioManager.Instance.PlaySFXAtPoint(transform.position, Resources.Load<AudioClip>("Audio/karenmanager"));
        active = true;
        collider.enabled = true;
    }

    public void SetEnraged(bool val) {
        enraged = val;
        spriteRenderer.sprite = enraged ? sprites[1] : sprites[0];
        transform.localScale = val ? new Vector3(1.25f, 1.25f, 1.25f) : Vector3.one;
    }

    private void Update() {
        if (!active) return;
        inRange = stuck ?
            false :
            Vector3.Distance(transform.position, PlayerInput.Instance.gameObject.transform.position) < attackRange;
        if (enemyHealth.CurrentHealth == 0 && !dead) {
            activeTween?.Kill();
            dead = true;
            agent.speed = 0f;
            collider.enabled = false;
            spriteRenderer.sprite = sprites[2];
            AudioManager.Instance.PlaySFXAtPoint(transform.position, Resources.Load<AudioClip>("Audio/screamshort"), Random.Range(1.4f, 1.6f));
            spriteRenderer.sortingOrder = 2;
            enabled = false;
        }
    }

    private void OnDisable() {
        CancelInvoke(nameof(StopCharge));
        fireSpriteRenderer.enabled = false;
    }

    // Charge and tackle when within range

    private void FixedUpdate() {
        if (!active) return;

        tackleCooldownTimer = Mathf.Clamp(tackleCooldownTimer - Time.fixedDeltaTime, 0, tackleCooltime);
        spriteRenderer.flipX =
            transform.position.x < PlayerInput.Instance.gameObject.transform.position.x ? false : true;
        agent.SetDestination(PlayerInput.Instance.gameObject.transform.position);

        if (tackleCooldownTimer <= 0f) {
            // Tackle!!
            if (!enraged) {
                AudioManager.Instance.PlaySFXAtPoint(transform.position, Resources.Load<AudioClip>("Audio/karennormal"+Random.Range(1,4)), Random.Range(0.8f, 1.2f));    
            } else {
                AudioManager.Instance.PlaySFXAtPoint(transform.position, Resources.Load<AudioClip>("Audio/karenangry"+Random.Range(1,3)), Random.Range(0.8f, 1.2f));   
            }
            
            fireSpriteRenderer.enabled = true;
            SetSpeech(Util.Choice(enraged ? angryLines : lines), 1.5f);
            tackleCooldownTimer = tackleCooltime;
            SetMoveSpeed(originalSpeed * (enraged ? 3.5f : 3f));
            attackDamage = enraged ? 2 : 1;
            Invoke(nameof(StopCharge), tackleDuration);
        }
        
    }


    private void StopCharge() {
        SetMoveSpeed(originalSpeed);
        attackDamage = 1;
        fireSpriteRenderer.enabled = false;
        
    }

    private void Unstuck() { stuck = false; }

    private void OnCollisionEnter2D(Collision2D other) {
        if (!active) return;
        if (enemyHealth.CurrentHealth == 0) return;
        if (other.gameObject.layer != 6) return; // Player layer
        if (PlayerInput.Instance.IsDodging) return;

        PlayerHealth playerHealth = other.gameObject.GetComponent<PlayerHealth>();
        if (playerHealth.Debounce) return;
        playerHealth.TakeDamage(attackDamage);
    }

    public void SetMoveSpeed(float speed) {
        agent.speed = speed;
        animator.StopPlayback();
        animator.Play(speed == 0 ? "BossIdleTest" : "BossWalkTest");
    }
    
    private void SetSpeech(string str, float cleanup = 3f) {
        CancelInvoke(nameof(ClearSpeech));
        dialogText.text = str;
        Invoke(nameof(ClearSpeech), cleanup);
    }
    
    private void ClearSpeech() {
        dialogText.text = "";
    }
    
}