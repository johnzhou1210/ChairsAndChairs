using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class UpgradeNoticeSoundEffects : MonoBehaviour {
    private AudioClip upgradeSound;
    private AudioClip cinematicSound;
    private void Start() {
        upgradeSound = Resources.Load<AudioClip>("Audio/upgradesound");
        cinematicSound = Resources.Load<AudioClip>("Audio/cinematichit2");
    }

    public void UpgradeSound() {
        print("play sound");
        AudioManager.Instance.PlaySFXAtPointUI(cinematicSound, Random.Range(0.8f,1.2f));
        AudioManager.Instance.PlaySFXAtPointUI(upgradeSound, 1f);
    }
}
