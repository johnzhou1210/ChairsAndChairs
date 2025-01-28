using System;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.UI;

public class FrenzyColors : MonoBehaviour {
   public static FrenzyColors Instance;
   
   [SerializeField] private Color color1 = new Color(1f,1f,0f,.3f);
   [SerializeField] private Color color2 = new Color(1f, .25f, 0f, .3f);
   [SerializeField] private float cycleDuration = 1f;
   
   [SerializeField, Self] private Image image;

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

   private void OnDisable() {
      image.enabled = false;
   }

   private void Update() {
      float t = Mathf.PingPong(Time.time / cycleDuration, 1f);
      Color currColor = Color.Lerp(color1, color2, t);
      image.color = currColor;
   }
}
