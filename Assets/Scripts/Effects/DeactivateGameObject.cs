using UnityEngine;

public class DeactivateGameObject : MonoBehaviour
{
    public void Deactivate() {
        gameObject.SetActive(false);
    }
}
