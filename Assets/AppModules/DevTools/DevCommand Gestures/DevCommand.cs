using System;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity {

  public static class DevCommand {

    #region Static Memory & Initialization

    private static Dictionary<string, Action> s_noArgCommands
             = new Dictionary<string, Action>();

    private static Dictionary<string, Action<Vector3>> s_positionCommands
             = new Dictionary<string, Action<Vector3>>();

    /// <summary>
    /// If you'd like to activate your own code, you can receive a callback on-load using
    /// Unity's built-in RuntimeInitializeOnLoadMethod attribute, or otherwise initialize
    /// a command however you'd like.
    /// 
    /// Registering a DevCommand is as simple as providing a command name and a resulting
    /// Action (or Action generic with a supported argument type) to DevCommand.Register.
    /// 
    /// This registration can occur at any time after your scene loads.
    /// </summary>
    [RuntimeInitializeOnLoadMethod]
    private static void RuntimeInitializeOnLoad() {
      DevCommand.Register("Recenter", () => { UnityEngine.XR.InputTracking.Recenter(); });
    }

    #endregion

    #region Public API

    #region Command Registration

    public static void Register(string commandName,
                                Action<Vector3> actionUsingPosition) {
      s_positionCommands[commandName] = actionUsingPosition;
    }

    public static void Register(string commandName,
                                Action commandAction) {
      s_noArgCommands[commandName] = commandAction;
    }

    #endregion

    #region Command Invocation

    public static void Invoke(string commandName) {
      Action noArgCommand;
      if (s_noArgCommands.TryGetValue(commandName, out noArgCommand)) {
        noArgCommand();
        return;
      }
      else {
        throw new System.ArgumentException(
          "[DevCommand] No command named '" + commandName + "' was found "
        + "in the no-argument command set.");
      }
    }

    public static void Invoke(string commandName, Vector3 positionArg) {
      Action<Vector3> positionCommand;
      if (s_positionCommands.TryGetValue(commandName, out positionCommand)) {
        positionCommand(positionArg);
        return;
      }
      else {
        throw new System.ArgumentException(
          "[DevCommand] No command named '" + commandName + "' was found "
        + "in the position-argument command set.");
      }
    }

    #endregion

    #endregion

  }

}
