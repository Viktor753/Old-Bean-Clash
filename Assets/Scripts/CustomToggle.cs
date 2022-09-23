using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class CustomToggle : MonoBehaviour
{
    public Image img;
    public Sprite onSprite, offSprite;

    public bool On = true;

    public UnityEvent ToggledOnEvent, ToggleOffEvent;
    
    public void UpdateUI()
    {
        img.sprite = On == true ? onSprite : offSprite;
    }

    public void Toggle()
    {
        On = !On;
        UpdateUI();
    }
}
