using System;
using DG.Tweening;
using KBCore.Refs;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class DamageIndicator : MonoBehaviour {
    [SerializeField, Self] private TextMeshPro textMesh;
    
    private float appearTime = .5f, stayTime = .3f, disappearTime = .5f;
    private Vector3 size = Vector3.one * 2f;

    private void OnValidate() {
        this.ValidateRefs();
    }

    private void Awake() {
        textMesh.color = new Color(textMesh.color.r, textMesh.color.g, textMesh.color.b, 0f);
        transform.localScale = Vector3.zero;
        transform.position += (Vector3.right * Random.Range(-.5f, .5f)) + (Vector3.up * Random.Range(-.5f, .5f));
    }

    private void Start() {
        textMesh.DOFade(1, appearTime);
        transform.DOScale(size, appearTime).SetEase(Ease.OutElastic).OnComplete(() => {
            transform.DOScale(size, stayTime).OnComplete(() => {
                textMesh.DOFade(0, disappearTime);
                transform.DOScale(Vector3.zero, disappearTime).SetEase(Ease.OutQuad).OnComplete(() => {
                    Destroy(gameObject);
                });
            });
        });
    }

    public void SetContent(int damage, Color textColor, Color gradientColor) {
        textMesh.text = (damage < 0 ? "+" + (-damage) : damage.ToString()) ;
        textMesh.color = textColor;
        textMesh.colorGradient = new VertexGradient(gradientColor, gradientColor, Color.white, Color.white);
    }
    
}
