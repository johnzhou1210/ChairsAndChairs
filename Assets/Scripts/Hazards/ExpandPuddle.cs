using System;
using DG.Tweening;
using KBCore.Refs;
using UnityEngine;
using Random = UnityEngine.Random;

public class ExpandPuddle : MonoBehaviour {
    [SerializeField, Self] private DamageZone damageZone;

    [SerializeField] private Vector2 radiusRange = new Vector2(1f, 2f);
    [SerializeField] private float expandDuration = 1f, stayDuration = 30f, shrinkDuration = 5f;

    private void OnValidate() {
        this.ValidateRefs();
    }

    private void OnEnable() {
        transform.localScale = Vector3.zero;
        damageZone.enabled = false;
    }

    private void Start() {
        damageZone.enabled = true;
        float expandedRadius = Random.Range(radiusRange.x, radiusRange.y);
        transform.DOScale(Vector3.one * expandedRadius, expandDuration).OnComplete(() => {
            Invoke(nameof(ShrinkToNothing), stayDuration);
        }) ;
    }

    private void ShrinkToNothing() {
        Invoke( nameof(DisarmZone), shrinkDuration / 3f);
        transform.DOScale(Vector3.zero, shrinkDuration).OnComplete(() => {
            Destroy(gameObject);
        });
    }

    private void DisarmZone() {
        damageZone.enabled = false;
        damageZone.GetComponentInChildren<SpriteRenderer>().color = Color.grey;
    }
    
}
