using System;
using System.Collections;
using System.Collections.Generic;
using KBCore.Refs;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class DirectorAI : MonoBehaviour, IBossAI {
    [SerializeField, Self] private BossHealth bossHealth;
    [SerializeField, Child] private SpriteRenderer spriteRenderer;
    [SerializeField, Self] private Animator animator;
    [SerializeField, Child] private TextMeshPro dialogText;
    [SerializeField, Self] private Collider2D collider;

    [SerializeField] private float angryPercentage = .4f;
    [SerializeField] private float actionDelay = 1f;
    [SerializeField] private List<Sprite> sprites;
    /*
     * 0: normal
     * 1: dead
     * 2: angry
     * 3: laughK1
     * 4: laughK2
     * 5: laughK3
     * 6: laughK4
     * 7: jumpScareNormal
     * 8: jumpScareAngry
     */

    private Vector3 TLCorner = new Vector3(-8.48f, 10.48f, 0f);
    private float tileSpacing = 1f;
    private int lightGridWidth = 17, lightGridHeight = 15;
    [SerializeField] private GameObject lightTilePrefab, lightTilesContainer, jumpScareObj;
    [SerializeField] private Light2D globalLight;
    
    private Coroutine activePhase, activeMove;
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
        collider.enabled = false;
    }
    
    private void Update() {
        if (bossHealth.CurrentHealth == 0 && ActivePhaseInt != -1) {
            ActivePhaseInt = -1;
            PlayerHealth.Instance.EndCameraShake();
            SetSpeech("");
            StopActivePhasesAndMoves();
            DeleteAllLightTiles();
            ToggleLights(true);
            spriteRenderer.sprite = sprites[1];
            LevelManager.Instance.BeatLevel = true;
            enabled = false;
            return;
        }
        if (ActivePhaseInt != 1) return;
        CheckIfNeedChangePhase();
    }

    private void FixedUpdate() {
            spriteRenderer.flipX = transform.position.x <= PlayerInput.Instance.gameObject.transform.position.x ? false : true;
            transform.position = Vector3.MoveTowards(transform.position,
                PlayerInput.Instance.gameObject.transform.position, Time.deltaTime * moveSpeed);
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
        spriteRenderer.sprite = sprites[0];
        yield return new WaitForSeconds(3f);
        while (true) {
            SetActiveMove(Phase1_SaveEnergy(5));  
            yield return new WaitUntil(() => activeMove == null);
            yield return new WaitForSeconds(actionDelay);
        }
    }


    private IEnumerator Phase1_SaveEnergy(int numLightTiles) {
        List<string> lines = new List<string> {
            "Lower the energy bill for our company!",
            "I hope you're not afraid of the dark.",
            "Lights out!",
            "Less paychecks, more DARKNESS!"
        };
        HashSet<Vector3> lightTiles = new HashSet<Vector3>();
        int spawned = 0;
        while (spawned < numLightTiles) {
            if (SpawnRandomLightTile(ref lightTiles, true)) {
                spawned++;
            }
        }
        SetSpeech(Util.Choice(lines));
        // laugh animation (alternative b/w 3 and 5)
        AudioManager.Instance.PlaySFXAtPointUI(Resources.Load<AudioClip>("Audio/laughnormal"), Random.Range(0.8f, 1.2f));
        for (int i = 0; i < 5; i++) {
            spriteRenderer.sprite = sprites[3];
            yield return new WaitForSeconds(1 / 6f);
            spriteRenderer.sprite = sprites[5];
            yield return new WaitForSeconds(1 / 6f);
        }

        spriteRenderer.sprite = sprites[0];
        
        
        
        
        yield return new WaitForSeconds(2f);
        AudioManager.Instance.PlaySFXAtPointUI(Resources.Load<AudioClip>("Audio/lightoff"), Random.Range(.8f, 1.2f));
        AudioManager.Instance.PlaySFXAtPointUI(Resources.Load<AudioClip>("Audio/footsteps"), 1.2f, 6f);
        DeleteAllLightTiles();
        ToggleLights(false);

        jumpScareObj.GetComponent<Image>().sprite = sprites[7];
        jumpScareObj.GetComponent<Image>().color = Color.white;
        // spriteRenderer.sprite = sprites[4];
        yield return new WaitForSeconds(5f);
        if (!PlayerStandingInLight(lightTiles)) {
            jumpScareObj.SetActive(true);
            jumpScareObj.GetComponent<UIShake>().TriggerShake();
            AudioManager.Instance.PlaySFXAtPointUI(Resources.Load<AudioClip>("Audio/scream"), Random.Range(0.8f,1.2f));
            yield return new WaitForSeconds(3f);
            PlayerHealth.Instance.ActuallyTakeDamage(3);
            jumpScareObj.SetActive(false);     
        }
        AudioManager.Instance.PlaySFXAtPointUI(Resources.Load<AudioClip>("Audio/lighton"), Random.Range(.8f, 1.2f));
        ToggleLights(true);
        yield return new WaitForSeconds(1f);
        
       
        FinishAction();
    }
    
    

    private void ToggleLights(bool val) {
        globalLight.intensity = val ? 1f : 0f;
        collider.enabled = val;
    }

    private bool PlayerStandingInLight(HashSet<Vector3> lightTiles) {
        foreach (Vector3 point in lightTiles) {
            Vector3 posInGame = new Vector3(TLCorner.x + point.x, TLCorner.y - point.y, 0);
            if (Vector3.Distance(PlayerInput.Instance.transform.position, posInGame) < 1f) {
                return true;
            }
        }
        return false;
    }
    
    private bool SpawnRandomLightTile(ref HashSet<Vector3> lightTiles, bool blinking = false) {
        Vector3 chosenPosition = new Vector3(Random.Range(0, lightGridWidth), Random.Range(0, lightGridHeight), 0);
        if (chosenPosition.x >= 9 && chosenPosition.x <= 11 && chosenPosition.y >= 4 && chosenPosition.y <= 7) return false;
        if (lightTiles.Contains(chosenPosition)) return false;
        
        lightTiles.Add(chosenPosition);
        print("chosen position: " + chosenPosition.ToString());
        GameObject lightTile = Instantiate(lightTilePrefab, new Vector3(TLCorner.x + chosenPosition.x, TLCorner.y - chosenPosition.y, 0), Quaternion.identity, lightTilesContainer.transform);
        if (blinking) {
            StartCoroutine(BlinkLight(lightTile.transform.Find("Light").GetComponent<Light2D>(), 3));
        }
        return true;
    }

    private IEnumerator BlinkLight(Light2D light, int numTimes) {
        for (int i = 0; i < numTimes; i++) {
            if (light == null) yield break;
            light.enabled = false;
            yield return new WaitForSeconds(.2f);
            if (light == null) yield break;
            light.enabled = true;
            yield return new WaitForSeconds(.2f);
        }
    }

    private void DeleteAllLightTiles() {
        for (int i = 0; i < lightTilesContainer.transform.childCount; i++) {
            Destroy(lightTilesContainer.transform.GetChild(i).gameObject);
        }
    }

    private IEnumerator Phase2() {
        // make boss invicible for short while
        ToggleLights(true);
        DeleteAllLightTiles();
        dialogText.fontSize *= 1.5f;
        SetSpeech("ARGH!!!! WASTE OF MONEY!!!");
        spriteRenderer.sprite = sprites[2];
        PlayerHealth.Instance.StartCameraShake();
        yield return new WaitForSeconds(2f);
        PlayerHealth.Instance.EndCameraShake();
        
        while (true) {
            SetActiveMove(Phase2_SaveMoreEnergy(1));
            yield return new WaitUntil(() => activeMove == null);
            yield return new WaitForSeconds(actionDelay);
        }
    }

    private IEnumerator Phase2_SaveMoreEnergy(int numLightTiles) {
        List<string> lines = new List<string> {
            "GOOD NIGHT!",
            "YOU DON'T DESERVE LIGHT, SLACKER!",
            "EMBRACE THE DARKNESS, COWARD!",
        };
        HashSet<Vector3> lightTiles = new HashSet<Vector3>();
        int spawned = 0;
        while (spawned < numLightTiles) {
            if (SpawnRandomLightTile(ref lightTiles, true)) {
                spawned++;
            }
        }
        SetSpeech(Util.Choice(lines));
        // laugh animation (alternative b/w 4 and 6)
        AudioManager.Instance.PlaySFXAtPointUI(Resources.Load<AudioClip>("Audio/laughangry"), Random.Range(0.8f, 1.2f));
        for (int i = 0; i < 5; i++) {
            spriteRenderer.sprite = sprites[4];
            yield return new WaitForSeconds(1 / 6f);
            spriteRenderer.sprite = sprites[6];
            yield return new WaitForSeconds(1 / 6f);
        }
        spriteRenderer.sprite = sprites[2];
        yield return new WaitForSeconds(1f);
        AudioManager.Instance.PlaySFXAtPointUI(Resources.Load<AudioClip>("Audio/lightoff"), Random.Range(.8f, 1.2f));
        AudioManager.Instance.PlaySFXAtPointUI(Resources.Load<AudioClip>("Audio/footsteps"), 2f, 5f);
        ToggleLights(false);
        DeleteAllLightTiles();
        
        // spriteRenderer.sprite = sprites[4];

        jumpScareObj.GetComponent<Image>().sprite = sprites[8];
        jumpScareObj.GetComponent<Image>().color = Color.red;
        yield return new WaitForSeconds(3f);
        if (!PlayerStandingInLight(lightTiles)) {
            AudioManager.Instance.PlaySFXAtPointUI(Resources.Load<AudioClip>("Audio/jumpscare"), Random.Range(1.2f,1.4f));
            jumpScareObj.SetActive(true);
            jumpScareObj.GetComponent<UIShake>().TriggerShake();
            yield return new WaitForSeconds(3f);
            PlayerHealth.Instance.ActuallyTakeDamage(6);
            jumpScareObj.SetActive(false);     
        }
        AudioManager.Instance.PlaySFXAtPointUI(Resources.Load<AudioClip>("Audio/lighton"), Random.Range(.8f, 1.2f));
        ToggleLights(true);
        yield return new WaitForSeconds(.5f);
        
       
        FinishAction();
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
        bossHealth.SetName("Director");
        BossBarRender.Instance.Show();
        collider.enabled = true;
        ChangePhase(1);
    }
    
    public void SetMoveSpeed(float speed) {
        moveSpeed = speed;
        animator.StopPlayback();
        animator.Play(moveSpeed == 0 ? "BossIdleTest" :"BossWalkTest");
    }

    public void OnHurtEnd() {
        if (moveSpeed > 0f) {
            SetMoveSpeed(moveSpeed);
        }
    }

}