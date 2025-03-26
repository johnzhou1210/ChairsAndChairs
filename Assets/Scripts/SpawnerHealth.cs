using System;
using System.Collections;
using KBCore.Refs;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class SpawnerHealth : MonoBehaviour, IDamageable {
    [SerializeField] public int MaxHealth = 30;
    [SerializeField, Self] private Animator animator;
    public int CurrentHealth { get; private set; }
    public bool Debounce { get; set; }
    public float DamageInvulPeriod { get; set; } = .13f;

    public static event Action<int, int> OnTakeDamage;
    public static event Action OnSpawnerDestroyed;

    private void OnValidate() { this.ValidateRefs(); }

    private void Awake() {
        CurrentHealth = MaxHealth;
    }

    private void Start() {
        OnTakeDamage?.Invoke(CurrentHealth, MaxHealth);
    }

    public void TakeDamage(int damage) {
        if (CurrentHealth <= 0) return;
        if (damage <= 0) return;
        if (Debounce) return;
        
        StartCoroutine(DoDamageDebounce());
        
        DamageIndicate(damage, new Color(255f/255f,200f/255f,100f/255f), new Color(255f/255f,0f/255f,0f/255f));
        CurrentHealth = Mathf.Clamp(CurrentHealth - damage, 0, MaxHealth);
        OnTakeDamage?.Invoke(CurrentHealth, MaxHealth);
        PlayerStats.DamageDealt += damage;
        animator.Play("Damaged");
        
        if (CurrentHealth == 0) {
            AudioManager.Instance.PlaySFXAtPoint(transform.position, Resources.Load<AudioClip>("Audio/explosion"), Random.Range(0.8f, 1.2f));
            animator.Play("Broken");
            OnSpawnerDestroyed?.Invoke();
            PlayerHealth.Instance.StartCameraShake();
            PlayerHealth.Instance.Invoke(nameof(PlayerHealth.EndCameraShake), .35f);
            GetComponent<Collider2D>().enabled = false;
        }
    }

    public void DamageIndicate(int damage, Color textColor, Color gradientColor) {
        GameObject indicatorPrefab = Resources.Load<GameObject>("Prefabs/DamageIndicator");
        GameObject indicator = Instantiate(indicatorPrefab, transform.position - Vector3.forward, Quaternion.identity);
        indicator.GetComponent<DamageIndicator>().SetContent(damage, textColor, gradientColor);
    }

    public Tuple<int, int> GetHealthStats() {
        return new Tuple<int, int>(CurrentHealth, MaxHealth);
    }
    
    public void SetDebounce(bool val) {
        Debounce = val;
    }
    
    private IEnumerator DoDamageDebounce() {
        Debounce = true;
        yield return new WaitForSeconds(DamageInvulPeriod);
        Debounce = false;
    }

    
    
}