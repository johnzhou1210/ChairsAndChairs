using System;
using System.Collections;
using KBCore.Refs;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class BossHealth : MonoBehaviour, IDamageable {
    [SerializeField] public int MaxHealth = 50;
    [SerializeField, Self] private Animator animator;
    public int CurrentHealth { get; private set; }
    public int LastHealth { get; private set; }
    public float DamageInvulPeriod { get; private set; } = .25f;
    public bool Debounce { get; set; }
    
    private string bossName;
    

    public static event Action<string, int, int> OnTakeDamage;

    private void OnValidate() { this.ValidateRefs(); }

    private void Awake() {
        CurrentHealth = MaxHealth;
        LastHealth = CurrentHealth;
    }

    private void Start() {
        OnTakeDamage?.Invoke(bossName, CurrentHealth, MaxHealth);
    }

    public void TakeDamage(int damage) {
        if (CurrentHealth <= 0) return;
        if (damage <= 0) return;
        if (Debounce) return;
        
        StartCoroutine(DoDamageDebounce());
        
        DamageIndicate(damage, new Color(255f/255f,200f/255f,100f/255f), new Color(255f/255f,0f/255f,0f/255f));
        LastHealth = CurrentHealth;
        CurrentHealth = Mathf.Clamp(CurrentHealth - damage, 0, MaxHealth);
        OnTakeDamage?.Invoke(bossName, CurrentHealth, MaxHealth);
        PlayerStats.DamageDealt += damage;
        animator.Play("BossHurt");
        if (CurrentHealth == 0) {
            PlayerStats.BossesKilled += 1;
            BossBarRender.Instance.Hide();
            animator.Play("BossDead");
            GameObject.FindWithTag("IntroBanner").GetComponent<Animator>().Play("winbattle");
        }
    }

    public void Recover(int amount) {
        if (CurrentHealth <= 0) return;
        if (amount <= 0) return;
        AudioManager.Instance.PlaySFXAtPoint(transform.position, Resources.Load<AudioClip>("Audio/heal"), Random.Range(.8f, 1.2f));
        DamageIndicate(amount*-1, new Color(255f/255f,255f/255f,255f/255f), new Color(0f/255f,255f/255f,0f/255f));
        LastHealth = CurrentHealth;
        CurrentHealth = Mathf.Clamp(CurrentHealth + amount, 0, MaxHealth);
        OnTakeDamage?.Invoke(bossName, CurrentHealth, MaxHealth);
        animator.Play("BossHeal");
    }

    public void SetName(string newName) {
        bossName = newName;
        OnTakeDamage?.Invoke(bossName, CurrentHealth, MaxHealth);
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