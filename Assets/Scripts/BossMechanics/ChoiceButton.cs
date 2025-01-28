using System;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Random = UnityEngine.Random;

public class ChoiceButton : MonoBehaviour
{
   [SerializeField, Self] private Light2D light;


   private void OnValidate() {
      this.ValidateRefs();
   }

   private void OnTriggerEnter2D(Collider2D other) {
      if (other.gameObject.layer == 6) { // player layer
         ButtonDownSound();
         light.intensity = 5f;
      }
   }
   
   private void OnTriggerExit2D(Collider2D other) {
      if (other.gameObject.layer == 6) { // player layer
         ButtonUpSound();
         light.intensity = 0f;
      }
   }

   private void ButtonDownSound() {
      AudioManager.Instance.PlaySFXAtPoint(transform.position, Resources.Load<AudioClip>("Audio/click"), Random.Range(0.8f, 1f));
   }
   
   private void ButtonUpSound() {
      AudioManager.Instance.PlaySFXAtPoint(transform.position, Resources.Load<AudioClip>("Audio/click"), Random.Range(1.6f, 1.8f));
   }
   
}
