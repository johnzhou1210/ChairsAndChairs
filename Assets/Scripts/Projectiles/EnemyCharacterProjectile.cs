using System;
using System.Collections.Generic;
using KBCore.Refs;
using Unity.VisualScripting;
using UnityEngine;
using TMPro;


 public class EnemyCharacterProjectile : Projectile {
     [SerializeField, Child] private TextMeshPro textMesh;
    public int ProjectileDamage = 1;

    private void OnValidate() {
        this.ValidateRefs();
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.CompareTag("Boss")) { // Ignore Boss itself
            return;
        }

        if (collision.gameObject.CompareTag("Player")) {
            if (collision.gameObject.GetComponent<PlayerInput>().IsDodging) {
                return;
            }
            collision.gameObject.GetComponent<PlayerHealth>().TakeDamage(ProjectileDamage);
        }
        Disintegrate();
    }

    private void Start() {
        List<string> randChars = new List<string> {"!","@","#","$","%","^","&","*","("};
        textMesh.text = Util.Choice(randChars);
        Invoke(nameof(Disintegrate), 10f);
    }
    

    public void SetRedText() {
        textMesh.color = Color.red;
        ProjectileDamage = 2;
        transform.localScale *= 3f;
    }


}
 