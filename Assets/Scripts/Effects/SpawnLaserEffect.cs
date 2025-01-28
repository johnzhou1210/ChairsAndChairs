using System;
using UnityEngine;

public class SpawnLaserEffect : MonoBehaviour
{
    private void Start() {
        AudioManager.Instance.PlaySFXAtPoint(transform.position, Resources.Load<AudioClip>("Audio/spawn"));
    }
}
