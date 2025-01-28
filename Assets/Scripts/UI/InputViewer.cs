using System;
using TMPro;
using TMPro.EditorUtilities;
using UnityEngine;
using UnityEngine.UI;

public class InputViewer : MonoBehaviour {
    [SerializeField] private GameObject move, shoot, dodge, frenzy, reload;
    [SerializeField] private TextMeshProUGUI lmb;
    [SerializeField] private Image w, a, s, d, shift, space, r;
    [SerializeField] private Sprite wUp,wDown,aUp,aDown,sUp,sDown,dUp,dDown,shiftUp,shiftDown,spaceUp,spaceDown, rUp, rDown;

    private void Update() {
        if (PlayerProjectile.FrenzyValue == PlayerStats.FrenzyThreshold) {
            frenzy.SetActive(true);
        } else {
            frenzy.SetActive(false);
        }
        
        if (Input.GetMouseButtonDown(0)) {
            shoot.transform.Find("Image").GetComponent<TextMeshProUGUI>().color = Color.gray;
        }

        if (Input.GetMouseButtonUp(0)) {
            shoot.transform.Find("Image").GetComponent<TextMeshProUGUI>().color = Color.white;
        }

        if (Input.GetKeyDown(KeyCode.W)) {
            w.sprite = wDown;
        }
        if (Input.GetKeyUp(KeyCode.W)) {
            w.sprite = wUp;
        }
        
        if (Input.GetKeyDown(KeyCode.A)) {
            a.sprite = aDown;
        }
        if (Input.GetKeyUp(KeyCode.A)) {
            a.sprite = aUp;
        }
        
        if (Input.GetKeyDown(KeyCode.S)) {
            s.sprite = sDown;
        }
        if (Input.GetKeyUp(KeyCode.S)) {
            s.sprite = sUp;
        }
        
        if (Input.GetKeyDown(KeyCode.D)) {
            d.sprite = dDown;
        }
        if (Input.GetKeyUp(KeyCode.D)) {
            d.sprite = dUp;
        }
        
        if (Input.GetKeyDown(KeyCode.LeftShift)) {
            shift.sprite = shiftDown;
        }
        if (Input.GetKeyUp(KeyCode.LeftShift)) {
            shift.sprite = shiftUp;
        }
        
        if (Input.GetKeyDown(KeyCode.Space)) {
            space.sprite = spaceDown;
        }
        if (Input.GetKeyUp(KeyCode.Space)) {
            space.sprite = spaceUp;
        }
        
        if (Input.GetKeyDown(KeyCode.R)) {
            r.sprite = rDown;
        }
        if (Input.GetKeyUp(KeyCode.R)) {
            r.sprite = rUp;
        }
        
        
        
    }

  
    
}
