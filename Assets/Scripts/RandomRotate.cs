using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class RandomRotate : MonoBehaviour {
    public float RotationSpeed = 16f;

    private void OnEnable() { RotationSpeed *= (Random.Range(0, 2) == 1 ? -1 : 1); }

    private void FixedUpdate() { transform.Rotate(Vector3.forward, RotationSpeed * Time.fixedDeltaTime); }
}