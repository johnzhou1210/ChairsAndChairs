using System;
using System.Collections;
using System.Collections.Generic;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.UI;

public class FrenzyColors : MonoBehaviour {
   public static FrenzyColors Instance;
   
   [SerializeField] private Color color1 = new Color(1f,1f,0f,.3f);
   [SerializeField] private Color color2 = new Color(1f, .25f, 0f, .3f);
   [SerializeField] private float cycleDuration = 1f;
   
   [SerializeField, Self] private Image image;

   bool disengaging = false;

   private void Awake() {
      if (Instance == null) {
         Instance = this;
      } else {
         Destroy(gameObject);
      }
   }

   private void OnValidate() {
      this.ValidateRefs();
   }

   private void OnEnable() {
      image.enabled = true;
      image.color = color1;
   }

   public IEnumerator DisengageEffect() {
      disengaging = true;
      float disengageTime = 2f;
      for (float i = image.color.a; i >= 0f; i -=  Time.deltaTime) {
         Color currColor = new(image.color.r, image.color.g, image.color.b, i);
         image.color = currColor;
         yield return new WaitForSeconds(Time.deltaTime * disengageTime);
      }
      image.enabled = false;
      disengaging = false;
      yield return null;
      Instance.enabled = false;
   }

   private void Update() {
      if (disengaging) return;
      float t = Mathf.PingPong(Time.time / cycleDuration, 1f);
      Color currColor = Color.Lerp(color1, color2, t);
      image.color = currColor;
   }
}
