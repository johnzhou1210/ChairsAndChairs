using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class GameOverScreen : MonoBehaviour, IPointerClickHandler {
    private float timeElapsed = 0f;
    [SerializeField] private TextMeshProUGUI headerText;
    [SerializeField] private GameObject hint;

    private void Start() {
        Invoke(nameof(ShowYoure), 1f);
        Invoke(nameof(ShowFired), 2f);
        Invoke(nameof(SetHintActive), 4f);
    }
    
    private void SetHintActive() {
        hint.SetActive(true);
    }
    
    private void ShowYoure() {
        headerText.text = "You're";
        AudioManager.Instance.PlaySFXAtPointUI(Resources.Load<AudioClip>("Audio/cinematichit"), 1f);
    }

    private void ShowFired() {
        headerText.text = "You're <color=red>FIRED!</color>";
        AudioManager.Instance.PlaySFXAtPointUI(Resources.Load<AudioClip>("Audio/cinematichit"), .5f);
    }
    
    private void Update() {
        timeElapsed += Time.deltaTime;
    }

    public void OnPointerClick(PointerEventData eventData) {
        if (timeElapsed > 4f) {
            SceneManager.LoadScene(2); // results screen  
        }
        
    }
}
