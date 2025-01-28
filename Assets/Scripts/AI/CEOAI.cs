using System;
using System.Collections;
using System.Collections.Generic;
using KBCore.Refs;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class CEOAI : MonoBehaviour, IBossAI {
    public static CEOAI Instance;

    [SerializeField, Self] private BossHealth bossHealth;
    [SerializeField, Child] private SpriteRenderer spriteRenderer;
    [SerializeField, Self] private Animator animator;
    [SerializeField, Child] private TextMeshPro dialogText;
    [SerializeField] private Collider2D collider, grabCollider1, grabCollider2;
    [SerializeField] private CinemachineVolumeSettings postProcessing;
    [SerializeField] private VolumeProfile normalProfile, aberrationProfile;
    [SerializeField] private float grabRange = 10f;

    [SerializeField] private float angryPercentage = .4f;
    [SerializeField] private float actionDelay = 1f;


    private bool canGrab = false;

    [SerializeField] private GameObject introBanner, missilePrefab, barrel1, barrel2, quicktimeEvent, eyeGlow, laserSpinnerPrefab, laserSpinnerContainer, sniperSquad;
    private Coroutine activePhase, activeMove;
    private AudioClip bossMusic;
    public int ActivePhaseInt { get; private set; } = 0; // -1 means dead

    private float moveSpeed = 0f;

    private void OnValidate() { this.ValidateRefs(); }

    /* DIRECTOR
     *      Moves                                                           Probability
     * ===== PHASE 1 =====
     * - Save energy                                                       100%
     *      - Shows 6 safe tiles before turning off the lights.
     *      - If the player is not on a safe tile within 5 seconds, player takes 3 damage before lights are turned on again.
     *      - Else, lights turn on and boss is vulnerable for 8 seconds.
     *
     * ===== PHASE 2 ===== (< 50% HP)
     * - Save more energy                                                  100%
     *      - Shows 2 safe tiles before turning off the lights.
     *      - If player is not on safe tile within 3 seconds, player takes 6 damage before lights are turned on again.
     *      - Else, lights turn on and boss is vulnerable for 4 seconds.
     */

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }

        collider.enabled = false;
    }

    private void Start() { bossMusic = Resources.Load<AudioClip>("Audio/Music/BossMusic"); }

    private void Update() {
        if (bossHealth.CurrentHealth == 0 && ActivePhaseInt != -1) {
            ActivePhaseInt = -1;
            PlayerHealth.Instance.EndCameraShake();
            SetSpeech("");
            StopActivePhasesAndMoves();
            LevelManager.Instance.BeatLevel = true;
            enabled = false;
            return;
        }

        if (ActivePhaseInt != 1) return;
        CheckIfNeedChangePhase();
    }

    private void FixedUpdate() {
        if (!collider.enabled) return;
        spriteRenderer.flipX = transform.position.x <= PlayerInput.Instance.gameObject.transform.position.x ? false : true;
        // Grab player if player is in range
        if (PlayerInput.Instance.Frenzy) return;
        if (!canGrab) return;
        if (Vector3.Distance(PlayerInput.Instance.transform.position, transform.position) < grabRange) {
            canGrab = false;
            AudioManager.Instance.PlaySFXAtPointUI(Resources.Load<AudioClip>("Audio/armswing"), Random.Range(0.8f, 1.2f));
            animator.Play("CEOGrab");
        }
    }

    public void CheckIfNeedChangePhase() {
        if (ActivePhaseInt != 1) return;

        if (bossHealth.CurrentHealth < bossHealth.MaxHealth * angryPercentage) {
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
        while (true) {
            List<IEnumerator> moves = new List<IEnumerator> {
                Phase1_SpawnLaserSpinners(),
                Phase1_MissileBarrage(),
            };
            SetActiveMove(Util.Choice(moves));  
            yield return new WaitUntil(() => activeMove == null);
            yield return new WaitForSeconds(actionDelay);
        }
    }


    private IEnumerator Phase1_MissileBarrage() {
        List<string> lines = new List<string> {
            "Think you can win? Think again.",
        };

        SetSpeech(Util.Choice(lines));
        
        for (int i = 0; i < 8; i++) {
            AudioManager.Instance.PlaySFXAtPoint(transform.position, Resources.Load<AudioClip>("Audio/missilefire"),
                Random.Range(.8f, 1.2f));
            // Raycast forward point destination
            GameObject muzzle = barrel1;
            Vector3 direction = (PlayerInput.Instance.gameObject.transform.position - muzzle.transform.position)
                .normalized;
            float centerAngle = (Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg) - 90f;
            float projectileSpeed = Random.Range(15f, 17f);
            GameObject missileObj = Instantiate(missilePrefab, muzzle.transform.position, Quaternion.identity);
            EnemyMissileProjectile projectile = missileObj.GetComponent<EnemyMissileProjectile>();
            missileObj.transform.rotation = Quaternion.Euler(0, 0, centerAngle);
            projectile.SetOwner(gameObject);
            projectile.ProjectileSpeed = projectileSpeed;
            projectile.SetBehavior(ProjectileBehavior.TARGET_GAMEOBJECT, PlayerInput.Instance.gameObject);


            AudioManager.Instance.PlaySFXAtPoint(transform.position, Resources.Load<AudioClip>("Audio/missilefire"),
                Random.Range(.8f, 1.2f));
            // Raycast forward point destination
            muzzle = barrel2;
            direction = (PlayerInput.Instance.gameObject.transform.position - muzzle.transform.position).normalized;
            centerAngle = (Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg) - 90f;
            projectileSpeed = Random.Range(15f, 17f);
            missileObj = Instantiate(missilePrefab, muzzle.transform.position, Quaternion.identity);
            projectile = missileObj.GetComponent<EnemyMissileProjectile>();
            missileObj.transform.rotation = Quaternion.Euler(0, 0, centerAngle);
            projectile.SetOwner(gameObject);
            projectile.ProjectileSpeed = projectileSpeed;
            projectile.SetBehavior(ProjectileBehavior.TARGET_GAMEOBJECT, PlayerInput.Instance.gameObject);
            yield return new WaitForSeconds(.25f);
        }

        yield return new WaitForSeconds(2f);
        FinishAction();
    }

    private IEnumerator Phase1_SpawnLaserSpinners() {
        List<string> lines = new List<string> {
            "Witness my high-tech weapons!",
        };
        
        List<Vector3> spawnedPoints = new List<Vector3>();
        Tilemap tilemap = GameObject.FindWithTag("SpawnArea").GetComponent<Tilemap>();
        BoundsInt bounds = tilemap.cellBounds;

        for (int i = 0; i < 12; i++) {
            Vector3 randPos = GetRandomTilePosition(tilemap, bounds, spawnedPoints);
            spawnedPoints.Add(randPos);
        }
        
        foreach (Vector3 point in spawnedPoints)
        {
            GameObject spinner = Instantiate(laserSpinnerPrefab, point, Quaternion.identity);
            spinner.transform.parent = laserSpinnerContainer.transform;
            AudioManager.Instance.PlaySFXAtPoint(point, Resources.Load<AudioClip>("Audio/laserspinnerdeploy"), Random.Range(.8f, 1.2f));
            yield return new WaitForSeconds(.25f);
        }
        
        
        yield return new WaitForSeconds(.5f);
        
        
        
        FinishAction();
    }

    Vector3Int GetRandomTilePosition(Tilemap tilemap, BoundsInt bounds, List<Vector3> spawnedPoints) {
        for (int attempts = 0; attempts < 100; attempts++) {
            int x = Random.Range(bounds.xMin, bounds.xMax);
            int y = Random.Range(bounds.yMin, bounds.yMax);
            Vector3Int randomPosition = new Vector3Int(x, y, 0);
            if (tilemap.HasTile(randomPosition)) {
                foreach (Vector3 point in spawnedPoints)
                {
                    if (Vector3.Distance(tilemap.CellToWorld(randomPosition), point) > 5f) {
                        return randomPosition;
                    }
                }
            }
        }
        return Vector3Int.zero;
    }


    private IEnumerator Phase2() {
        SetSpeech("ARGH!!!! WASTE OF MONEY!!!");
        PlayerHealth.Instance.StartCameraShake();
        yield return new WaitForSeconds(2f);
        PlayerHealth.Instance.EndCameraShake();

        while (true) {
            yield return new WaitUntil(() => activeMove == null);
            yield return new WaitForSeconds(actionDelay);
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
        bossHealth.SetName("CEO");
        BossBarRender.Instance.Show();
        collider.enabled = true;
        ChangePhase(1);
        Invoke(nameof(StartSniperSquad), 3f);
        Invoke(nameof(EnableCanGrab), 3f);
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

    public void CueIntroBanner() {
        introBanner.SetActive(true);
        Invoke(nameof(EnablePlayerControls), 5f);
        AudioManager.Instance.PlayMusic(bossMusic);
    }

    private void StartSniperSquad() {
        sniperSquad.SetActive(true);
    }

    public void EnablePlayerControls() { PlayerInput.Instance.enabled = true; }

    public void StartCameraShake() { PlayerHealth.Instance.StartCameraShake(); }

    public void StopCameraShake() { PlayerHealth.Instance.EndCameraShake(); }

    public void TransformSound() {
        AudioManager.Instance.PlaySFXAtPointUI(Resources.Load<AudioClip>("Audio/transform"), 1f);
    }

    public void WheelsSound() {
        AudioManager.Instance.PlaySFXAtPointUI(Resources.Load<AudioClip>("Audio/wheels" + Random.Range(1, 4)),
            Random.Range(0.8f, 1.2f));
    }

    public void AberrationEffectOn() {
        AudioManager.Instance.PlaySFXAtPointUI(Resources.Load<AudioClip>("Audio/monstergrowl3"), 1f);
        postProcessing.Profile = aberrationProfile;
        Time.timeScale = .33f;
    }

    public void AberrationEffectOff() {
        postProcessing.Profile = normalProfile;
        Time.timeScale = 1f;
    }

    public void EnableGrabCollider1() {
        grabCollider2.enabled = false;
        grabCollider1.enabled = true;
    }

    public void EnableGrabCollider2() {
        grabCollider1.enabled = false;
        grabCollider2.enabled = true;
    }

    public void DisableGrabColliders() {
        grabCollider1.enabled = false;
        grabCollider2.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.layer != 6) return; // if not player layer
        if (other.GetComponent<PlayerInput>().Frenzy) return;
        if (PlayerInput.Instance.IsDodging) return;
        // grab player
        PlayerInput.Instance.enabled = false;
        PlayerInput.Instance.transform.position = new Vector3(0f, 2.18f, 0f);
        PlayerHealth.Instance.ActuallyTakeDamage(2);
        
        // show quicktime event if player still alive
        if (PlayerHealth.Instance.GetHealthStats().Item1 > 0) {
            quicktimeEvent.SetActive(true);    
        }
        
        // disable grab colliders and play CEO grab animation
        DisableGrabColliders();
        animator.Play("CEOGrabbing");
    }

    private void EnableCanGrab() { canGrab = true; }

    private void DisableCanGrab() { canGrab = false; }

    public void EndGrabIfFailed() {
        if (PlayerInput.Instance.enabled) {
            OnPlayerRelease();
        }
    }
    
    public void OnPlayerRelease() {
        animator.Play("CEOMechIdle");
        Invoke(nameof(EnableCanGrab), 3f);
    }

    public void FlipSpriteTrue() {
        spriteRenderer.flipX = true;
    }

    public void FliipSpriteFalse() {
        spriteRenderer.flipX = false;
    }

    public void GlowEyes() {
        eyeGlow.SetActive(true);
    }
    
}