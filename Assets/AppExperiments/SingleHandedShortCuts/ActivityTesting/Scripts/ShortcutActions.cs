using System;
using System.Collections;
using System.Collections.Generic;
using Leap.Unity;
using UnityEngine;
using LeapSingleHandedShortcuts;

public class ShortcutActions : MonoBehaviour
{

    //Helper script to consolidate both hands into one script, see PowerBall on each Attachment hand for more events: OnSelectShortCut, OnActivatePower, etc.

    public PowerBall leftPowerball;
    public PowerBall rightPowerball;

    //Corresponding SELECTED and ACTIVATED are triggered by the public events in PowerBall.cs:

    //============= SELECTED - when shortcut cursor highlights over quadrant. Note, this fires if it's selected and you try to bring up menu again
    public void ShortcutSelectedLeft(Int32 shortcutID)
    {
        Debug.Log("*****LEFT hand shortcut SELECTING: " + shortcutID);
    }

    public void ShortcutSelectedRight(Int32 shortcutID)
    {
        Debug.Log("*****RIGHT hand shortcut SELECTING: " + shortcutID);
    }

    //============= ACTIVATED - when flipping hand back over

    public void ShortcutActivatedLeft(Int32 shortcutID)
    {
        Debug.Log("*****LEFT hand shortcut ACTIVATED: " + shortcutID);

    }

    public void ShortcutActivatedRight(Int32 shortcutID)
    {
        Debug.Log("*****RIGHT hand shortcut ACTIVATED: " + shortcutID);

        switch (shortcutID)
        {
            case 3:
                //enable shortcut behavior here
                break;
            case 2:

                break;
            case 1:

                break;
            default:

                break;
        }

    }

    // Update is called once per frame
    void Update()
    {
    }

}
