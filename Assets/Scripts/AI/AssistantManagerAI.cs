using System;
using System.Collections;
using System.Collections.Generic;
using KBCore.Refs;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class AssistantManagerAI : MonoBehaviour, IBossAI {
    [SerializeField, Self] private BossHealth bossHealth;
    [SerializeField, Child] private SpriteRenderer spriteRenderer;
    [SerializeField, Self] private Animator animator;
    [SerializeField, Child] private TextMeshPro dialogText;
    [SerializeField, Self] private Collider2D collider;
    [SerializeField, Self] private NavMeshAgent agent;

    [SerializeField] private float actionDelay = 1f;
    [SerializeField] private List<Sprite> sprites;

    private Coroutine activePhase, activeMove;
    public int ActivePhaseInt { get; private set; } = 0; // -1 means dead

    private GameObject coffeeCupProjectile;

    private Vector2
        coffeeMachineStatus = new Vector2(4, 4); // x: How many machines remain. y: how many machines at the start
    
    private void OnValidate() { this.ValidateRefs(); }

    /* ASSISTANT MANAGER
     * Mode                  Lines
     *  Non-Crisis            I love coffee!, Try my coffee!, No coffee no life!, Life will be much better with coffee!, Feel the coffee burn!, I love coffee machines!
     *  Crisis                I need more coffee!, How dare you destroy my coffee machines! I can't live without coffee! Feel my caffeinated wrath!
     *
     *      Moves                                                           Probability
     * - Coffee Belch                                                       0% (100% if all broken)
     *      - Shoots a wide arc of scalding coffee, burning the player on contact. Coffee arc dissipates with a timeout.
     * - Coffee Throw                                                      100% (0% if all broken, 67% if health not full and not all broken)
     *      - Throws 3 cups of scalding coffee one by one, creating circular AOEs that deal damage to player over time once touched.
     * - Drink Coffee (if coffee machines available)                        33% (0% if all broken or health is full)
     *      - Drinks a cup of coffee, restoring his health by 33%.
     */

    private void Awake() {
        coffeeCupProjectile = Resources.Load("Prefabs/CoffeeCupProjectile") as GameObject;
        CoffeeMachineHealth.OnTakeDamage += CheckIfCoffeeMachineDestroyed;
        collider.enabled = false;
    }
    
    private void Start() {
        agent.updateUpAxis = false;
        agent.updateRotation = false;
        agent.speed = 0f;
    }

    private void OnDestroy() { CoffeeMachineHealth.OnTakeDamage -= CheckIfCoffeeMachineDestroyed; }
    
    private void Update() {
        if (bossHealth.CurrentHealth == 0 && ActivePhaseInt != -1) {
            ActivePhaseInt = -1;
            PlayerHealth.Instance.EndCameraShake();
            SetSpeech("");
            StopActivePhasesAndMoves();
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
        if (ActivePhaseInt == 2) {
            agent.SetDestination(PlayerInput.Instance.transform.position);
        }
    }

    public void CheckIfNeedChangePhase() {
        if (ActivePhaseInt != 1) return;
        
        if (coffeeMachineStatus.x == 0 && bossHealth.CurrentHealth < bossHealth.MaxHealth / 2) {
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
        while (true) {
            if (bossHealth.CurrentHealth == bossHealth.MaxHealth) {
                // Only do coffee throw
                SetActiveMove(Phase1_CoffeeThrow(3, 1f));
                yield return new WaitUntil(() => activeMove == null);
            } else {
                // Have chance to either drink and heal or just coffee throw
                if (coffeeMachineStatus.x != 0) {
                    SetActiveMove(Random.Range(0, 3) == 0 ? Phase1_CoffeeThrow(3, 1f) : Phase1_DrinkCoffee());    
                } else {
                    SetActiveMove(Phase1_CoffeeThrow(9, .15f));
                }
                yield return new WaitUntil(() => activeMove == null);
            }

            yield return new WaitForSeconds(actionDelay);
        }
    }


    private IEnumerator Phase1_CoffeeThrow(int numThrows, float delay) {
        List<string> coffeeThrowLines = new List<string> {
            "Feel the scalding coffee!",
            "Try my coffee!",
            "Feel the coffee burn!",
            "Did you know that coffee is acidic?"
        };
        SetSpeech(Util.Choice(coffeeThrowLines));
        for (int i = 0; i < numThrows; i++) {
            ThrowCoffee();     
            yield return new WaitForSeconds(.5f);
        }
        FinishAction();
    }

    private IEnumerator Phase1_DrinkCoffee() {
        List<string> drinkCoffeeLines = new List<string>{"I love coffee machines, don't destroy them!", "Coffee is love, coffee is life!", "Coffee keeps me alive!"};
        
        SetSpeech(Util.Choice(drinkCoffeeLines));
        AudioManager.Instance.PlaySFXAtPoint(transform.position, Resources.Load<AudioClip>("Audio/drinkliquid"), Random.Range(.8f, 1.2f));
        spriteRenderer.sprite = sprites[2];
        yield return new WaitForSeconds(4f);
        spriteRenderer.sprite = coffeeMachineStatus.x != 0 ? sprites[0] : sprites[3];
        yield return new WaitForSeconds(.5f);
        bossHealth.Recover(bossHealth.MaxHealth);
        animator.Play("BossHeal");
        yield return new WaitForSeconds(1f);

        FinishAction();
    }

    private IEnumerator Phase2() {
        List<string> angryLines = new List<string> {
            "$%@^@@$%!!","(*&^)&!!!","!!@$$%!@#$%%&%!"
        };
        SetSpeech("...");
        AudioManager.Instance.PlaySFXAtPointUI(Resources.Load<AudioClip>("Audio/growl"), Random.Range(.8f, 1.2f));
        yield return new WaitForSeconds(2.5f);
        dialogText.fontSize *= 2;
        spriteRenderer.sprite = sprites[4];
        
        while (true) {
            PlayerHealth.Instance.StartCameraShake();
            SetSpeech(Util.Choice(angryLines), 5f);
            SetMoveSpeed(4f);
            for (int i = 0; i < 3; i++) {
                SetActiveMove(Phase2_CoffeeBelch());
                yield return new WaitUntil(() => activeMove == null);
                yield return new WaitForSeconds(actionDelay / 3f);     
            }
            PlayerHealth.Instance.EndCameraShake();
            SetMoveSpeed(0f);
            yield return new WaitForSeconds(actionDelay);
        }
    }

    private IEnumerator Phase2_CoffeeBelch() {
        GameObject belchPrefab = Resources.Load("Prefabs/CoffeeBelchProjectile") as GameObject;
        AudioManager.Instance.PlaySFXAtPointUI(Resources.Load<AudioClip>("Audio/belch"), Random.Range(.8f, 1.2f), 4f);
        for (int i = 0; i < 8; i++) {
            GameObject belch = Instantiate(belchPrefab, transform.position, Quaternion.identity);
            Vector3 direction = (PlayerInput.Instance.gameObject.transform.position - transform.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            belch.transform.rotation = Quaternion.Euler(0, 0, angle - 90f + Random.Range(-60f, 60f));
            belch.GetComponent<EnemyCoffeeBelchProjectile>().ProjectileSpeed = Random.Range(7f, 8f);
            yield return new WaitForSeconds(.175f);
        }

        FinishAction();
    }


    private void CheckIfCoffeeMachineDestroyed(int health, int maxHealth) {
        List<string> machineDestroyedLines = new List<string> {
            "How dare you destroy my coffee machine!", "No! don't deprive me of caffeine!", "That's several thousand off your paycheck!",
            "I'll destroy you before you destroy my coffee machines!"
        };
        if (health == 0) {
            coffeeMachineStatus.x -= 1;
            if (coffeeMachineStatus.x == 0) {
                SetSpeech("You will pay for this!");
            } else {
                SetSpeech(Util.Choice(machineDestroyedLines));
                spriteRenderer.sprite = sprites[3];
            }
        }
    }

    private void ThrowCoffee() {
        GameObject clone = Instantiate(coffeeCupProjectile, transform.position, Quaternion.identity);
        Vector3 direction = (PlayerInput.Instance.gameObject.transform.position - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        clone.transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
        clone.GetComponent<Projectile>().SetBehavior(ProjectileBehavior.TARGET_POSITION,
            PlayerInput.Instance.gameObject.transform.position + new Vector3(
                Random.Range(-2f, 2f),
                Random.Range(-2f, 2f),
                0f
            ));
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
        bossHealth.SetName("Assistant Manager");
        BossBarRender.Instance.Show();
        collider.enabled = true;
        ChangePhase(1);
    }

    public void SetMoveSpeed(float speed) {
        agent.speed = speed;
        animator.StopPlayback();
        animator.Play(agent.speed == 0 ? "BossIdleTest" :"BossWalkTest");
    }
    
    public void OnHurtEnd() {
        if (agent.speed > 0f) {
            SetMoveSpeed(agent.speed);
        }
    }

}