using System;
using UnityEngine;

public class WorkerDescription : MonoBehaviour {
    [SerializeField] private GameObject textContainer;
    
    private void Update() {
        if (Vector3.Distance(transform.position, PlayerInput.Instance.gameObject.transform.position) < .9f) {
            textContainer.SetActive(true);
        } else {
            textContainer.SetActive(false);
        }
    }
}
