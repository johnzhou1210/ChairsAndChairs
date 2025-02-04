using System;
using System.Collections;
using System.Collections.Generic;
using KBCore.Refs;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class TitleScreen : MonoBehaviour {
   private AudioClip titleScreenMusic, clickSound, hoverSound;
   [SerializeField, Self] private Animator animator;
   [SerializeField] private GameObject boss;
   [SerializeField] private TextMeshPro playerText, bossText;
   [SerializeField] private TextMeshProUGUI masterText, BGMText, SFXText;
   [SerializeField] private Slider[] sliders;

   private void OnValidate() {
      this.ValidateRefs();
   }

   private void Start() {
      titleScreenMusic = Resources.Load("Audio/Music/TitleScreenMusic") as AudioClip;
      clickSound = Resources.Load<AudioClip>("Audio/click");
      hoverSound = Resources.Load<AudioClip>("Audio/hover");
      AudioManager.Instance.PlayMusic(Resources.Load<AudioClip>("Audio/Music/TitleScreenMusic"));
      
      sliders[0].value = AudioManager.Instance.GetMasterVolumeSetting();
      sliders[1].value = AudioManager.Instance.GetBGMVolumeSetting();
      sliders[2].value = AudioManager.Instance.GetSFXVolumeSetting();
   }

   private void Update() {
      masterText.text = Mathf.Round(AudioManager.Instance.GetMasterVolumeSetting() * 100) + "%";
      BGMText.text = Mathf.Round(AudioManager.Instance.GetBGMVolumeSetting() * 100) + "%";
      SFXText.text = Mathf.Round(AudioManager.Instance.GetSFXVolumeSetting() * 100) + "%";
      
   }

   public void StartGame() {
      ClickSound();
      animator.Play("PressStart");
      StartCoroutine(StartDialog());
   }

   public void ClickSound() {
      AudioManager.Instance.PlaySFXAtPointUI(clickSound, Random.Range(0.8f, 1.2f));
   }

   public void HoverSound() {
      AudioManager.Instance.PlaySFXAtPointUI(hoverSound, Random.Range(0.8f, 1.2f));
   }

   public void CueBossWalk() {
      boss.SetActive(true);
   }

   private IEnumerator StartDialog() {
      yield return new WaitForSeconds(5f);
      bossText.text = "You're too slow. Get your work done already!";
      AudioManager.Instance.PlaySFXAtPointUI(Resources.Load<AudioClip>("Audio/talk1"), Random.Range(1.6f, 2.0f));
      yield return new WaitForSeconds(2f);
      bossText.text = "";
      for (int i = 0; i < 2; i++) {
         playerText.text = ".";
         yield return new WaitForSeconds(.5f);
         playerText.text = "..";
         yield return new WaitForSeconds(.5f);
         playerText.text = "...";   
         yield return new WaitForSeconds(.5f);
      }

      playerText.text = "";
      yield return new WaitForSeconds(1f);
      bossText.text = "If you have a problem,";
      AudioManager.Instance.PlaySFXAtPointUI(Resources.Load<AudioClip>("Audio/talk2"), Random.Range(1.6f, 2.0f));
      yield return new WaitForSeconds(1.5f);
      bossText.text = "then settle it with the higher-ups!";
      AudioManager.Instance.PlaySFXAtPointUI(Resources.Load<AudioClip>("Audio/talk3"), Random.Range(1.6f, 2.0f));
      yield return new WaitForSeconds(2f);
      bossText.text = "";
      playerText.text = "Ok.";
      AudioManager.Instance.StopMusic();
      yield return new WaitForSeconds(1.5f);
      playerText.text = "";
      bossText.text = "?!!";
      AudioManager.Instance.PlaySFXAtPointUI(Resources.Load<AudioClip>("Audio/talk1"), Random.Range(1.6f, 2.0f), 1f);
      yield return new WaitForSeconds(1f);
      SceneManager.LoadScene(3);
   }

   public void OnMasterSliderUpdate() {
      AudioManager.Instance.SetMasterVolumeSetting(sliders[0].value); 
   }

   public void OnBGMSliderUpdate() {
      AudioManager.Instance.SetBGMVolumeSetting(sliders[1].value);
   }

   public void OnSFXSliderUpdate() {
      AudioManager.Instance.SetSFXVolumeSetting(sliders[2].value);
   }
   
   // settings screen x is 145
   public void OnSettingsPressed() {
      animator.Play("PressSettings");
   }

   public void OnLeaveSettingsPressed() {
      animator.Play("ExitSettings");
   }

   public void SkipToAction() {
      AudioManager.Instance.StopMusic();
      SceneManager.LoadScene(3);
   }
   
}
