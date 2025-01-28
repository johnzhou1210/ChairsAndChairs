using System;
using UnityEngine;

public enum ProjectileBehavior {
    FORWARD,
    TARGET_POSITION,
    TARGET_GAMEOBJECT,
}

public class Projectile : MonoBehaviour
{
    public float ProjectileSpeed = 1f;

    public ProjectileBehavior ProjectileBehavior = ProjectileBehavior.FORWARD;
    private float focusDuration = 0f;
    private Vector3 targetPos; // if TARGET_POSITION
    private GameObject targetGameObject; // if TARGET_GAMEOBJECT
    [SerializeField] public GameObject DisintegrationEffect;

    private void FixedUpdate() {
        switch (ProjectileBehavior)
        {
            case ProjectileBehavior.FORWARD:
                transform.Translate(Vector3.up * (ProjectileSpeed * Time.fixedDeltaTime));
            break;
            case ProjectileBehavior.TARGET_POSITION:
                transform.position = Vector3.MoveTowards(transform.position, targetPos, ProjectileSpeed * Time.fixedDeltaTime);
                if (Vector3.Distance(transform.position, targetPos) < .1f) {
                    Disintegrate();
                }
            break;
            case ProjectileBehavior.TARGET_GAMEOBJECT:
                float homingStrength = 2.5f;
                Vector3 direction = (PlayerInput.Instance.gameObject.transform.position - transform.position).normalized;
                Vector3 adjustedDirection = Vector3.Lerp(transform.up, direction, homingStrength * Time.fixedDeltaTime);
                float centerAngle = (Mathf.Atan2(adjustedDirection.y, adjustedDirection.x) * Mathf.Rad2Deg) - 90f;
                transform.rotation = Quaternion.Euler(0, 0, centerAngle);
                transform.position += adjustedDirection.normalized * (ProjectileSpeed * Time.fixedDeltaTime);
                // transform.position = Vector3.MoveTowards(transform.position, targetGameObject.transform.position, ProjectileSpeed * Time.fixedDeltaTime);
         
                if (Vector3.Distance(transform.position, targetGameObject.transform.position) < .1f) {
                    Disintegrate();
                }
            break;
        }
        
        
    }

    public virtual void Disintegrate() {
        if (DisintegrationEffect != null) {
            Instantiate(DisintegrationEffect, transform.position + (transform.up * .2f), Quaternion.identity);
        }
        Destroy(gameObject);
    }
    
    public void SetBehavior(ProjectileBehavior newBehavior, Vector3 pos) {
        ProjectileBehavior = newBehavior;
        targetPos = pos;
    }

    public void SetBehavior(ProjectileBehavior newBehavior, GameObject target, float focusDuration = Mathf.Infinity) {
        ProjectileBehavior = newBehavior;
        targetGameObject = target;
        focusDuration = focusDuration;
        Invoke(nameof(UnfocusProjectile), focusDuration);
    }

    public void SetBehavior(ProjectileBehavior newBehavior) {
        ProjectileBehavior = newBehavior;
    }

    private void OnTriggerEnter2D(Collider2D other) {
        Debug.Log("base disintegrate");
        Disintegrate();
    }

    private void UnfocusProjectile() {
        SetBehavior(ProjectileBehavior.FORWARD);
    }

    private void OnDestroy() {
        CancelInvoke(nameof(UnfocusProjectile));
    }
}
