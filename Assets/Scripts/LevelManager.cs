using System;
using System.Collections;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    [SerializeField] GameObject boss; // Boss must have script that uses IBossAI interface!
    public static LevelManager Instance;
    public bool BeatLevel = false;
    [SerializeField] private GameObject mainUI, upgradeNotice, inputViewer;
    

    private void OnValidate() {
        this.ValidateRefs();
    }

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }
    }

    private void Start() {
        Invoke("StartBossFight", SceneManager.GetActiveScene().buildIndex == 8 ? 13f : 5f);
        StartCoroutine(AwaitUpgrade());
    }

    private IEnumerator AwaitUpgrade() {
        yield return new WaitUntil(() => BeatLevel);
        string upgradeDescription;
        switch (SceneManager.GetActiveScene().buildIndex) {
            case 3:
                PlayerStats.AttackCooldownTime = .3f;
                PlayerStats.ProjectileSpeed = 8f;
            break;
            case 4:
                PlayerStats.MaxHealth = 15;
            break;
            case 5:
                PlayerStats.DodgeCooldownTime = .5f;
            break;
            case 6:
                PlayerStats.PiercingUpgrade = true;
            break;
            case 7:
                PlayerStats.FrenzyThreshold = 25;
            break;
        }
        yield return new WaitForSeconds(5f);
        AudioManager.Instance.PlaySFXAtPointUI(Resources.Load<AudioClip>("Audio/upgradesound"), 1f);
        upgradeNotice.SetActive(true);
        yield return new WaitForSeconds(5f);
        upgradeNotice.SetActive(false);
    }

    
    public void StartBossFight() {
        mainUI.SetActive(true);
        inputViewer.SetActive(true);
        WeaponStatusRender.Instance.ForceUIUpdate();
        IBossAI bossAI = boss.GetComponent<IBossAI>();
        bossAI.Awaken();
    }

    private void Update() {
        PlayerStats.TimeElapsed += Time.deltaTime;
    }
}
