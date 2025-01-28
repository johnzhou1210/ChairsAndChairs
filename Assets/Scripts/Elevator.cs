using System;
using KBCore.Refs;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Elevator : MonoBehaviour {
    [SerializeField] private InputActionReference playerInteractPress;
    [SerializeField] private GameObject nextFloorPrompt;
    [SerializeField] private int nextFloor;
    [SerializeField, Child] private SpriteRenderer spriteRenderer;
    private Sprite elevatorInactiveSprite, elevatorActiveSprite;
    private void OnValidate() {
        this.ValidateRefs();
    }

    private void Start() {
        nextFloorPrompt = transform.Find("Prompt").gameObject;
        nextFloorPrompt.SetActive(false);
        
        elevatorInactiveSprite = Resources.Load<Sprite>("Sprites/elevator_inactive");
        elevatorActiveSprite = Resources.Load<Sprite>("Sprites/elevator_active");
        
        spriteRenderer.sprite = elevatorInactiveSprite;
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.layer != 6) { // Player layer is 6
            return;
        }

        if (LevelManager.Instance.BeatLevel) {
            nextFloorPrompt.SetActive(true);
        }
    }

    private void Update() {
        if (LevelManager.Instance.BeatLevel && spriteRenderer.sprite != elevatorActiveSprite) {
            spriteRenderer.sprite = elevatorActiveSprite;
            AudioManager.Instance.PlaySFXAtPointUI(Resources.Load<AudioClip>("Audio/elevator"), 1f);
        }
        if (!LevelManager.Instance.BeatLevel) return;
        if (!nextFloorPrompt.activeInHierarchy) return;
        if (playerInteractPress.action.IsPressed()) {
            SceneManager.LoadScene(nextFloor);
            enabled = false;
        }
    }

    private void OnTriggerExit2D(Collider2D other) { nextFloorPrompt.SetActive(false); }
}