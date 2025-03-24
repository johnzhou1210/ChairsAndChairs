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
    [SerializeField] private GameObject mainUI, upgradeNotice, inputViewer, pauseButtonUI;
    

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
        
        switch (SceneManager.GetActiveScene().buildIndex) { // Don't set CEO music here
            case 3: // Assistant Manager
            case 4: // Manager
                AudioManager.Instance.PlayMusic(Resources.Load<AudioClip>("Audio/Music/Beat Down"));
            break;
            case 5: // Director
                AudioManager.Instance.PlayMusic(Resources.Load<AudioClip>("Audio/Music/Spooky Tendency"));
            break;
            case 6: // Superintendent
            case 7: // President
                AudioManager.Instance.PlayMusic(Resources.Load<AudioClip>("Audio/Music/Chairs of Justice"));
            break;
        }
        
        Invoke("StartBossFight", SceneManager.GetActiveScene().buildIndex == 8 ? 13f : 5f);
        if (SceneManager.GetActiveScene().buildIndex != 8) {
            StartCoroutine(AwaitUpgrade());
        } else {
            // await victory scene
            StartCoroutine(AwaitVictory());
        }
    }

    private IEnumerator AwaitVictory() {
        yield return new WaitUntil(() => BeatLevel);
        PlayerStats.Victory = true;
        yield return new WaitForSeconds(8f); // delay until transition to results
        SceneManager.LoadScene(2);
    }
    
    private IEnumerator AwaitUpgrade() {
        yield return new WaitUntil(() => BeatLevel);
        AudioManager.Instance.StopMusic();
        string upgradeDescription;
        switch (SceneManager.GetActiveScene().buildIndex) {
            case 3:
                PlayerStats.AttackCooldownTime = .15f;
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
        yield return new WaitForSeconds(4f);
        upgradeNotice.SetActive(true);
        yield return new WaitForSeconds(5f);
        upgradeNotice.SetActive(false);
    }

    
    public void StartBossFight() {
        mainUI.SetActive(true);
        pauseButtonUI.SetActive(true);
        inputViewer.SetActive(true);
        WeaponStatusRender.Instance.ForceUIUpdate();
        IBossAI bossAI = boss.GetComponent<IBossAI>();
        bossAI.Awaken();
    }

    private void Update() {
        PlayerStats.TimeElapsed += Time.deltaTime;
    }
}
