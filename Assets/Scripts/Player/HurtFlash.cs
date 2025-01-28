using System;
using DG.Tweening;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.UI;

public class HurtFlash : MonoBehaviour {
    [SerializeField, Self] private Image image;

    private void OnEnable() {
        PlayerHealth.OnTakeDamage += DoFlash;
    }

    private void OnDisable() {
        PlayerHealth.OnTakeDamage -= DoFlash;
    }

    private void OnValidate() {
        this.ValidateRefs();
    }

    private void DoFlash(int currHealth, int maxHealth) {
        image.DOFade(.5f, 0f).OnComplete(() => {
            image.DOFade(0f, .5f);
        });
    }
    
}
