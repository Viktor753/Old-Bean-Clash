using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

[System.Serializable]
public class Item : MonoBehaviour
{
    public PhotonView pv;
    public float pullOutDuration = 0.35f;
    public string itemKey;
    public bool canBeDropped = true;
    public bool canBeSelected = true;
    public ItemOnGround droppedItemVersion;
    public ItemType type;
    public bool destroyOnRoundEnd = false;

    [HideInInspector] public bool itemPulledOut;

    [HideInInspector] public Animator playerHandAnimator;
    

    public virtual void OnEnable()
    {
        if (pv.IsMine)
        {
            if (playerHandAnimator == null)
            {
                playerHandAnimator = PlayerInventory.instance.inventoryHandAnimator;
            }
            Invoke(nameof(CompletePullOut), pullOutDuration);
        }
    }

    public virtual void OnDisable()
    {
        if (pv.IsMine)
        {
            CancelInvoke(nameof(CompletePullOut));
            itemPulledOut = false;
        }
    }

    private void CompletePullOut()
    {
        itemPulledOut = true;
    }

    public virtual void OnDropped()
    {

    }
}

public enum ItemType
{
    Primary,
    Secondary,
    Knife,
    Utility,
    Special,
    None
}
