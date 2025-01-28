using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using KBCore.Refs;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class SuperintendentAI : MonoBehaviour, IBossAI {
    [SerializeField, Self] private BossHealth bossHealth;
    [SerializeField, Child] private SpriteRenderer spriteRenderer;
    [SerializeField, Self] private Animator animator;
    [SerializeField, Child] private TextMeshPro dialogText;
    [SerializeField, Self] private Collider2D collider;

    [SerializeField] private float actionDelay = 1f;

    [SerializeField] private List<Sprite> sprites;
    /*
     * 0: dead
     * 1: normal
     * 2: angry
     */

    [SerializeField] private GameObject choiceButtons, barrier, thumbtackPrefab, thumbtacksContainer;
    private int numQuestionsAsked = 0;
    private Vector3 TLCorner = new Vector3(-9.48f, 11.48f, 0f);
    private int gridWidth = 20, gridHeight = 17;

    private Coroutine activePhase, activeMove;
    public int ActivePhaseInt { get; private set; } = 0; // -1 means dead

    private float moveSpeed = 0f;

    private void OnValidate() { this.ValidateRefs(); }

    /* SUPERINTENDENT
     *      Moves                                                           Probability
     * ===== PHASE 1 =====
     * - Clipboard throw                                                    N/A, goes in the pattern of clipboard/fans for two moves, and then a question
     *      - Throws 5 clipboards in the shape of a fan.
     * - Thumbtack scatter
     *      - Throws 8 thumbtacks onto a random area within the battle area, lasting until anything besides projectiles touches it.
     *
     * ===== PHASE 2 ===== (< 50% HP)
     * - Angry clipboard throw                                              N/A, goes in the pattern of clipboard/fans for three moves, and then a question
     *      - Throws 10 large clipboards in the shape of a fan.
     *      - Throws 16 thumbtacks onto a random area within the battle area, lasting until anything besides projectiles touch it.
     */

    private void Awake() { collider.enabled = false; }

    private void Update() {
        if (bossHealth.CurrentHealth == 0 && ActivePhaseInt != -1) {
            ActivePhaseInt = -1;
            PlayerHealth.Instance.EndCameraShake();
            SetSpeech("");
            StopActivePhasesAndMoves();
            spriteRenderer.sprite = sprites[0];
            ClearThumbtacksContainer();
            LevelManager.Instance.BeatLevel = true;
            enabled = false;
            return;
        }

        if (ActivePhaseInt != 1) return;
        CheckIfNeedChangePhase();
    }

    private void FixedUpdate() {
        spriteRenderer.flipX =
            transform.position.x <= PlayerInput.Instance.gameObject.transform.position.x ? false : true;
        transform.position = Vector3.MoveTowards(transform.position,
            PlayerInput.Instance.gameObject.transform.position, Time.deltaTime * moveSpeed);
    }

    public void CheckIfNeedChangePhase() {
        if (ActivePhaseInt != 1) return;

        if (bossHealth.CurrentHealth < bossHealth.MaxHealth / 2) {
            ChangePhase(2);
        }
    }


    private void ClearThumbtacksContainer() {
        for (int i = 0; i < thumbtacksContainer.transform.childCount; i++) {
            Destroy(thumbtacksContainer.transform.GetChild(i).gameObject);
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
        spriteRenderer.sprite = sprites[1];
        yield return new WaitForSeconds(3f);
        while (true) {
            for (int i = 0; i < 2; i++) {
                SetActiveMove(Random.Range(0,2) == 0 ? Phase1_ClipboardFan(5, 15f) : Phase1_ThumbtackScatter(16));
                yield return new WaitUntil(() => activeMove == null);
                yield return new WaitForSeconds(actionDelay);
            }
            
            int timeAllowed = (int)(25f / Mathf.Pow(numQuestionsAsked + 1f, .2f)) - 5; 
            SetActiveMove(Phase1_Questionaire(timeAllowed));  

            // SetActiveMove(Phase1_ClipboardFan(5, 15f));
            yield return new WaitUntil(() => activeMove == null);
            yield return new WaitForSeconds(actionDelay);
        }
    }


    private IEnumerator Phase1_ClipboardFan(int projectilePairs, float spreadAngle) {
        List<string> lines = new List<string> {
            "Clipboards are great weapons!",
            "You are not doing your job!",
            "Try dodging this!"
        };

        SetSpeech(Util.Choice(lines));
        
        GameObject clipboardProjectile = Resources.Load<GameObject>("Prefabs/ClipboardProjectile");

        for (int j = 0; j < 2; j++) {
            Vector3 direction = (PlayerInput.Instance.gameObject.transform.position - transform.position).normalized;
            float centerAngle = (Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg) - 90f;
            float projectileSpeed = Random.Range(4f, 7f);
            GameObject clipboard = Instantiate(clipboardProjectile, transform.position, Quaternion.identity);
            clipboard.transform.rotation = Quaternion.Euler(0, 0, centerAngle);
            clipboard.GetComponent<EnemyClipboardProjectile>().ProjectileSpeed = projectileSpeed;

            for (int i = 0; i < projectilePairs; i++) {
                clipboard = Instantiate(clipboardProjectile, transform.position, Quaternion.identity);
                clipboard.transform.rotation = Quaternion.Euler(0, 0, centerAngle - spreadAngle * (i + 1));
                clipboard.GetComponent<EnemyClipboardProjectile>().ProjectileSpeed = projectileSpeed;

                clipboard = Instantiate(clipboardProjectile, transform.position, Quaternion.identity);
                clipboard.transform.rotation = Quaternion.Euler(0, 0, centerAngle + spreadAngle * (i + 1));
                clipboard.GetComponent<EnemyClipboardProjectile>().ProjectileSpeed = projectileSpeed;
            }


            yield return new WaitForSeconds(1f);
        }
        
       


        FinishAction();
    }

    private IEnumerator Phase1_ThumbtackScatter(int numThumbtacks) {
        List<string> lines = new List<string> {
            "You better watch your step!",
            "Hahaha! Everything is going according to plan.",
            "I'm going to have fun seeing you struggle and die."
        };

        SetSpeech(Util.Choice(lines));

        for (int i = 0; i < numThumbtacks; i++) {
            GameObject thumbtack = Instantiate(thumbtackPrefab, transform.position, Quaternion.identity, thumbtacksContainer.transform);
            
            int numJumps = Random.Range(1, 4);
            float tweenTimePerJump = Random.Range(.5f, 1f);
            float tweenTime = tweenTimePerJump * numJumps;
            thumbtack.GetComponent<Thumbtack>().StartSetTimer(tweenTime);
            // find a target position
            Vector3 targetPos = TLCorner + new Vector3(Random.Range(0f, gridWidth), -Random.Range(0f, gridHeight), 0f);
            thumbtack.transform.DOJump(targetPos, Random.Range(2f,3f), numJumps, tweenTime, false).SetEase(Ease.OutBounce);
        }

        yield return new WaitForSeconds(1f);


        FinishAction();
    }

    private IEnumerator Phase1_Questionaire(int timeAllowed) {
        List<Tuple<string, bool>> questions = new List<Tuple<string, bool>> {
            new("Is Joe awake?", true),
            new("Is Bob here?", true),
            new("Is Sam closest to me?", false),
            new("Is Alex asleep?", true),
            new("Is Bill slacking off?", true),
            new("Is John slacking off?", false),
            new("Is Carol here?", false),
            new("Is Brian in the same group as Steven?", false),
            new("Is David awake?", true),
            new("Is Bill asleep?", true),
            new("Is Daniel awake?", true),
            new("Is Richard in the top left group?", true),
            new("Is Susan in the bottom right group?", false),
            new("Is Stacy awake?", true),
            new("Is John closest to the elevator?", true),
            new("Is Thomas here?", false),
            new("Is Miles here?", false),
            new("Is Barbara here?", true),
            new("Is Andrew here?", true),
        };

        Tuple<string, bool> selectedQuestion = Util.Choice(questions);
        choiceButtons.SetActive(true);
        int timer = timeAllowed;
        while (timer > 0) {
            timer--;
            dialogText.text = selectedQuestion.Item1 + "\n" + timer;
            yield return new WaitForSeconds(1f);
        }

        dialogText.text = "time's up!";
        yield return new WaitForSeconds(2f);
        dialogText.text = "you...";
        yield return new WaitForSeconds(1f);
        if (GetPlayerChoice() == -1) {
            dialogText.text = "didn't even make a choice! Stay on the button!";
            PlayerHealth.Instance.ActuallyTakeDamage(1);
            AudioManager.Instance.PlaySFXAtPointUI(Resources.Load<AudioClip>("Audio/wrong"), 1f);
        } else if (CheckPlayerAnswer(GetPlayerChoice() == 0 ? false : true, selectedQuestion.Item2) == false) {
            dialogText.text = "are wrong! nice try though.";
            PlayerHealth.Instance.ActuallyTakeDamage(1);
            AudioManager.Instance.PlaySFXAtPointUI(Resources.Load<AudioClip>("Audio/wrong"), 1f);
        } else {
            // answer was correct!
            dialogText.text = "are correct! i'll let you hit me for a bit.";
            AudioManager.Instance.PlaySFXAtPointUI(Resources.Load<AudioClip>("Audio/correct"), 1f);
            barrier.SetActive(false);
            collider.enabled = true;
        }

        choiceButtons.SetActive(false);
        yield return new WaitForSeconds(5f);
        collider.enabled = false;
        barrier.SetActive(true);
        yield return new WaitForSeconds(1f);
        numQuestionsAsked++;
        FinishAction();
    }

    private int GetPlayerChoice() {
        if (choiceButtons.transform.Find("No").GetComponent<Light2D>().intensity > 0f) {
            return 0;
        } else if (choiceButtons.transform.Find("Yes").GetComponent<Light2D>().intensity > 0f) {
            return 1;
        }

        return -1;
    }

    private bool CheckPlayerAnswer(bool answer, bool correctAnswer) { return answer == correctAnswer; }


    private IEnumerator Phase2() {
        spriteRenderer.sprite = sprites[2];
        barrier.SetActive(true);
        collider.enabled = false;
        SetSpeech("Those staples hurt!! No more fair and square!");
        PlayerHealth.Instance.StartCameraShake();
        yield return new WaitForSeconds(2f);
        PlayerHealth.Instance.EndCameraShake();
        // spriteRenderer.sprite = sprites[5];
        yield return new WaitForSeconds(1f);

        while (true) {
            for (int i = 0; i < 2; i++) {
                SetActiveMove(Random.Range(0,2) == 0 ? Phase2_ClipboardFan(6, 10f) : Phase2_ThumbtackScatter(48));
                yield return new WaitUntil(() => activeMove == null);
                yield return new WaitForSeconds(actionDelay);
            }

            int timeAllowed = (int)(25f / Mathf.Pow(numQuestionsAsked + 1f, .2f)) - 5;
            SetActiveMove(Phase2_Questionaire((int)(timeAllowed / 2f)));
            yield return new WaitUntil(() => activeMove == null);
            yield return new WaitForSeconds(actionDelay);
        }
    }


    
    
    private IEnumerator Phase2_ClipboardFan(int projectilePairs, float spreadAngle) {
        List<string> lines = new List<string> {
            "Take this!",
            "I don't want to die!",
            "I'll kill you before you kill me!"
        };

        SetSpeech(Util.Choice(lines));
        
        GameObject clipboardProjectile = Resources.Load<GameObject>("Prefabs/ClipboardProjectile");

        for (int j = 0; j < 5; j++) {
            Vector3 direction = (PlayerInput.Instance.gameObject.transform.position - transform.position).normalized;
            float centerAngle = (Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg) - 90f;
            float projectileSpeed = Random.Range(4f, 7f);
            GameObject clipboard = Instantiate(clipboardProjectile, transform.position, Quaternion.identity);
            clipboard.transform.rotation = Quaternion.Euler(0, 0, centerAngle);
            clipboard.GetComponent<EnemyClipboardProjectile>().ProjectileSpeed = projectileSpeed;

            for (int i = 0; i < projectilePairs; i++) {
                clipboard = Instantiate(clipboardProjectile, transform.position, Quaternion.identity);
                clipboard.transform.rotation = Quaternion.Euler(0, 0, centerAngle - spreadAngle * (i + 1));
                clipboard.GetComponent<EnemyClipboardProjectile>().ProjectileSpeed = projectileSpeed;

                clipboard = Instantiate(clipboardProjectile, transform.position, Quaternion.identity);
                clipboard.transform.rotation = Quaternion.Euler(0, 0, centerAngle + spreadAngle * (i + 1));
                clipboard.GetComponent<EnemyClipboardProjectile>().ProjectileSpeed = projectileSpeed;
            }


            yield return new WaitForSeconds(.5f);
        }
        
       


        FinishAction();
    }

    private IEnumerator Phase2_ThumbtackScatter(int numThumbtacks) {
        List<string> lines = new List<string> {
            "You're in for a world of hurt!",
            "I'm not holding back!",
            "The floor is lava!"
        };

        SetSpeech(Util.Choice(lines));

        for (int i = 0; i < numThumbtacks; i++) {
            GameObject thumbtack = Instantiate(thumbtackPrefab, transform.position, Quaternion.identity, thumbtacksContainer.transform);
            
            int numJumps = Random.Range(1, 4);
            float tweenTimePerJump = Random.Range(.5f, 1f);
            float tweenTime = tweenTimePerJump * numJumps;
            thumbtack.GetComponent<Thumbtack>().StartSetTimer(tweenTime);
            // find a target position
            Vector3 targetPos = TLCorner + new Vector3(Random.Range(0f, gridWidth), -Random.Range(0f, gridHeight), 0f);
            thumbtack.transform.DOJump(targetPos, Random.Range(2f,3f), numJumps, tweenTime, false).SetEase(Ease.OutBounce);
        }

        yield return new WaitForSeconds(1f);


        FinishAction();
    }
    
    
    
    
    private IEnumerator Phase2_Questionaire(int timeAllowed) {
        List<Tuple<string, bool>> questions = new List<Tuple<string, bool>> {
            new("Do you want to get promoted?", false),
            new("Am I the best boss ever?", true),
            new("Do you want to go home?", false),
            new("Is money all I want?", true),
            new("Do I care about you folks?", false),
            new("Do you think you can beat me?", false),
            new("Do you think you are going to lose?", true),
            new("Am I a genius?", true),
            new("Are you allowed to disobey me?", false),
            new("Are you in a dead-end job?", true),
            new("Will you die here?", true),
            new("Are you satisfied with your job?", true),
            new("Will you work here until you rot in your chair?", true),
            new("Will you get to the next floor?", false),
            new("Is my hair looking fine?", true),
            new("Am I nicer than the other bosses?", true),
            new("Are you my pets?", true),
            new("Do you want sunlight?", false),
            new("Am I in a great mood?", true),
            new("Is my salary high enough?", false),
            new("Is this a trick question?", false),
            new("Is the weather good outside?", false),
        };
        Tuple<string, bool> selectedQuestion = Util.Choice(questions);
        choiceButtons.SetActive(true);
        int timer = timeAllowed;
        while (timer > 0) {
            timer--;
            dialogText.text = selectedQuestion.Item1 + "\n" + timer;
            yield return new WaitForSeconds(1f);
        }

        dialogText.text = "time's up!";
        yield return new WaitForSeconds(2f);
        dialogText.text = "you...";
        yield return new WaitForSeconds(1f);
        if (GetPlayerChoice() == -1) {
            dialogText.text = "didn't even make a choice! Stay on the button!";
            PlayerHealth.Instance.ActuallyTakeDamage(1);
            AudioManager.Instance.PlaySFXAtPointUI(Resources.Load<AudioClip>("Audio/wrong"), 1f);
        } else if (CheckPlayerAnswer(GetPlayerChoice() == 0 ? false : true, selectedQuestion.Item2) == false) {
            dialogText.text = "are wrong. don't lie to yourself!";
            PlayerHealth.Instance.ActuallyTakeDamage(1);
            AudioManager.Instance.PlaySFXAtPointUI(Resources.Load<AudioClip>("Audio/wrong"), 1f);
        } else {
            // answer was correct!
            dialogText.text = "are correct! aaahhh, stay away from me!!!.";
            AudioManager.Instance.PlaySFXAtPointUI(Resources.Load<AudioClip>("Audio/correct"), 1f);
            barrier.SetActive(false);
            collider.enabled = true;
        }

        choiceButtons.SetActive(false);
        yield return new WaitForSeconds(2.5f);
        collider.enabled = false;
        barrier.SetActive(true);
        yield return new WaitForSeconds(1f);
        numQuestionsAsked++;
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

    private void ClearSpeech() { dialogText.text = ""; }

    public void Awaken() {
        PlayerInput.Instance.enabled = true;
        bossHealth.SetName("Superintendent");
        BossBarRender.Instance.Show();
        // collider.enabled = true;
        ChangePhase(1);
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