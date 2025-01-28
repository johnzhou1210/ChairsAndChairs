using System;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossBarRender : MonoBehaviour {
    public static BossBarRender Instance;

    [SerializeField] private GameObject bossBar;
    [SerializeField, Child] private Slider slider;
    [SerializeField, Child] private TextMeshProUGUI bossTitle;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }
    }

    private void OnValidate() { this.ValidateRefs(); }

    private void OnEnable() { BossHealth.OnTakeDamage += UpdateBossHealth; }

    private void OnDisable() { BossHealth.OnTakeDamage -= UpdateBossHealth; }

    public void UpdateBossHealth(string bossName, int bossHealth, int bossMaxHealth) {
        slider.maxValue = bossMaxHealth;
        slider.minValue = 0;
        slider.value = bossHealth;
        bossTitle.text = bossName;
    }

    public void Hide() {
       bossBar.SetActive(false);
    }

    public void Show() {
        bossBar.SetActive(true);
    }
    
}