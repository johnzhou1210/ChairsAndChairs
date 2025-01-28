using System;
using UnityEngine;

public class LockRotation : MonoBehaviour
{
   private Quaternion initialRotation;

   private void Start() {
      initialRotation = transform.rotation;
   }

   private void LateUpdate() {
      transform.rotation = initialRotation;
   }
}
