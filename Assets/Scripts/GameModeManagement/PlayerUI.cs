using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerUI : MonoBehaviour
{
    public GameObject canvas;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI ammoText;
    public TextMeshProUGUI interactTextDisplay;

    private void OnEnable()
    {
        PlayerManager.OnLocalPlayerControllerSpawned += OnLocalPlayerSpawned;
        PlayerManager.OnLocalPlayerControllerDeSpawned += OnLocalPlayerDeSpawned;
        PlayerManager.SendInteractTextMessage += UpdateInteractMessage;
        PlayerInventory.OnItemSelected += OnItemSelected;
        Weapon.OnAmmoUpdated += OnAmmoUpdated;
    }

    private void OnDisable()
    {
        PlayerManager.OnLocalPlayerControllerSpawned -= OnLocalPlayerSpawned;
        PlayerManager.OnLocalPlayerControllerDeSpawned -= OnLocalPlayerDeSpawned;
        PlayerManager.SendInteractTextMessage -= UpdateInteractMessage;
        PlayerInventory.OnItemSelected -= OnItemSelected;
        Weapon.OnAmmoUpdated -= OnAmmoUpdated;

        if (canvas != null)
        {
            canvas.SetActive(false);
        }
    }

    private void OnItemSelected(Item item)
    {
        if(item.TryGetComponent<Weapon>(out var weaponComponent))
        {
            if (weaponComponent.usesAmmo)
            {
                ammoText.text = $"{weaponComponent.ammo}/{weaponComponent.baseAmmo}";
            }
            else
            {
                ammoText.text = string.Empty;
            }
        }
        else
        {
            ammoText.text = string.Empty;
        }
    }

    private void OnAmmoUpdated(int ammo, int baseAmmo)
    {
        ammoText.text = $"{ammo}/{baseAmmo}";
    }

    private void OnHealthValueChanged(int oldHealthValue, int newHealthValue)
    {
        if(healthFlashCoroutine != null)
        {
            StopCoroutine(healthFlashCoroutine);
        }


        newHealthValue = Mathf.Clamp(newHealthValue, 0, Health.localInstance.maxHealth);

        healthFlashCoroutine = StartCoroutine(HealthFlashEffect(3, 0.2f, 0.3f, newHealthValue >= oldHealthValue ? Color.green : Color.red));
        healthText.text = newHealthValue.ToString();
    }


    private Coroutine healthFlashCoroutine;
    private IEnumerator HealthFlashEffect(int flashCount, float flashDuration,float flashCooldown, Color colorToFlash)
    {
        for (int i = 0; i < flashCount; i++)
        {
            healthText.color = colorToFlash;
            yield return new WaitForSeconds(flashDuration);
            healthText.color = Color.white;
            yield return new WaitForSeconds(flashCooldown);
        }
    }

    private void OnLocalPlayerSpawned(PlayerController player)
    {
        canvas.SetActive(true);
        Health.localInstance.OnHealthChanged += OnHealthValueChanged;

        healthText.text = 100.ToString();
        ammoText.text = "0/0";
    }

    private void OnLocalPlayerDeSpawned()
    {
        canvas.SetActive(false);
        Health.localInstance.OnHealthChanged -= OnHealthValueChanged;

        healthText.text = 0.ToString();
        ammoText.text = "0/0";
    }

    private void UpdateInteractMessage(string message)
    {
        interactTextDisplay.text = message;
    }
}
