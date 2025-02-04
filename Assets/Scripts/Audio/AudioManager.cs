using System;
using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour {
    public static AudioManager Instance;
    [SerializeField] [Range(0f, 1f)] private float masterVolume, musicVolume, sfxVolume = 1f;
    [SerializeField] AudioSource musicSource, whiteNoiseSource;
    
    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.volume = GetMusicVolume();
    }

    public void PlayMusic(AudioClip clip) {
        musicSource.clip = clip;
        musicSource.volume = GetMusicVolume();
        musicSource.loop = true;
        musicSource.Play();
    }

    public void PlayWhiteNoise() {
        whiteNoiseSource.clip = Resources.Load("audio/whitenoise") as AudioClip;
        whiteNoiseSource.loop = true;
        whiteNoiseSource.volume = GetSFXVolume() / 4f;
        whiteNoiseSource.Play();
    }

    public void StopMusic() {
        musicSource.Stop();
    }

    public void PlaySFXAtPoint(Vector3 point, AudioClip clip) {
        AudioSource.PlayClipAtPoint(clip, point, GetSFXVolume());
    }

    public void PlaySFXAtPoint(Vector3 point, AudioClip clip, float pitch, float startPosition = 0f) {
        if (clip == null) return;
        GameObject tempAudioObj = new GameObject("TempSFX");
        tempAudioObj.transform.position = point;
        AudioSource source = tempAudioObj.AddComponent<AudioSource>();
        source.clip = clip;
        source.pitch = pitch;
        source.volume = GetSFXVolume();
        source.spatialBlend = 1f;
        source.time = startPosition;
        source.Play();
        Destroy(tempAudioObj, source.clip.length / pitch);
    }
    
    public void PlaySFXAtPointUI(AudioClip clip, float pitch, float startPosition = 0f) {
        if (clip == null) return;
        GameObject tempAudioObj = new GameObject("TempSFX");
        tempAudioObj.transform.position = Vector3.zero;
        AudioSource source = tempAudioObj.AddComponent<AudioSource>();
        source.clip = clip;
        source.pitch = pitch;
        source.volume = GetSFXVolume();
        source.spatialBlend = 0f;
        source.time = startPosition;
        source.Play();
        Destroy(tempAudioObj, source.clip.length / pitch);
    }

    public void PlaySFXAtPointUIUntil(AudioClip clip, float pitch, IDamageable entity) {
        if (clip == null) return;
        GameObject tempAudioObj = new GameObject("TempSFX");
        tempAudioObj.transform.position = Vector3.zero;
        AudioSource source = tempAudioObj.AddComponent<AudioSource>();
        source.clip = clip;
        source.pitch = pitch;
        source.volume = GetSFXVolume();
        source.spatialBlend = 0f;
        source.Play();

        StartCoroutine(DontDestroyUntilDead(tempAudioObj, entity));
        
        Destroy(tempAudioObj, source.clip.length / pitch);
    }

    private bool CheckEntityAlive(GameObject tempAudioObj, IDamageable condition) {
        if (tempAudioObj == null) return false;
       return !tempAudioObj.GetComponent<AudioSource>().isPlaying || condition.GetHealthStats().Item1 == 0;
    }
    
    private IEnumerator DontDestroyUntilDead(GameObject tempAudioObj, IDamageable condition) {
        yield return new WaitUntil(() => CheckEntityAlive(tempAudioObj, condition));
        if (tempAudioObj) {
            Destroy(tempAudioObj);    
        }
    }
    

    public float GetMusicVolume() {
        return musicVolume * masterVolume;
    }

    public float GetSFXVolume() {
        return sfxVolume * masterVolume;
    }

    public float GetMasterVolumeSetting() {
        return masterVolume;
    }

    public float GetBGMVolumeSetting() {
        return musicVolume;
    }

    public float GetSFXVolumeSetting() {
        return sfxVolume;
    }

    public void SetBGMVolumeSetting(float volume) {
        musicVolume = volume;
        musicSource.volume = GetMusicVolume();
    }
    
    public void SetSFXVolumeSetting(float volume) {
        sfxVolume = volume;
    }
    
    public void SetMasterVolumeSetting(float volume) {
        masterVolume = volume;
        musicSource.volume = GetMusicVolume();
    }
    
    
}