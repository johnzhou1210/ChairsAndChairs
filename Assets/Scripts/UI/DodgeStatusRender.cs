using System;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.UI;

public class DodgeStatusRender : MonoBehaviour {
    [SerializeField, Self] private Image fillSprite;

    private void OnValidate() {
        this.ValidateRefs();
    }

    private void Update() {
        fillSprite.fillAmount = 1f - (PlayerInput.Instance.GetDodgeStatus().x / PlayerInput.Instance.GetDodgeStatus().y);
        fillSprite.color = fillSprite.fillAmount < 1 ? new Color(.6f, .6f, .6f, 1f) : Color.white;
    }
}
