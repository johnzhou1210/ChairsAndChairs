using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using KBCore.Refs;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class PlayerInput : MonoBehaviour {
    public static PlayerInput Instance;

    [Header("Validated Refs")] [SerializeField, Self]
    private Animator animator;

    [SerializeField, Self] private BoxCollider2D frenzyCollider;

    [SerializeField, Self] private PlayerHealth healthManager;

    [Header("Input Manager Linking")] [SerializeField]
    private InputActionReference playerMovement;

    [SerializeField] private InputActionReference mouseMovement;
    [SerializeField] private InputActionReference mouseClick;
    [SerializeField] private InputActionReference reloadPress;
    [SerializeField] private InputActionReference dodgePress;
    [SerializeField] private InputActionReference frenzyPress;

    [Header("Player Movement")] [SerializeField]
    private float movementSpeed = 5f;

    [SerializeField] private float dodgeDuration = 1f, dodgeSpeed = 1f;

    public bool IsDodging { get; private set; } = false;
    private float currentDodgeCooldownTimer = 0f;


    [Header("Player Shooting")]
    [SerializeField] private int clipSize = 20;
    [SerializeField] private float reloadDuration = 3f, fastReloadDuration = 1f;

    public static event Action<float> OnWeaponReload;
    public static event Action<int, int> OnUpdateAmmo;

    private Vector2 movementInput = Vector2.zero;
    private Vector2 mousePositionInput = Vector2.zero;
    private int ammoLeft;
    private bool isReloading = false;
    private float currentShootCooldownTimer = 0f;


    [Header("Other Variables")] private SpriteRenderer spriteRenderer;
    [SerializeField] private List<Sprite> sprites;
    private GameObject gun, gunPivot, barrel, projectilePrefab, laserLine;
    public bool Frenzy { get; private set; } = false;
    [SerializeField] private int frenzyDamage = 5;
    [SerializeField] private float frenzyDuration = 10f;


    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }
    }

    private void OnValidate() { this.ValidateRefs(); }

    
    void Start() {
        InitializeCursor();

        spriteRenderer = transform.Find("Sprite").GetComponent<SpriteRenderer>();
        gunPivot = transform.Find("GunPivot").gameObject;
        gun = gunPivot.transform.Find("Gun").gameObject;
        barrel = gun.transform.Find("Barrel").gameObject;
        laserLine = gun.transform.Find("Laser").gameObject;
        projectilePrefab = (GameObject)Resources.Load("Prefabs/StapleProjectile");
        ammoLeft = clipSize;
    }


    void Update() {
        if (healthManager.CurrentHealth <= 0) {
            return;
        }
        
        ReadMovementInput();
        ListenForReloadInput();
        ListenForFireInput();
        ListenForDodgeInput();
        ListenForFrenzyInput();
    }

    private void FixedUpdate() {
        if (healthManager.CurrentHealth <= 0) {
            return;
        }

        HandleWASD();
        RespondToDodgeInput();
        GunRotationFollowMouse();

        
        
        if (Frenzy) {
            // transform.position += new Vector3(Random.insideUnitCircle.x * 1f * Time.fixedDeltaTime,
            //     Random.insideUnitCircle.y * 1f * Time.fixedDeltaTime, 0f);    
            Vector3 direction = new Vector3(Random.insideUnitCircle.x / 4f, Random.insideUnitCircle.y / 4f, 0).normalized;
            if (!Physics2D.Raycast(transform.position, direction, 8f, LayerMask.GetMask("Barrier") )) {
                transform.position += new Vector3(Random.insideUnitCircle.x / 4f, Random.insideUnitCircle.y / 4f, 0f);
            } else {
                print("hit something");
            }
            
        }
        
    }

    public Vector2 GetAmmo() {
        return new(ammoLeft, clipSize);
    }

    public bool GetIsReloading() {
        return isReloading;
    }

    private void ListenForFrenzyInput() {
        if (Frenzy) return;
        // if (IsDodging) return;
        if (PlayerProjectile.FrenzyValue < PlayerStats.FrenzyThreshold) return;
        print(frenzyPress);
        if (frenzyPress.action.triggered) {
            Frenzy = true;
            spriteRenderer.sprite = sprites[1];
            StartCoroutine(FrenzySpin());
        }
    }

    private IEnumerator FrenzySpin() {
        AudioManager.Instance.PlaySFXAtPointUI(Resources.Load<AudioClip>("Audio/frenzytrigger"), 1f);
        PlayerStats.FrenziesUnleashed += 1;
        PlayerProjectile.FrenzyValue = 0;
        FrenzyColors.Instance.enabled = true;
        frenzyCollider.enabled = true;
        transform.localScale = Vector3.one * 2f;
        float originalSpeed = movementSpeed;
        movementSpeed = movementSpeed * 2f;
        animator.Play("PlayerFrenzy");
        Invoke(nameof(EndFrenzy), frenzyDuration);
        WeaponStatusRender.Instance.DepleteFrenzyAnimation(frenzyDuration);
        yield return new WaitUntil(() => Frenzy == false);
        movementSpeed = originalSpeed;
        animator.Play("PlayerIdle");
        transform.localScale = Vector3.one;
        frenzyCollider.enabled = false;
        FrenzyColors.Instance.enabled = false;
        yield return null;
    }

    private void ReadMovementInput() {
        movementInput = playerMovement.action.ReadValue<Vector2>();
        mousePositionInput = mouseMovement.action.ReadValue<Vector2>();
    }

    private void RespondToDodgeInput() {
        if (!IsDodging) {
            return;
        }

        transform.Translate(movementInput * (dodgeSpeed * Time.fixedDeltaTime));
        // Debug.Log("Translation: " + (movementInput * (dodgeSpeed * Time.fixedDeltaTime)).ToString());
    }

    private void ListenForDodgeInput() {
        currentDodgeCooldownTimer = currentDodgeCooldownTimer > 0f ? currentDodgeCooldownTimer - Time.deltaTime : 0f;
        if (Frenzy) return;
        if (IsDodging) return;
        if (currentDodgeCooldownTimer > 0f) return;

        if (dodgePress.action.triggered) {
            currentDodgeCooldownTimer = PlayerStats.DodgeCooldownTime;
            print("Player Dodge");
            StartCoroutine(Dodge());
        }
    }

    private IEnumerator Dodge() {
        AudioManager.Instance.PlaySFXAtPoint(transform.position, Resources.Load<AudioClip>("Audio/wheel"+Random.Range(1,4)), Random.Range(.8f, 1.2f));
        PlayerStats.TimesDodged += 1;
        IsDodging = true;
        laserLine.SetActive(isReloading ? laserLine.activeInHierarchy : false);
        animator.Play("PlayerDodge");
        yield return new WaitForSeconds(dodgeDuration);
        laserLine.SetActive(isReloading ? laserLine.activeInHierarchy : true);
        IsDodging = false;
    }

    private void ListenForReloadInput() {
        if (isReloading) return;
        if (Frenzy) return;

        if (ammoLeft >= clipSize) {
            return;
        }

        if (reloadPress.action.triggered) {
            print("Manual Reload");
            StartCoroutine(Reload());
        }
    }

    private void ListenForFireInput() {
        if (IsDodging) return;
        if (isReloading) return;
        if (Frenzy) return;

        currentShootCooldownTimer = currentShootCooldownTimer > 0f ? currentShootCooldownTimer - Time.deltaTime : 0f;

        if (currentShootCooldownTimer > 0f) {
            return;
        }

        if (mouseClick.action.phase == InputActionPhase.Performed) {
            currentShootCooldownTimer = PlayerStats.AttackCooldownTime;
            ammoLeft -= 1;
            OnUpdateAmmo?.Invoke(ammoLeft, clipSize);
            PerformAttack();
            if (ammoLeft <= 0) {
                StartCoroutine(Reload());
            }
        }
    }

    private IEnumerator ReloadAnimation(float reloadWaitTime) {
        // Done here because using Unity Animation causes issues
        // 720 degrees in reloadWaitTime
        Vector3 initialRotation = gunPivot.transform.localEulerAngles;
        for (float i = 0; i < 1f; i += .01f) {
            gunPivot.transform.rotation =
                Quaternion.Euler(0, 0, Mathf.Lerp(initialRotation.z, initialRotation.z + 1440f, i));
            yield return new WaitForSeconds(reloadWaitTime / 100f);
        }

        yield return null;
    }

    private IEnumerator Reload() {
        float reloadWaitTime = ammoLeft <= 0 ? reloadDuration : fastReloadDuration;
        isReloading = true;

        ReloadSound();
        Invoke(nameof(ReloadSound),.2f);
        Invoke(nameof(ReloadSound),.4f);
        Invoke(nameof(ReloadSound),.6f);
        Invoke(nameof(ReloadSound),.8f);
        Invoke(nameof(ReloadSound),1f);
        
        // Reload animation
        OnWeaponReload?.Invoke(reloadWaitTime);
        StartCoroutine(ReloadAnimation(reloadWaitTime / 1.25f));

        laserLine.SetActive(IsDodging ? laserLine.activeInHierarchy : false);
        DOTween.To(() => ammoLeft, aL => ammoLeft = aL, clipSize, reloadWaitTime).OnUpdate(() => {
            OnUpdateAmmo?.Invoke(ammoLeft, clipSize);
        });
        yield return new WaitForSeconds(reloadWaitTime);
        currentShootCooldownTimer = 0f;
        laserLine.SetActive(IsDodging ? laserLine.activeInHierarchy : true);
        isReloading = false;
    }

    private void ReloadSound() {
        AudioManager.Instance.PlaySFXAtPoint(transform.position, Resources.Load<AudioClip>("Audio/reload"), Random.Range(.8f,1.2f));
    }

    private void PerformAttack() {
        GameObject projectile = Instantiate(projectilePrefab, barrel.transform.position, Quaternion.identity);
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(new(mousePositionInput.x, mousePositionInput.y, 0));
        mousePosition.z = 0;

        AudioManager.Instance.PlaySFXAtPoint(transform.position, Resources.Load<AudioClip>("Audio/staplerfire" + Random.Range(1,3) ), Random.Range(0.8f, 1.2f));

        Vector3 gunOriginalSize = gun.transform.localScale;
        gun.transform.DOScale(gunOriginalSize * 1.2f, .05f).OnComplete(() => {
            gun.transform.DOScale(gunOriginalSize, .05f);
        });

        Vector3 direction = (mousePosition - projectile.transform.position).normalized;
        float angle = (float)Math.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        projectile.transform.rotation = Quaternion.Euler(0, 0, angle - 90f);

        projectile.GetComponent<PlayerProjectile>().ProjectileSpeed = PlayerStats.ProjectileSpeed;
    }

    private void GunRotationFollowMouse() {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(new(mousePositionInput.x, mousePositionInput.y, 0));
        Vector3 direction = (mousePosition - gunPivot.transform.position).normalized;
        float angle = (float)(Math.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
        gunPivot.transform.rotation = Quaternion.Euler(0, 0, angle);
        gun.GetComponent<SpriteRenderer>().flipY = direction.x < 0;
        spriteRenderer.flipX = direction.x < 0;
    }

    private void HandleWASD() {
        if (IsDodging) return;
        if (movementInput == Vector2.zero) return;
        transform.Translate(movementInput * (movementSpeed * Time.fixedDeltaTime));
    }

    private void OnCollisionEnter2D(Collision2D collision) { Debug.Log(collision.gameObject.name); }

    public Vector2 GetDodgeStatus() { return new(currentDodgeCooldownTimer, PlayerStats.DodgeCooldownTime); }

    private void InitializeCursor() {
        // Might want to move this to title screen and make it as a singleton later
        Texture2D sprite = Resources.Load<Texture2D>("Sprites/UI/CursorSmaller");
        Cursor.SetCursor(sprite, new(16, 16), CursorMode.ForceSoftware);
    }

    private void EndFrenzy() {
        Frenzy = false;
        spriteRenderer.sprite = sprites[0];
        PlayerProjectile.FrenzyValue = 0;
    }

    private void OnTriggerStay2D(Collider2D other) {
        if (!Frenzy) return;
        if (other.gameObject.layer != 7) return;
        if (other.gameObject.GetComponent<IDamageable>() == null) return;
        
        print("triggering " + Random.Range(0f, 256f).ToString() );
        
        IDamageable target = other.gameObject.GetComponent<IDamageable>();
        if (target.GetHealthStats().Item1 <= 0) return;
        AudioManager.Instance.PlaySFXAtPoint(transform.position, Resources.Load<AudioClip>("Audio/hitgeneric"), Random.Range(.8f, 1.2f));
        target.TakeDamage(frenzyDamage);
    }


    
}