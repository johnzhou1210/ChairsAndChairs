using System;
using System.Collections;
using DG.Tweening;
using KBCore.Refs;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class WeaponStatusRender : MonoBehaviour {
    public static WeaponStatusRender Instance;
    
    private Transform ring;
    private GameObject weaponIcon;
    private Image ammoFill, frenzyFill, maxGaugeEffect;
    [SerializeField, Child] private TextMeshProUGUI ammoText;
    [SerializeField] private Color ammoFillColor = new Color(252f/255f, 173f/255f, 15f/255f, 1f), ammoReloadingColor = new Color(252f/255f/1.1f, 173f/255f/1.1f, 15f/255f/1.1f, 1f);

    private Coroutine frenzyGlowCoroutine;
    
    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(this);
        }
        ring = transform.Find("Ring");
        ammoFill = ring.Find("Ammo").Find("AmmoFill").GetComponent<Image>();
        weaponIcon = ring.Find("IconContainer").Find("Icon").gameObject;
        frenzyFill = ring.Find("Gauge").Find("GaugeFill").GetComponent<Image>();
        maxGaugeEffect = transform.Find("MaxGaugeEffect").GetComponent<Image>();
    }

    private void Start() {
        
    }

    private void OnValidate() { this.ValidateRefs(); }

    private void OnEnable() {
        PlayerInput.OnUpdateAmmo += UpdateAmmo;
        PlayerInput.OnWeaponReload += SpinIcon;
        PlayerProjectile.OnUpdateFrenzy += UpdateFrenzy;
    }

    private void OnDisable() {
        PlayerInput.OnUpdateAmmo -= UpdateAmmo;
        PlayerInput.OnWeaponReload -= SpinIcon;
        PlayerProjectile.OnUpdateFrenzy -= UpdateFrenzy;
    }

    private void UpdateAmmo(int currAmmo, int maxAmmo) {
        ammoText.gameObject.transform.DOScale(Vector3.one * 1.5f, .05f).OnComplete(() => {
            ammoText.gameObject.transform.DOScale(Vector3.one, .05f);
        });
        weaponIcon.transform.DOScale(Vector3.one * 1.8f, .05f).OnComplete(() => {
            weaponIcon.transform.DOScale(Vector3.one * 1.4f, .05f);
        });
        
        ammoFill.fillAmount = currAmmo / (float)maxAmmo;
        ammoText.text = currAmmo.ToString();
    }

    private void SpinIcon(float reloadWaitTime) {
        StartCoroutine(ReloadAnimation(reloadWaitTime - .5f));
    }
    
    private IEnumerator ReloadAnimation(float reloadWaitTime) { // Done here because using Unity Animation causes issues
        // 720 degrees in reloadWaitTime
        ammoFill.color = ammoReloadingColor;
        Vector3 initialRotation = weaponIcon.transform.localEulerAngles;
        for (float i = 0; i < 1f; i += .01f) {
            weaponIcon.transform.rotation = Quaternion.Euler(0,0, Mathf.Lerp(initialRotation.z, initialRotation.z + 1440f, i));
            yield return new WaitForSeconds(reloadWaitTime / 100f);
        }

        ammoFill.color = ammoFillColor;
        yield return null;
    }

    private void UpdateFrenzy(int currFrenzy) {
        frenzyFill.fillAmount = currFrenzy / (float)PlayerStats.FrenzyThreshold;
        if (Mathf.Approximately(frenzyFill.fillAmount, 1f)) {
            frenzyGlowCoroutine = StartCoroutine(FrenzyGaugeGlow());
        }
    }

    public void DepleteFrenzyAnimation(float frenzyDuration) {
        AudioManager.Instance.PlaySFXAtPointUI(Resources.Load<AudioClip>("Audio/gust"), 1f);
        maxGaugeEffect.enabled = false;
        if (frenzyGlowCoroutine != null) {
            StopCoroutine(frenzyGlowCoroutine);
            frenzyGlowCoroutine = null;
        }
        DOTween.To(() => frenzyFill.fillAmount, x => frenzyFill.fillAmount = x, 0f, frenzyDuration).OnComplete(() => {
            frenzyFill.color = Color.red;
        });
    }

    public IEnumerator FrenzyGaugeGlow() {
        AudioManager.Instance.PlaySFXAtPointUI(Resources.Load<AudioClip>("Audio/shing"), Random.Range(.8f, 1.2f));
        maxGaugeEffect.enabled = true;
        Color color1 = Color.red, color2 = Color.white;
        while (true) {
            float t = Mathf.PingPong(Time.time / 2f, 1f);
            float t2 = Mathf.PingPong(Time.time / .5f, 1f);
            frenzyFill.color = Color.Lerp(color1, color2, t);
            yield return new WaitForSeconds(Time.deltaTime);
        }
    }

    public void ForceUIUpdate() {
        UpdateFrenzy(PlayerProjectile.FrenzyValue);
        
    }
    
}