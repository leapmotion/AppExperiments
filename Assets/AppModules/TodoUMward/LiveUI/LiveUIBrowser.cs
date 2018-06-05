using Leap.Unity;
using Leap.Unity.Gestures;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.LiveUI {

  public class Browser {

    #region Dev Command Gesture

    public const string LAUNCH_COMMAND_NAME = "Launch LiveUI Browser";

    [RuntimeInitializeOnLoadMethod]
    private static void RuntimeInitializeOnLoad() {

      // Register the LiveUIBrowserGesture with the associated name and action.
      DevCommandGesture.Register<LiveUIBrowserGesture>(LAUNCH_COMMAND_NAME,
                                                       LaunchNew);

    }

    #endregion

    public static void LaunchNew(Vector3 atPosition) {

      GameObject newObj = new GameObject("New Browser");
      newObj.transform.position = atPosition;
      Debug.Log("Spawned cube given position: " + atPosition.ToString("R"));

      GameObject cubeObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
      cubeObj.transform.parent = newObj.transform;
      cubeObj.transform.ResetLocalPose();
      cubeObj.transform.localScale = Vector3.one * 0.05f;

      //return Promise.ToReturn<Browser>()
      //              .WithArgs<Vector3>(atPosition)
      //              .OnThread(ThreadType.UnityThread)
      //              .Otherwise(notifyBrowserLaunchException);
    }

    //private static Browser constructBrowser() {
    //  return new Browser();
    //}

    //private static Browser constructBrowserAtPosition(Vector3 atPosition) {

    //}

    //private static void notifyBrowserLaunchException(Exception e) {
    //  throw e;
    //}

    //private Browser() {
      
    //}

  }

}