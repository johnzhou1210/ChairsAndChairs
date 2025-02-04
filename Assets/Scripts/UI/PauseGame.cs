using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class PauseGame : MonoBehaviour {
   [SerializeField] private Image backgroundImage, pauseImage;
   [SerializeField] private List<Sprite> sprites = new List<Sprite>();
   [SerializeField] private GameObject pausePanel, settingsPanel;
   [SerializeField] private TextMeshProUGUI masterText, BGMText, SFXText;
   [SerializeField] private Slider[] sliders;
   
   private float lastTimeScale = 1f;
   public bool Paused { get; private set; } = false;

   private void Start() {
      sliders[0].value = AudioManager.Instance.GetMasterVolumeSetting();
      sliders[1].value = AudioManager.Instance.GetBGMVolumeSetting();
      sliders[2].value = AudioManager.Instance.GetSFXVolumeSetting();
   }

   public void OnPauseGame() {
      if (!Paused) {
         pausePanel.SetActive(true);
         pauseImage.sprite = sprites[1];
         Paused = true;
         Time.timeScale = 0f;
         backgroundImage.color = new Color(0f, 0f, 0f, .85f);   
      } else {
         pausePanel.SetActive(false);
         settingsPanel.SetActive(false);
         pauseImage.sprite = sprites[0];
         Paused = false;
         Time.timeScale = lastTimeScale;
         backgroundImage.color = new Color(0f, 0f, 0f, 0f);
      }
      
   }

   public void OnClickSettings() {
      if (settingsPanel.activeInHierarchy) {
         settingsPanel.SetActive(false);
         pausePanel.SetActive(true);
      } else {
         settingsPanel.SetActive(true);
         pausePanel.SetActive(false);
      }
   }

   public void OnClickHome() {
      Time.timeScale = 1f;
      SceneManager.LoadScene(0);
   }

   private void Update() {
      if (!Paused) {
         lastTimeScale = Time.timeScale;
      }
      masterText.text = Mathf.Round(AudioManager.Instance.GetMasterVolumeSetting() * 100) + "%";
      BGMText.text = Mathf.Round(AudioManager.Instance.GetBGMVolumeSetting() * 100) + "%";
      SFXText.text = Mathf.Round(AudioManager.Instance.GetSFXVolumeSetting() * 100) + "%";
   }

   public void ClickSound() {
      AudioManager.Instance.PlaySFXAtPointUI(Resources.Load<AudioClip>("Audio/click"), Random.Range(0.8f, 1.2f));
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
   
}
