using System;
using System.Collections;
using System.Collections.Generic;
using KBCore.Refs;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class BodyguardJumpTrigger : MonoBehaviour {
    [SerializeField, Self] private Collider2D collider;
    [SerializeField] private GameObject bodyguardEffectPrefab;
    [SerializeField] private List<Sprite> bodyGuardSprites;
    [SerializeField] private Transform bodyguardEffectContainer;
    [SerializeField] private GameObject disintegrationEffect;

    private List<Vector3> bodyguardEffectSpawnPoints = new List<Vector3> {
        new Vector3(6.48000002f,14.4300003f,-3.95305991f),
        new Vector3(-5.55999994f,14.5100002f,-3.95305991f)
    };
    
    private void OnValidate() { this.ValidateRefs(); }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.layer == LayerMask.NameToLayer("Projectile")) {
            print("TRIGGER ACTIVATE");
            int chosenIndx = Random.Range(0, 2);
            GameObject meatShield = Instantiate(bodyguardEffectPrefab, bodyguardEffectSpawnPoints[chosenIndx] + new Vector3(Random.Range(-2f, 2f), Random.Range(-.5f,.5f), 0f),
                Quaternion.identity, bodyguardEffectContainer);
            // assign sprite (2 * n - 1)
            GameObject bodyGuard = meatShield.transform.Find("Container").Find("Bodyguard").gameObject;
            int spriteIndx = (2 * Random.Range(1, 4)) - 1;
            bodyGuard.GetComponent<SpriteRenderer>().sprite = bodyGuardSprites[spriteIndx];
            meatShield.GetComponent<Animator>().Play("BodyguardProtect" + (chosenIndx + 1));
            bodyGuard.transform.Find("Dialog").GetComponent<TextMeshPro>().enabled = true;
            StartCoroutine(TakeHit(bodyGuard, spriteIndx, other.gameObject, 1/3f));
        }
    }

    
    private IEnumerator TakeHit(GameObject bodyGuard, int spriteIndx, GameObject interceptedProjectile, float delay) {
        yield return new WaitForSeconds(delay);
        if (bodyGuard == null) yield break;
        bodyGuard.transform.Find("Dialog").GetComponent<TextMeshPro>().enabled = false;
        if (spriteIndx == -1) yield break;
        SpriteRenderer spriteRenderer = bodyGuard.GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = bodyGuardSprites[spriteIndx - 1];
        
        if (interceptedProjectile == null) yield break;
        Instantiate(disintegrationEffect, interceptedProjectile.transform.position, Quaternion.identity);
        Destroy(interceptedProjectile);
    }
}