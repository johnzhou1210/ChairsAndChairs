using System;
using KBCore.Refs;
using UnityEngine;
using Random = UnityEngine.Random;

public class Thumbtack : MonoBehaviour {
    [SerializeField, Self] private RandomRotate randomRotate;
    [SerializeField, Self] private CircleCollider2D circleCollider2D;
    [SerializeField, Child] private SpriteRenderer spriteRenderer;

    [SerializeField] private Sprite[] sprites;
    
    private void OnValidate() {
        this.ValidateRefs();
    }

    private void Awake() {
        spriteRenderer.sprite = sprites[Random.Range(0, sprites.Length)];
    }

    private void Start() {
        spriteRenderer.color = Color.gray;
    }


    public void StartSetTimer(float duration) {
        Invoke(nameof(SetThumbtack), duration);
    }

    private void OnDestroy() {
        CancelInvoke(nameof(SetThumbtack));
    }

    private void SetThumbtack() {
        circleCollider2D.enabled = true;
        randomRotate.enabled = false;
        spriteRenderer.color = Color.white;
    }
    
}
