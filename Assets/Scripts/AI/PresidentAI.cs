using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using KBCore.Refs;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class PresidentAI : MonoBehaviour, IBossAI {
    [SerializeField, Self] private PresidentHealth bossHealth;
    [SerializeField, Child] private SpriteRenderer spriteRenderer;
    [SerializeField, Self] private Animator animator;
    [SerializeField, Child] private TextMeshPro dialogText;
    [SerializeField, Self] private Collider2D collider;

    [SerializeField] private float actionDelay = 1f;

    [SerializeField] private Slider bossHealthBar;
    [SerializeField] private GameObject spawnerContainer,
        smallBodyguardContainer,
        largeBodyguardContainer,
        bodyguardJumpTrigger,
        bodyguardPrefab,
        largeBodyguardPrefab,
        laserSpawnPrefab, reminderPrefab;

    public static Vector2Int SpawnerStatus = new(8,8);
    public static int LargeBodyguardsKilled = 0;

    private Coroutine activePhase, activeMove;
    private Coroutine reminderCoroutine;
    private float reminderTimer = 0f;
    private bool reminderEnabled = true;
    public int ActivePhaseInt { get; private set; } = 0; // -1 means dead

    private float moveSpeed = 0f;
    private bool readyToSpawn = true;

    private void OnValidate() { this.ValidateRefs(); }

    /* PRESIDENT
     *      Moves                                                           Probability
     * ===== PHASE 1 =====                                                  N/A
     * - There are 8 Spawners.
     * - Summon 1
     *      - Summons 4 large bodyguards to the middle room, and 4 in each side room spawner.
     *      - Waits until 4 large bodyguards are killed, and then repeats this summon.
     * - Secret service protection
     *      - Immune to damage. If bullet tries to deal damage, bodyguard will soak up hit.
     *
     * ===== PHASE 2 =====                                                  N/A
     * - When 4 or more spawners are destroyed, transitions to this phase.
     * - Summons 1 large bodyguard to all spawners, and then periodically spawn smaller bodyguards every two seconds.
     * - Waits until 8 large bodyguards are killed, and then repeats this summon.
     *
     * ===== PHASE 3 =====
     * - All spawners must be destroyed.                                    N/A
     * - No longer summons anything
     *      - Dies in one hit if hit by player projectile
     */

    private void Awake() { collider.enabled = false; }


    private void Start() {
        LargeBodyguardBrain.OnLargeBodyguardKilled += IncrementLargeBodyguardKilled;
        SpawnerHealth.OnSpawnerDestroyed += DecrementSpawnersRemaining;

    }

    private void OnDestroy() {
        LargeBodyguardBrain.OnLargeBodyguardKilled -= IncrementLargeBodyguardKilled;
        SpawnerHealth.OnSpawnerDestroyed -= DecrementSpawnersRemaining;
    }

    private void Update() {
        print(SpawnerStatus.x);
        if (bossHealth.CurrentHealth == 0 && ActivePhaseInt != -1) {
            ActivePhaseInt = -1;
            bossHealthBar.value = 0f;
            PlayerHealth.Instance.EndCameraShake();
            SetSpeech("");
            StopActivePhasesAndMoves();
            ClearAllBodyguardContainers();
            LevelManager.Instance.BeatLevel = true;
            enabled = false;
            return;
        }

        if (reminderEnabled) {
            reminderTimer += Time.deltaTime;    
        }
        
        if (ActivePhaseInt != 1) return;
        CheckIfNeedChangePhase();
    }

    private void IncrementLargeBodyguardKilled() {
        LargeBodyguardsKilled++;
        if (LargeBodyguardsKilled % (ActivePhaseInt == 1 ? 4 : 8) == 0) {
            readyToSpawn = true;
        }
    }

    private void DecrementSpawnersRemaining() {
        reminderEnabled = false;
        SpawnerStatus.x -= 1;
    }
    
    public void CheckIfNeedChangePhase() {
        if (ActivePhaseInt != 1) return;
        if (SpawnerStatus.x <= 4 && SpawnerStatus.x > 0) {
            ChangePhase(2);
        } else if (SpawnerStatus.x == 0) {
            ChangePhase(3);
        }
    }


    private void ClearAllBodyguardContainers() {
        ClearSmallBodyguardContainer();
        ClearLargeBodyguardContainer();
    }

    private void ClearSmallBodyguardContainer() {
        for (int i = 0; i < smallBodyguardContainer.transform.childCount; i++) {
            Destroy(smallBodyguardContainer.transform.GetChild(i).gameObject);
        }
    }

    private void ClearLargeBodyguardContainer() {
        for (int i = 0; i < largeBodyguardContainer.transform.childCount; i++) {
            Destroy(largeBodyguardContainer.transform.GetChild(i).gameObject);
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
            case 3:
                readyToSpawn = true;
                activePhase = StartCoroutine(Phase3());
            break;
        }
    }


    private IEnumerator Phase1() {
        yield return new WaitForSeconds(3f);
        while (true) {
            readyToSpawn = false;
            SetActiveMove(Phase1_Summon());
            yield return new WaitUntil(() => activeMove == null);
            yield return new WaitForSeconds(actionDelay);
            yield return new WaitUntil(() => readyToSpawn);
        }
    }


    private IEnumerator Phase1_Summon() {
        List<string> lines = new List<string> {
            "Bodyguards, keep him busy!",
            "I have many bodyguards!",
            "I won't get down!"
        };
        SetSpeech(Util.Choice(lines));
        AudioManager.Instance.PlaySFXAtPointUI(Resources.Load<AudioClip>("Audio/screamattack"), Random.Range(.8f, .9f));

        for (int i = 0; i < 8; i++) {
            Vector3 spawnPos = spawnerContainer.transform.Find("Spawner (" + i + ")").position;
            if (i >= 4) {
                for (int j = 0; j < 4; j++) {
                    StartCoroutine(LaserSpawnEffect1(i, spawnPos));
                }
            } else {
                StartCoroutine(LaserSpawnEffect1(i, spawnPos));    
            }
            yield return new WaitForSeconds(.25f);
        }
        yield return new WaitForSeconds(1f);
        FinishAction();
    }

    private IEnumerator LaserSpawnEffect1(int spawnerNum, Vector3 spawnPos, float delay = 1/12f) {
        yield return new WaitForSeconds(delay);
        GameObject laser = Instantiate(laserSpawnPrefab, spawnPos, Quaternion.identity);
        GameObject spawnedEntity = Instantiate(
            spawnerNum < 4 ? largeBodyguardPrefab : bodyguardPrefab,
            spawnPos,
            Quaternion.identity,
            spawnerNum < 4 ? largeBodyguardContainer.transform : smallBodyguardContainer.transform
        );
        yield return new WaitForSeconds(.5f);
        Destroy(laser);
    }

    private IEnumerator Phase2() {
        // president spawns bodyguards more aggressively
        SetSpeech("Play time is over; time to sleep!");
        animator.Play("IdleNotStonks");
        PlayerHealth.Instance.StartCameraShake();
        yield return new WaitForSeconds(2f);
        PlayerHealth.Instance.EndCameraShake();
        yield return new WaitForSeconds(1f);

        Coroutine periodicSpawn = StartCoroutine(Phase2PeriodicSpawn(2f));
        readyToSpawn = true;
        while (true) {
            readyToSpawn = false;
            if (GetAllAliveSpawners().Count == 0) break;
            SetActiveMove(Phase2_Summon());
            yield return new WaitUntil(() => activeMove == null);
            yield return new WaitForSeconds(actionDelay);
            yield return new WaitUntil(() => readyToSpawn);
        }
        StopCoroutine(periodicSpawn);
        periodicSpawn = null;
        ChangePhase(3);
    }
    
    
    private IEnumerator Phase2_Summon() {
        LargeBodyguardsKilled = 0;
        List<string> lines = new List<string> {
            "I am invincible!",
            "You shall be overwhelmed!",
            "Your staples will never reach me!"
        };
        SetSpeech(Util.Choice(lines));
        AudioManager.Instance.PlaySFXAtPointUI(Resources.Load<AudioClip>("Audio/needreinforcements"), Random.Range(.8f, 1.2f));
        // Spawn 8 large bodyguards randomly to all spawners that are still alive
        for (int i = 0; i < 8; i++) {
            List<GameObject> aliveSpawners = GetAllAliveSpawners();
            if (aliveSpawners.Count == 0) {
                break;}
            StartCoroutine(LaserSpawnEffect2(true, Util.Choice(aliveSpawners).transform.position));
            yield return new WaitForSeconds(.25f);
        }
        
        yield return new WaitForSeconds(1f);
        FinishAction();
    }


    private IEnumerator Phase2PeriodicSpawn(float spawnDelay) {
        while (ActivePhaseInt == 2) {
            List<GameObject> aliveSpawners = GetAllAliveSpawners();
            if (aliveSpawners.Count == 0) {
                break;
            }
            yield return new WaitForSeconds(spawnDelay);
            // choose random spawner that is not destroyed
            
            GameObject chosenAliveSpawner = Util.Choice(aliveSpawners);
            // spawn entity
            print("spawning periodic enemy at " + chosenAliveSpawner.transform.position);
            StartCoroutine(LaserSpawnEffect2(false, chosenAliveSpawner.transform.position));
        }
        yield return null;
    }

    private List<GameObject> GetAllAliveSpawners() {
        List<GameObject> result = new List<GameObject>();
        for (int i = 0; i < spawnerContainer.transform.childCount; i++) {
            if (spawnerContainer.transform.GetChild(i).gameObject.GetComponent<IDamageable>().GetHealthStats().Item1 > 0) {
                result.Add(spawnerContainer.transform.GetChild(i).gameObject);
            }
        }
        return result;
    }
    
    private IEnumerator LaserSpawnEffect2(bool isLarge, Vector3 spawnPos, float delay = 1/12f) {
        yield return new WaitForSeconds(delay);
        GameObject laser = Instantiate(laserSpawnPrefab, spawnPos, Quaternion.identity);
        GameObject spawnedEntity = Instantiate(
            isLarge ? largeBodyguardPrefab : bodyguardPrefab,
            spawnPos,
            Quaternion.identity,
            isLarge ? largeBodyguardContainer.transform : smallBodyguardContainer.transform
        );
        yield return new WaitForSeconds(.5f);
        Destroy(laser);
    }
    

    private IEnumerator Phase3() {
        // president comes out of elevated platform
        animator.Play("IdlePoverty");
        collider.enabled = false;
        SetSpeech("Where are all my bodyguards?!!");
        collider.enabled = true;
        bodyguardJumpTrigger.SetActive(false);
        PlayerHealth.Instance.StartCameraShake();
        yield return new WaitForSeconds(2f);
        PlayerHealth.Instance.EndCameraShake();
        yield return new WaitForSeconds(5f);
        dialogText.fontSize *= 2f;
        PlayerHealth.Instance.StartCameraShake();
        SetSpeech("HELP!!!!!", Mathf.Infinity);
        List<string> panicLines = new List<string> {
            "Audio/nonono",
            "Audio/itgotme"
        };
        while (bossHealth.CurrentHealth > 0) {
            AudioManager.Instance.PlaySFXAtPointUIUntil(Resources.Load<AudioClip>(Util.Choice(panicLines)), Random.Range(0.8f,1.2f), bossHealth);
            yield return new WaitForSeconds(5f);
        }
        
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

    private void ClearSpeech() { dialogText.text = ""; }

    public void Awaken() {
        PlayerInput.Instance.enabled = true;
        bossHealth.SetName("President");
        BossBarRender.Instance.Show();
        // collider.enabled = true;
        SpawnerStatus = new(8, 8);
        LargeBodyguardsKilled = 0;
        ChangePhase(1);
        reminderCoroutine = StartCoroutine(ReminderCoroutine());
    }

    private IEnumerator ReminderCoroutine() {
        TextMeshProUGUI text = reminderPrefab.GetComponent<TextMeshProUGUI>();
        while (true) {
            yield return new WaitForSeconds(1f);
            if (reminderTimer > 60f) {
                // flash the reminder
                text.enabled = true;
                yield return new WaitForSeconds(.5f);
                text.enabled = false;
                yield return new WaitForSeconds(.5f);
                text.enabled = true;
                yield return new WaitForSeconds(.5f);
                text.enabled = false;
                yield return new WaitForSeconds(.5f);
                text.enabled = true;
                yield return new WaitForSeconds(.5f);
                text.enabled = false;
                reminderTimer = 0f;
            }
        }
    }

    public void SetMoveSpeed(float speed) {
        moveSpeed = speed;
        animator.StopPlayback();
        animator.Play(moveSpeed == 0 ? "BossIdleTest" : "BossWalkTest");
    }

    public void OnHurtEnd() {
        if (moveSpeed > 0f) {
            SetMoveSpeed(moveSpeed);
        }
    }
}