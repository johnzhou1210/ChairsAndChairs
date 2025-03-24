using System;
using System.Collections;
using KBCore.Refs;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyHealth : MonoBehaviour, IDamageable {
    [SerializeField] public int MaxHealth = 3;
    [SerializeField, Self] private Animator animator;
    public bool Debounce { get; set; }
    public int CurrentHealth { get; private set; }
    public float DamageInvulPeriod { get; private set; } = .15f;

    public static event Action<int, int> OnTakeDamage;

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
        animator.Play("BossHurt");
        
        if (CurrentHealth == 0) {
            animator.Play("BossDead");
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