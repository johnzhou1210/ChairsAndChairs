using System;
using KBCore.Refs;
using TMPro;
using UnityEngine;

public class SlowTextBlink : MonoBehaviour {
    [SerializeField, Self] private TextMeshProUGUI tmpro;
    [SerializeField] private float blinkSpeed = 1f;

    private void OnValidate() {
        this.ValidateRefs();
    }

    private void Update() {
        tmpro.color = new Color(1, 1, 1, (Mathf.Sin((Time.time * blinkSpeed) - (Mathf.PI / 2f)  ) + 1) / 2f);
    }
}
