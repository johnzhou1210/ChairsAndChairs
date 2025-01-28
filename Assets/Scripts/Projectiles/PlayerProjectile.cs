using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerProjectile : Projectile {
    
    public int ProjectileDamage = 1;
    public static event Action<int> OnUpdateFrenzy;
    public static int FrenzyValue = 0;

    private void Start() {
        Invoke(nameof(Disintegrate), 30f);
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.layer == 6) return;
        if (collision.gameObject.layer == 10) return;
        
        if (collision.gameObject.layer == 7 ) { // Enemy layer
            print(collision.gameObject.CompareTag("Spawner"));
            if (collision.gameObject.CompareTag("Spawner")) {
                AudioManager.Instance.PlaySFXAtPoint(transform.position, Resources.Load<AudioClip>("Audio/hitgeneric"), Random.Range(.8f, 1.2f));
                if (DisintegrationEffect != null) {
                    Instantiate(DisintegrationEffect, transform.position + (transform.up * .2f), Quaternion.identity);
                }
                if (!PlayerStats.PiercingUpgrade) {
                    Disintegrate();
                }
                return;
            }
            if (collision.gameObject.GetComponent<IDamageable>().GetHealthStats().Item1 > 0) {
                if (!PlayerInput.Instance.Frenzy && FrenzyValue < PlayerStats.FrenzyThreshold) {
                    FrenzyValue++;    
                    OnUpdateFrenzy?.Invoke(FrenzyValue);
                }
                collision.gameObject.GetComponent<IDamageable>().TakeDamage(ProjectileDamage);
            }
            
        }

        AudioManager.Instance.PlaySFXAtPoint(transform.position, Resources.Load<AudioClip>("Audio/hitgeneric"), Random.Range(.8f, 1.2f));
        if (DisintegrationEffect != null) {
            Instantiate(DisintegrationEffect, transform.position + (transform.up * .2f), Quaternion.identity);
        }
        if (!PlayerStats.PiercingUpgrade) {
            Disintegrate();
        }
    }
    
}