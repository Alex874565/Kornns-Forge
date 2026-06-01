using System.Collections;
using DG.Tweening;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(PlayerStatusController))]
public class PlayerUIController : NetworkBehaviour
{
    [SerializeField] private Slider energy_bar;

    private PlayerStatusController statusController;
    private Coroutine updateEnergyCoroutine;

    [SerializeField] private Color lowEnergyColor = new Color(0.85f, 0.15f, 0.15f);
    [SerializeField] private Color highEnergyColor = new Color(0.2f, 0.8f, 0.2f);
    
    private Vector3 originalScale;
    private Tween energyScaleTween;

    private void Awake()
    {
        originalScale = energy_bar.transform.localScale;
    }
    
    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            if (energy_bar != null)
                energy_bar.gameObject.SetActive(false);

            return;
        }
        
        statusController = GetComponent<PlayerStatusController>();
        statusController.OnEnergyChanged += HandleEnergyChanged;
        statusController.OnGetEnergyLevel += HandleGetEnergyLevel;
        HandleEnergyChanged(0f, statusController.GetEnergyLevel());
    }
    
    private void HandleEnergyChanged(float oldValue, float newValue)
    {
        if (!IsOwner) return;

        if (updateEnergyCoroutine != null)
            StopCoroutine(updateEnergyCoroutine);

        updateEnergyCoroutine = StartCoroutine(UpdateEnergyBar(oldValue, newValue));
    }

    private void HandleGetEnergyLevel(float value)
    {
        if (value <= 0)
        {
            PulseEnergyBar(2, .1f);
        }
    }
    
    private IEnumerator UpdateEnergyBar(float from, float to)
    {
        if (energy_bar == null)
            yield break;
        
        float time = 0f;
        float duration = 0.2f;
        
        PulseEnergyBar(1, duration);

        while (time < duration)
        {
            float value = Mathf.Lerp(from, to, time / duration);

            energy_bar.value = value;
            SetEnergyBarColor(value);

            time += Time.deltaTime;
            yield return null;
        }

        energy_bar.value = to;
        SetEnergyBarColor(to);
    }
    
    private void SetEnergyBarColor(float value)
    {
        Image energyFillImage = energy_bar.fillRect.GetComponentInChildren<Image>();
        if (energyFillImage == null) return;

        float normalized = Mathf.Clamp01(value / energy_bar.maxValue);

        energyFillImage.color = Color.Lerp(lowEnergyColor, highEnergyColor, normalized);
    }

    private void PulseEnergyBar(int pulseTimes, float duration)
    {
        energyScaleTween?.Kill();
        energy_bar.transform.localScale = originalScale;

        energyScaleTween = energy_bar.transform
            .DOScale(originalScale * 1.2f, duration)
            .SetLoops(pulseTimes * 2, LoopType.Yoyo);
    }
}