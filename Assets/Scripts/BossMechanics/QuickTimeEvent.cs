using System;
using System.Collections;
using Coffee.UIEffects;
using KBCore.Refs;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class QuickTimeEvent : MonoBehaviour {
   [SerializeField] private Image spacebarSprite;
   [SerializeField] private Sprite spacebarDown, spacebarUp;
   [SerializeField, Self] private UIShake uiShake;
   [SerializeField] private Slider progressSlider, timerSlider;
   [SerializeField] private float gainPerPress = .05f, timerDepletionSpeed = 1f;
   [SerializeField, Child] private UIEffect uiEffect;
   private float fillValue = 0f;


   private void OnValidate() {
      this.ValidateRefs();
   }

   private void OnEnable() {
      fillValue = 0f;
      timerSlider.value = 1f;
      uiEffect.enabled = false;
      timerDepletionSpeed = CEOAI.Instance.ActivePhaseInt == 2 ? .2f : .1f;
   }

   private void Update() {
      if (PlayerHealth.Instance.CurrentHealth == 0) gameObject.SetActive(false);
      progressSlider.value = fillValue;
      if (fillValue >= 1f) return;
      if (timerSlider.value <= 0f) {
         gameObject.SetActive(false);
         return;
      }

      if (fillValue < 1f) {
         timerSlider.value = Mathf.Clamp(timerSlider.value - Time.deltaTime * timerDepletionSpeed, 0f, 1f);
         if (timerSlider.value <= 0f) {
            // if player fails to break free in time, deal a lot of damage to player
            PlayerHealth.Instance.ActuallyTakeDamage(CEOAI.Instance.ActivePhaseInt == 2 ? 6 : 10);
            AudioManager.Instance.PlaySFXAtPointUI(Resources.Load<AudioClip>("Audio/bonecrush"), Random.Range(0.8f, 1.2f));
            return;
         }
      }
      if (Input.GetKeyDown(KeyCode.Space)) {
         spacebarSprite.sprite = spacebarDown;
         uiShake.TriggerShake();
      } else if (Input.GetKeyUp(KeyCode.Space)) {
         spacebarSprite.sprite = spacebarUp;
         fillValue += gainPerPress;
         AudioManager.Instance.PlaySFXAtPointUI(Resources.Load<AudioClip>("Audio/blip"), fillValue + .5f);
         if (fillValue >= 1f) {
            AudioManager.Instance.PlaySFXAtPointUI(Resources.Load<AudioClip>("Audio/shing"), 1f);
            Invoke(nameof(BreakFree), 1f);
         }
      }

      uiEffect.enabled = fillValue >= 1f;
   }

   private void BreakFree() {
      Shing();
      GameObject explosionEffect = Resources.Load<GameObject>("Prefabs/ExplosionLargeHarmless");
      GameObject explosion = Instantiate(explosionEffect, PlayerInput.Instance.transform.position, Quaternion.identity);
      AudioManager.Instance.PlaySFXAtPointUI(Resources.Load<AudioClip>("Audio/explosion"), Random.Range(0.8f,1.2f));
      PlayerInput.Instance.enabled = true;
      CEOAI.Instance.OnPlayerRelease();
      PlayerHealth.Instance.CustomCameraShake(.7f);
      gameObject.SetActive(false);
   }
   
   

   private void Shing() {
      AudioManager.Instance.PlaySFXAtPointUI(Resources.Load<AudioClip>("Audio/shing2"), Random.Range(0.8f,1.2f));
   }
   
}
