using System;
using System.Collections;
using DG.Tweening;
using KBCore.Refs;
using UnityEngine;
using Random = UnityEngine.Random;

public class UIShake : MonoBehaviour
{
    public float ShakeDuration = 5f;
    public float ShakeIntensity = 0.5f;
    private Vector3 originalPosition;
    [SerializeField] private RectTransform rectTransform;

    private AudioClip cinematicHit;
    

    private void Start() {
        cinematicHit = Resources.Load<AudioClip>("Audio/cinematichit");
        print(cinematicHit);
    }

    private void OnEnable() {
        originalPosition = rectTransform.anchoredPosition;
    }
    
    public void TriggerShake() {
        rectTransform.DOShakeAnchorPos(ShakeDuration, ShakeIntensity);
    }

    public void CinematicHit() {
        AudioManager.Instance.PlaySFXAtPointUI(cinematicHit, Random.Range(0.8f, 1.2f));
    }
    
}
