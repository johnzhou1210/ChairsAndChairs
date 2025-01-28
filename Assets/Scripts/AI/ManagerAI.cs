using System;
using System.Collections;
using System.Collections.Generic;
using KBCore.Refs;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class ManagerAI : MonoBehaviour, IBossAI {
    [SerializeField, Self] private BossHealth bossHealth;
    [SerializeField, Child] private SpriteRenderer spriteRenderer;
    [SerializeField, Self] private Animator animator;
    [SerializeField, Child] private TextMeshPro dialogText;
    [SerializeField, Self] private Collider2D collider;
    [SerializeField, Self] private NavMeshAgent agent;

    [SerializeField] private float actionDelay = 1f;
    [SerializeField] private List<Sprite> sprites;
    /*
     * 0: Default
     * 1: Command
     * 2: Dead
     * 3: Angry Command/Shout
     * 4: Shout
     * 5: Angry
     */
    
    
    private Coroutine activePhase, activeMove;
    public int ActivePhaseInt { get; private set; } = 0; // -1 means dead

    private GameObject characterProjectile;
    private GameObject karenContainer;
    
    private void OnValidate() { this.ValidateRefs(); }

    /* MANAGER
     *      Moves                                                           Probability
     * ===== PHASE 1 =====
     * - Berate                                                        50%
     *      - Curses at the player, shooting character projectiles.
     * - Command Karens                                                50%
     *      - Summons 3 Karens that fall from the sky at the same time that chase and tackle the player, dealing damage if touched.
     *
     * ===== PHASE 2 ===== (< 50% HP)
     * - Angry Berate                                                  50%
     *      - Curses at the player, stomping 3 times creating aoe shockwave damage, and then shoots more dangerous character projectiles.
     * - Command Angry Karens                                          50%
     *      - Curses at the player stomping 3 times creating aoe shockwave damage, and then summoning 5 Enraged Karens that are stronger than the base Karen.
     */

    private void Awake() {
        characterProjectile = Resources.Load("Prefabs/CharacterProjectile") as GameObject;
        karenContainer = GameObject.FindWithTag("MinionContainer");
        collider.enabled = false;
    }

    private void Start() {
        agent.updateUpAxis = false;
        agent.updateRotation = false;
        agent.speed = 0f;
    }

    private void Update() {
        if (bossHealth.CurrentHealth == 0 && ActivePhaseInt != -1) {
            // kill all karens
            for (int i = karenContainer.transform.childCount - 1; i >= 0; i--) {
                karenContainer.transform.GetChild(i).GetComponent<EnemyHealth>().TakeDamage(999);
            }
            
            ActivePhaseInt = -1;
            PlayerHealth.Instance.EndCameraShake();
            SetSpeech("");
            StopActivePhasesAndMoves();
            spriteRenderer.sprite = sprites[2];
            LevelManager.Instance.BeatLevel = true;
            enabled = false;
            return;
        }
        if (ActivePhaseInt != 1) return;
        CheckIfNeedChangePhase();
    }

    private void FixedUpdate() {
            spriteRenderer.flipX = transform.position.x <= PlayerInput.Instance.gameObject.transform.position.x ? false : true;
            agent.SetDestination(PlayerInput.Instance.gameObject.transform.position);
    }

    public void CheckIfNeedChangePhase() {
        if (ActivePhaseInt != 1) return;
        
        if (bossHealth.CurrentHealth < bossHealth.MaxHealth / 2) {
            ChangePhase(2);
        }
    }


    public void ChangePhase(int phase) {
        if (ActivePhaseInt == phase) return;
        StopActivePhasesAndMoves();
        ActivePhaseInt = phase;
        switch (phase) {
            case 1:
                activePhase = StartCoroutine(Phase1());
            break;
            case 2:
                activePhase = StartCoroutine(Phase2()); 
            break;
        }
    }


    private IEnumerator Phase1() {
        yield return new WaitForSeconds(3f);
        float summonKarenChance = 1f;
        float berateChance = 1f;
        while (true) {
            
            SetActiveMove(Phase1_Berate(30,1f));  
            yield return new WaitUntil(() => activeMove == null);
            yield return new WaitForSeconds(actionDelay);
            
            SetActiveMove(Phase1_SummonKarens(5f));
            
            yield return new WaitUntil(() => activeMove == null);
            yield return new WaitForSeconds(actionDelay);
            
            SetActiveMove(Phase1_Berate(20,1f));  
            yield return new WaitUntil(() => activeMove == null);
            yield return new WaitForSeconds(actionDelay);
        }
    }


    private IEnumerator Phase1_Berate(int numThrows, float delay) {
        List<string> lines = new List<string> {
            "You're fired after I make quick work of you.",
            "Stop slacking and go back to work.",
            "You useless employee!",
            "Know your place. I am your manager."
        };
        SetMoveSpeed(2f);
        spriteRenderer.sprite = sprites[4];
        SetSpeech(Util.Choice(lines));

        for (int j = 0; j < 2; j++) {
            AudioManager.Instance.PlaySFXAtPoint(transform.position, Resources.Load<AudioClip>("Audio/talk"+Random.Range(1,4)));
            for (int i = 0; i < numThrows; i++) {
                ThrowCharacters();     
                yield return new WaitForSeconds(.05f);
            }      
        }    
            
      
        spriteRenderer.sprite = sprites[0];
        SetMoveSpeed(0f);
        FinishAction();
    }

    private IEnumerator Phase1_SummonKarens(float delay) {
        List<string> lines = new List<string>{"Looks like we got some company!","Karens, get him!", "Animal meat should be procured by butchers.","You've got quite the attitude, livestock!"};

        spriteRenderer.sprite = sprites[1];
        SetSpeech(Util.Choice(lines));
        
        for (int i = 0; i < 6; i++) {
            GameObject karenPrefab = Resources.Load("Prefabs/Karen") as GameObject;
            GameObject karen = Instantiate(karenPrefab, new Vector3(Random.Range(-8.5f, 8.6f), Random.Range(-3.53f, 10.34f), 0f), Quaternion.identity);
        }
        
        yield return new WaitForSeconds(delay);
        spriteRenderer.sprite = sprites[0];
        
        FinishAction();
    }

    private IEnumerator Phase2() {
        SetSpeech("...");
        yield return new WaitForSeconds(2.5f);
        dialogText.fontSize *= 1.5f;
        spriteRenderer.sprite = sprites[5];
        // make all alive karens dangerous
        for (int i = karenContainer.transform.childCount - 1; i >= 0; i--) {
            karenContainer.transform.GetChild(i).GetComponent<KarenBrain>().SetEnraged(true);
        }
        
        while (true) {
            SetActiveMove(Phase2_AngryBerate(36));
            yield return new WaitUntil(() => activeMove == null);
            yield return new WaitForSeconds(actionDelay);
            SetActiveMove(Phase2_AngrySummonKarens(7f));
            yield return new WaitUntil(() => activeMove == null);
            yield return new WaitForSeconds(actionDelay);
            SetActiveMove(Phase2_AngryBerate(36));
            yield return new WaitUntil(() => activeMove == null);
            yield return new WaitForSeconds(actionDelay);
        }
    }

    private IEnumerator Phase2_AngryBerate(int numThrows) {
        List<string> angryLines = new List<string> {
            "$!@$)^&*$%^!", "!@$$^#!@#$%!%!@@","@!!@$&$%^&!"
        };
        AudioManager.Instance.PlaySFXAtPoint(transform.position, Resources.Load<AudioClip>("Audio/censorbeeps"), 1f, 4f);
        SetSpeech(Util.Choice(angryLines), 5f);
        GameObject projectilePrefab = Resources.Load("Prefabs/CharacterProjectile") as GameObject;
        PlayerHealth.Instance.StartCameraShake();
        SetMoveSpeed(6f);
        for (int i = 0; i < numThrows; i++) {
            spriteRenderer.sprite = sprites[3];
            GameObject character = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            Vector3 direction = (PlayerInput.Instance.gameObject.transform.position - transform.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            character.GetComponent<EnemyCharacterProjectile>().SetRedText();
            character.transform.rotation = Quaternion.Euler(0, 0, angle - 90f + Random.Range(-90f, 90f));
            character.GetComponent<EnemyCharacterProjectile>().ProjectileSpeed = Random.Range(8f, 10f);
            yield return new WaitForSeconds(.1f);
        }
        spriteRenderer.sprite = sprites[5];
        SetMoveSpeed(0f);
        PlayerHealth.Instance.EndCameraShake();
        yield return new WaitForSeconds(2f);
        
        FinishAction();
    }

    private IEnumerator Phase2_AngrySummonKarens(float delay) {
        List<string> lines = new List<string>{"KARENS, GET HIM!!!", "I WANT HIM DEAD. D-E-A-D.", "YOU WON'T LIVE TO SEE THE NEXT DAY!"};

        spriteRenderer.sprite = sprites[3];
        
        SetSpeech(Util.Choice(lines));
        
        for (int i = 0; i < 9; i++) {
            GameObject karenPrefab = Resources.Load("Prefabs/Karen") as GameObject;
            GameObject karen = Instantiate(karenPrefab, new Vector3(Random.Range(-8.5f, 8.6f), Random.Range(-3.53f, 10.34f), 0f), Quaternion.identity);
            karen.transform.parent = karenContainer.transform;
            karen.GetComponent<KarenBrain>().SetEnraged(true);
        }
        
        yield return new WaitForSeconds(delay);
        spriteRenderer.sprite = sprites[5];

        FinishAction();
    }
    

    private void ThrowCharacters() {
        GameObject clone = Instantiate(characterProjectile, transform.position, Quaternion.identity);
        Vector3 direction = (PlayerInput.Instance.gameObject.transform.position - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        clone.transform.rotation = Quaternion.Euler(0, 0, angle - 90f + Random.Range(-40f, 40f));
    }

    public void StopActivePhasesAndMoves() {
        CancelCoroutine(ref activePhase);
        CancelCoroutine(ref activeMove);
    }

    public void CancelCoroutine(ref Coroutine coroutine) {
        if (coroutine == null) return;
        StopCoroutine(coroutine);
        coroutine = null;
    }

    public void SetActiveMove(IEnumerator move) {
        CancelCoroutine(ref activeMove);
        activeMove = StartCoroutine(move);
    }

    private void FinishAction() { CancelCoroutine(ref activeMove); }

    private void OnCollisionStay2D(Collision2D other) {
        if (bossHealth.CurrentHealth == 0) return;
        
        if (other.gameObject.layer == 6) {
            // Player layer
            if (PlayerInput.Instance.IsDodging) return;
            PlayerHealth playerHealth = other.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth.Debounce) return;
            playerHealth.TakeDamage(ActivePhaseInt == 1 ? 1 : 4);
        }
    }

    private void SetSpeech(string str, float cleanup = 3f) {
        CancelInvoke(nameof(ClearSpeech));
        dialogText.text = str;
        Invoke(nameof(ClearSpeech), cleanup);
    }

    private void ClearSpeech() {
        dialogText.text = "";
    }

    public void Awaken() {
        PlayerInput.Instance.enabled = true;
        bossHealth.SetName("Manager");
        BossBarRender.Instance.Show();
        collider.enabled = true;
        ChangePhase(1);
    }
    
    public void SetMoveSpeed(float speed) {
        agent.speed = speed;
        animator.StopPlayback();
        animator.Play(speed == 0 ? "BossIdleTest" :"BossWalkTest");
    }

    public void OnHurtEnd() {
        if (agent.speed > 0f) {
            SetMoveSpeed(agent.speed);
        }
    }

}