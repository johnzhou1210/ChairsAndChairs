using System;
using System.Collections;
using KBCore.Refs;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class ResultsScreen : MonoBehaviour
{
    private AudioClip resultsScreenMusic, clickSound, hoverSound;

    [SerializeField, Self] private Animator animator;

    [SerializeField] private TextMeshProUGUI timeElapsedText,
        damageDealtText,
        damageTakenText,
        frenziesUnleashedText,
        timesDodgedText,
        headerText,
        timeElapsedHeader,
        damageDealtHeader,
        damageTakenHeader,
        frenziesUnleashedHeader,
        timesDodgedHeader;
    private AudioClip cinematicHit;
    [SerializeField] private Image backgroundImage, flashEffect;
    [SerializeField] private Sprite victoryBackground, gameoverBackground;

    private void OnValidate() {
        this.ValidateRefs();
    }

    private void Start() {
        backgroundImage.sprite = PlayerStats.Victory ? victoryBackground : gameoverBackground;

        cinematicHit = Resources.Load<AudioClip>("Audio/cinematichit2");
        resultsScreenMusic = Resources.Load("Audio/Music/ResultsScreenMusic") as AudioClip;
        clickSound = Resources.Load<AudioClip>("Audio/click");
        hoverSound = Resources.Load<AudioClip>("Audio/hover");
        headerText.text = PlayerStats.Victory ? "Victory" : "In Custody";

        if (!PlayerStats.Victory) flashEffect.enabled = false;
        
        StartCoroutine(PlayerStats.Victory ? VictoryCoroutine() : GameOverCoroutine());
        
    }


    private IEnumerator ShowResults() {
        yield return new WaitForSeconds(.33f);
        timeElapsedHeader.text = "time elapsed";
        timeElapsedText.text = FormatTime((int)PlayerStats.TimeElapsed);
        CinematicHitSound(1.4f);
        yield return new WaitForSeconds(.33f);
        damageDealtHeader.text = "damage dealt";
        damageDealtText.text = PlayerStats.DamageDealt.ToString();
        CinematicHitSound(1.4f);
        yield return new WaitForSeconds(.33f);
        damageTakenHeader.text = "damage taken";
        damageTakenText.text = PlayerStats.DamageTaken.ToString();
        CinematicHitSound(1.4f);
        yield return new WaitForSeconds(.33f);
        frenziesUnleashedHeader.text = "frenzies unleashed";
        frenziesUnleashedText.text = PlayerStats.FrenziesUnleashed.ToString();
        CinematicHitSound(1.4f);
        yield return new WaitForSeconds(.33f);
        timesDodgedHeader.text = "times dodged";
        timesDodgedText.text = PlayerStats.TimesDodged.ToString();
        CinematicHitSound(1.4f);
        yield return new WaitForSeconds(.33f);
        animator.Play("ResultScreenShowButtons");
        yield return null;
    }

    private IEnumerator GameOverCoroutine() {
        AudioManager.Instance.PlaySFXAtPointUI(Resources.Load<AudioClip>("Audio/GameOverSFX"), 1f);
        yield return new WaitForSeconds(.5f);
        animator.Play("ResultScreenShowMainFrame");
        yield return new WaitForSeconds(2f);
        StartCoroutine(ShowResults());
    }
    
    private IEnumerator VictoryCoroutine() {
        float a = 0f;
        while (flashEffect.color.r < 1f) {
            a += .01f;
            flashEffect.color = new Color(a, a, a, 1f);
            yield return new WaitForSeconds(.02f);
        }
        
        yield return new WaitForSeconds(1f);
        a = 1f;
        
        while (flashEffect.color.a > 0f) {
            a -= .01f;
            if (Mathf.Approximately(a, .65f)) {
                AudioManager.Instance.PlayMusic(Resources.Load<AudioClip>("Audio/Music/Ecstatic"));
            }
            flashEffect.color = new Color(1f, 1f, 1f, a);
            yield return new WaitForSeconds(0.05f);
        }

        yield return new WaitForSeconds(3f);
        animator.Play("ResultScreenShowMainFrame");
        yield return new WaitForSeconds(2f);
        StartCoroutine(ShowResults());
    }


    private string FormatTime(int seconds) {
        TimeSpan time = TimeSpan.FromSeconds(seconds);
        return string.Format("{0:D2}:{1:D2}:{2:D2}", time.Hours, time.Minutes, time.Seconds);
    }
    
    
    public void ToHomeScreen() {
        ClickSound();
        ResetStats();
        AudioManager.Instance.StopMusic();
        SceneManager.LoadScene(0);
    }

    public void RestartGame() {
        ClickSound();
        ResetStats();
        AudioManager.Instance.StopMusic();
        SceneManager.LoadScene(3);
    }

    public void ClickSound() {
        AudioManager.Instance.PlaySFXAtPointUI(clickSound, Random.Range(0.8f, 1.2f));
    }

    public void HoverSound() {
        AudioManager.Instance.PlaySFXAtPointUI(hoverSound, Random.Range(0.8f, 1.2f));
    }

    public void CinematicHitSound(float pitch) {
        AudioManager.Instance.PlaySFXAtPointUI(cinematicHit, pitch);
    }
    
    private void ResetStats() {
        PlayerStats.TimeElapsed = 0f;
        PlayerStats.DamageDealt = 0;
        PlayerStats.DamageTaken = 0;
        PlayerStats.FrenziesUnleashed = 0;
        PlayerStats.TimesDodged = 0;
        PlayerStats.BossesKilled = 0;

        PlayerStats.PiercingUpgrade = false;
        PlayerStats.AttackCooldownTime = .5f;
        PlayerStats.ProjectileSpeed = 5f;
        PlayerStats.DodgeCooldownTime = 1f;
        PlayerStats.MaxHealth = 10;
        PlayerStats.FrenzyThreshold = 50;

        PlayerStats.Victory = false;

    }
    
}
