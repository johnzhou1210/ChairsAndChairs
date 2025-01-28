using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class SniperSquadAI : MonoBehaviour {
    [SerializeField] private GameObject trackingDotPrefab, sniperLaserPrefab;
    [SerializeField] private int numSnipers = 5;

    private List<Coroutine> sniperCoroutines ;
    
    private void Start() {
        sniperCoroutines = new List<Coroutine>();
        for (int i = 0; i < numSnipers; i++) {
            Coroutine sniperCoro = StartCoroutine(InitRecon()); 
            sniperCoroutines.Add(sniperCoro);
        }
        
    }

    private IEnumerator InitRecon () {

        Tween currMovementTween = null;
        
        bool goodToShoot = false;
        float timeElapsedSinceLastShot = 3f;
        float changeFocusInterval = 3f;
        float changeFocusTimer = 0f;
        float shotCooldown = Random.Range(2f,9f);
        Vector3 focusPosition = GetNewFocusPosition();

        bool tweenDone = true;
        Vector3 startPos = GetPositionAroundPlayer(focusPosition, 30f);


        yield return new WaitForSeconds(1f);
        GameObject trackingDot = Instantiate(trackingDotPrefab, transform.position, Quaternion.identity);
        
        while (true) {
            changeFocusTimer += Time.fixedDeltaTime;
            timeElapsedSinceLastShot += Time.fixedDeltaTime;

            if (timeElapsedSinceLastShot >= shotCooldown) {
                goodToShoot = true;
                
            }
            
                if (currMovementTween != null) currMovementTween.Kill(); 
                focusPosition = GetNewFocusPosition();
                // Move to focus position
                currMovementTween = trackingDot.transform.DOMove(focusPosition, Random.Range(.5f, 1f)).OnKill(() => {
                    tweenDone = true;
                }).OnComplete(() => {
                    tweenDone = true;
                });
                
               
                
                // Check if dot is near player. If so, take a shot!
                if (Vector3.Distance(PlayerInput.Instance.transform.position, focusPosition) < 1f && goodToShoot && currMovementTween != null) {
                    yield return new WaitUntil(()=>tweenDone);
                    goodToShoot = false;
                    shotCooldown = Random.Range(2f, 9f);
                    timeElapsedSinceLastShot = 0f;
                    yield return new WaitForSeconds(.5f);
                    Shoot(focusPosition, startPos);
                    yield return new WaitForSeconds(2f);
                }
            
            
            yield return new WaitForSeconds(Time.fixedDeltaTime);
        }
        
       
    }

  

    private Vector3 GetPositionAroundPlayer(Vector3 center, float radius) {
        Vector3 randomDir = Random.onUnitSphere;
        randomDir.z = 0f;
        randomDir = randomDir.normalized * radius;
        return center + randomDir;
    }
    
    private Vector3 GetNewFocusPosition() {
        return PlayerInput.Instance.gameObject.transform.position + new Vector3(Random.Range(-.75f,.75f), Random.Range(-.75f,.75f), 0f);
    }

    private void Shoot(Vector3 focusPosition, Vector3 originPosition) {
        sniperLaserPrefab = Instantiate(Resources.Load<GameObject>("Prefabs/SniperLaser"), focusPosition, Quaternion.identity);
        // choose an origin position to calculate direction
        Vector3 direction = (focusPosition - originPosition).normalized;
        float centerAngle = (Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg) - 90f;
        sniperLaserPrefab.transform.rotation = Quaternion.Euler(0, 0, centerAngle);
        AudioManager.Instance.PlaySFXAtPointUI(Resources.Load<AudioClip>("Audio/snipershot"), Random.Range(0.8f, 1.2f));
    }

    public void TerminateRecon() {
        for (int i = 0; i < sniperCoroutines.Count; i++) {
            if (sniperCoroutines[i] != null) {
                StopCoroutine(sniperCoroutines[i]);
                sniperCoroutines[i] = null;
            }
        }
    }

}
