using System;
using System.Collections.Generic;
using KBCore.Refs;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthRender : MonoBehaviour {
    public static PlayerHealthRender Instance;
    
    [SerializeField, Child] private Slider healthSlider;
    [SerializeField, Child] private TextMeshProUGUI healthText;
    [SerializeField] private List<Sprite> playerSprites;
    [SerializeField] private Image portraitSpriteRenderer;


    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }
    }

    private void OnValidate() { this.ValidateRefs(); }
    
    private void OnEnable() {
        PlayerHealth.OnTakeDamage += UpdateUIRender;
    }

    private void OnDisable() {
        PlayerHealth.OnTakeDamage -= UpdateUIRender;
    }

    private void UpdateUIRender(int currHealth, int maxHealth) {
        healthSlider.maxValue = maxHealth;
        healthSlider.value = currHealth;
        healthText.text = $"{currHealth}/{maxHealth}";
    }

    public void ForceUpdate() {
        healthSlider.value = PlayerHealth.Instance.CurrentHealth;
        healthSlider.maxValue = PlayerHealth.Instance.MaxHealth;
        healthText.text = $"{healthSlider.value}/{healthSlider.maxValue}";
    }
    
    private void Update() {
        portraitSpriteRenderer.sprite = PlayerInput.Instance.Frenzy ? playerSprites[1] : playerSprites[0];
        ForceUpdate();
    }
}
