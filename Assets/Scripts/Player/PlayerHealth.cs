using System;
using System.Collections;
using KBCore.Refs;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class PlayerHealth : MonoBehaviour, IDamageable {
    public static PlayerHealth Instance;
    public bool Debounce { get; set; }
    public int MaxHealth { get; private set; } = PlayerStats.MaxHealth;
    [SerializeField, Self] private Animator animator;
    private CinemachineBasicMultiChannelPerlin cinemachineNoiseChannel;
    public int CurrentHealth { get; private set; }
    public float DamageInvulPeriod { get; set; } = 1f;

    public static event Action<int, int> OnTakeDamage;

    private void OnValidate() { this.ValidateRefs(); }

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }
        Debounce = false;
        cinemachineNoiseChannel = GameObject.FindWithTag("CinemachineCamera").GetComponent<CinemachineBasicMultiChannelPerlin>();
    }

    private void Start() {
        CurrentHealth = MaxHealth;
    }

    public void TakeDamage(int damage) {
        if (PlayerInput.Instance.Frenzy) return;
        if (Debounce) return;
        if (CurrentHealth <= 0) return;
        if (damage <= 0) return;

       ActuallyTakeDamage(damage);
    }

    public void ActuallyTakeDamage(int damage) {
        StartCoroutine(DoDamageDebounce());
        AudioManager.Instance.PlaySFXAtPoint(transform.position, Resources.Load<AudioClip>("Audio/hurtsound"), Random.Range(0.8f, 1.2f), .5f);
        
        StartCameraShake();
        Invoke(nameof(EndCameraShake), .25f);
        
        DamageIndicate(damage, new Color(255f/255f,220f/255f,255f/255f), new Color(255f/255f,0f/255f,0f/255f));
        CurrentHealth = Mathf.Clamp(CurrentHealth - damage, 0, MaxHealth);
        OnTakeDamage?.Invoke(CurrentHealth, MaxHealth);
        PlayerStats.DamageTaken += damage;
        animator.Play("PlayerHurt");
        if (CurrentHealth == 0) {
            AudioManager.Instance.StopMusic();
            animator.Play("PlayerDead");
            Invoke(nameof(LoseBattle), .5f);
        }
    }

    private void LoseBattle() {
        GameObject.FindWithTag("IntroBanner").GetComponent<Animator>().Play("losebattle");
        StartCoroutine(PrepareGameOver());
    }

    IEnumerator PrepareGameOver() {
        yield return new WaitForSeconds(5f);
        SceneManager.LoadScene(1); // game over scene
        yield return null;
    }
    

    public void Recover(int amount) {
        if (CurrentHealth <= 0) return;
        if (amount <= 0) return;
        AudioManager.Instance.PlaySFXAtPoint(transform.position, Resources.Load<AudioClip>("Audio/heal"), Random.Range(0.8f, 1.2f));
        DamageIndicate(amount*-1, new Color(255f/255f,255f/255f,255f/255f), new Color(0f/255f,255f/255f,0f/255f));
        CurrentHealth = Mathf.Clamp(CurrentHealth + amount, 0, MaxHealth);
        OnTakeDamage?.Invoke(CurrentHealth, MaxHealth);
        animator.Play("PlayerHeal");
    }
    

    private IEnumerator DoDamageDebounce() {
        Debounce = true;
        yield return new WaitForSeconds(DamageInvulPeriod);
        Debounce = false;
    }
    
    public void StartCameraShake() {
        cinemachineNoiseChannel.AmplitudeGain +=  5f;
        cinemachineNoiseChannel.FrequencyGain +=  5f;
    }

    public void EndCameraShake() {
        cinemachineNoiseChannel.AmplitudeGain = Mathf.Clamp(cinemachineNoiseChannel.AmplitudeGain - 5f, 0f, 256f);
        cinemachineNoiseChannel.FrequencyGain = Mathf.Clamp(cinemachineNoiseChannel.FrequencyGain - 5f, 0f, 256f);
        
    }

    public void CustomCameraShake(float duration) {
        StartCameraShake();
        Invoke(nameof(EndCameraShake), duration);
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
    
    
}