using Leap.Unity;
using Leap.Unity.Attachments;
using LeapSingleHandedShortcuts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideWhenNotTracked : MonoBehaviour
{
    public AttachmentHand attachmentHand;

    public GameObject itemToHide;

    void Update()
    {
        // Reactivation trigger
        if (attachmentHand.isTracked)
        {
            itemToHide.SetActive(true);
        }
        else if (!attachmentHand.isTracked)
        {
            itemToHide.SetActive(false);
        }
    }

}

