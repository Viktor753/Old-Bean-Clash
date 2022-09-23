using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using TMPro;

public class KillFeedObject : MonoBehaviour
{
    public Image weaponUsedDisplay;
    public TextMeshProUGUI killerNameDisplay, victimNameDisplay;
    public Color teamOneColor;
    public Color teamTwoColor;

    public void SetupUI(Sprite weapon, Player killer, Player victim)
    {
        weaponUsedDisplay.sprite = weapon;
        killerNameDisplay.text = killer.NickName;
        victimNameDisplay.text = victim.NickName;

        var killerColor = (int)killer.CustomProperties[PlayerPropertyKeys.teamIDKey] == 1 ? teamTwoColor : teamOneColor;
        var victimColor = (int)victim.CustomProperties[PlayerPropertyKeys.teamIDKey] == 1 ? teamTwoColor : teamOneColor;

        killerNameDisplay.color = killerColor;
        victimNameDisplay.color = victimColor;
    }
}
