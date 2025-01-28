using System;
using KBCore.Refs;
using UnityEngine;
using Random = UnityEngine.Random;

public class LaserSpinner : MonoBehaviour {
   [SerializeField, Self] private Animator animator;

   private void OnValidate() {
      this.ValidateRefs();
   }

   private void Start() {
      Invoke(nameof(Deactivate), 6f);
   }

   public void Deactivate() {
      AudioManager.Instance.PlaySFXAtPoint(transform.position, Resources.Load<AudioClip>("Audio/laserspinnerdeploy"), Random.Range(.6f, .7f));
      animator.Play("LaserSpinnerDeactivate");
   }
}
