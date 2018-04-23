using UnityEngine;
using System.Collections.Generic;
using System;
using Leap.Unity.Gestures;

namespace Leap.Unity {

  public class DevCommandGesturesManager : MonoBehaviour {

    #region Constants

    public const string DEV_COMMANDS_RUNNER_NAME = "__Dev Command Gestures Manager__";

    #endregion

    #region Static Memory & Initialization

    private static DevCommandGesturesManager s_instance = null;

    private static Dictionary<string, GameObject> _gestureCommands
      = new Dictionary<string, GameObject>();
    
    private static Dictionary<string, GameObject> _gesturePositionCommands
      = new Dictionary<string, GameObject>();

    private static bool s_hasOnLoadOccurred = false;

    [RuntimeInitializeOnLoadMethod]
    private static void RuntimeInitializeOnLoad() {
      var runnerObj = new GameObject(DEV_COMMANDS_RUNNER_NAME);
      s_instance = runnerObj.AddComponent<DevCommandGesturesManager>();

      // If there were any registered commands before initialization, create them now.
      s_hasOnLoadOccurred = true;
      foreach (var preloadCommandStore in s_preLoadRegisteredCommandsBuffer) {
        if (preloadCommandStore.isPositionCommand) {
          RegisterPositionCommand(preloadCommandStore.commandName,
                                  preloadCommandStore.gestureType);
        }
        else {
          RegisterCommand(preloadCommandStore.commandName,
                          preloadCommandStore.gestureType);
        }
      }
      s_preLoadRegisteredCommandsBuffer.Clear();
    }

    #endregion

    #region Pre-Loaded Registration Memory

    private struct PreLoadCommandStorage {
      public Type gestureType;
      public string commandName;
      public bool isPositionCommand;
    }

    /// <summary>
    /// Utilized when RegisterCommand is received before this manager loads; any such
    /// registrations are stored here, to be consumed when the manager actually loads.
    /// Any registrations that occur post-load can simply spawn objects immediately.
    /// (Necessary because RuntimeInitializeOnLoadMethod doesn't guarantee any order.)
    /// </summary>
    private static List<PreLoadCommandStorage> s_preLoadRegisteredCommandsBuffer
             = new List<PreLoadCommandStorage>();

    #endregion

    #region Command Registration

    /// <summary>
    /// Registers a new DevCommand Gesture with the given commandName. The type of
    /// Gesture, which must implement IGesture and at least be a MonoBehaviour, is
    /// specified by the type argument.
    /// </summary>
    public static void RegisterCommand<GestureType>(string commandName)
                         where GestureType : IGesture {
      RegisterCommand(commandName, typeof(GestureType));
    }

    /// <summary>
    /// Non-generic version of RegisterCommand for use when types aren't known at
    /// compile-time. The gestureType MUST be a MonoBehaviour AND implement IGesture!
    /// </summary>
    public static void RegisterCommand(string commandName,
                                       Type gestureType) {
      // If we haven't actually loaded up yet, just store the registration to be
      // converted into a scene object on-load.
      if (!s_hasOnLoadOccurred) {
        s_preLoadRegisteredCommandsBuffer.Add(new PreLoadCommandStorage() {
          gestureType = gestureType,
          commandName = commandName,
          isPositionCommand = false
        });

        return;
      }

      GameObject existingCommandObject;
      if (_gestureCommands.TryGetValue(commandName, out existingCommandObject)) {
        Destroy(existingCommandObject);
      }

      _gestureCommands[commandName] =
        s_instance.CreateGestureSequence(commandName, gestureType,
                                         isPositionGesture: false);
    }

    /// <summary>
    /// Registers a new DevCommand Gesture whose command takes a position argument with
    /// the given commandName. The type of Gesture, which must be a TwoHandedHeldGesture,
    /// is received as the generic type argument.
    /// </summary>
    public static void RegisterPositionCommand<GestureType>(string commandName)
                         where GestureType : TwoHandedHeldGesture {
      RegisterPositionCommand(commandName, typeof(GestureType));
    }
    /// <summary>
    /// Non-generic version of RegisterPositionCommand for use when types aren't known at
    /// compile-time. The gestureType must be a TwoHandedHeldGesture.
    /// </summary>
    public static void RegisterPositionCommand(string commandName, Type gestureType) {
      // If we haven't actually loaded up yet, just store the registration to be
      // converted into a scene object on-load.
      if (!s_hasOnLoadOccurred) {
        s_preLoadRegisteredCommandsBuffer.Add(new PreLoadCommandStorage() {
          gestureType = gestureType,
          commandName = commandName,
          isPositionCommand = true
        });

        return;
      }

      GameObject existingCommandObject;
      if (_gesturePositionCommands.TryGetValue(commandName, out existingCommandObject)) {
        Destroy(existingCommandObject);
      }

      _gesturePositionCommands[commandName] =
        s_instance.CreateGestureSequence(commandName, gestureType,
                                         isPositionGesture: true);
    }

    #endregion

    #region Gesture Sequence Creation

    private GameObject CreateGestureSequence<GestureType>(string commandName,
                                                          bool isPositionGesture)
                         where GestureType : IGesture {
      return CreateGestureSequence(commandName, typeof(GestureType), isPositionGesture);
    }
    private GameObject CreateGestureSequence(string commandName,
                                             Type gestureType,
                                             bool isPositionGesture) {
      GameObject sequenceObj = new GameObject("Dev Command: " + commandName);
      sequenceObj.transform.parent = this.transform;
      sequenceObj.transform.ResetLocalTransform();

      // Construct gesture sequence object and gesture sequence "graph",
      // really just a sequence of Node structs with a gesture and expected duration
      // between the completion of each gesture.
      var gestureSequence = sequenceObj.AddComponent<GestureSequence>();
      var gestureSequenceGraph = new GestureSequence.GestureSequenceNode[2];

      var baseGestureNode = new GestureSequence.GestureSequenceNode();
      baseGestureNode.name = "Dev Command Gesture";
      baseGestureNode.waitDuration = GestureSequence.DEFAULT_GESTURE_HOLD_DURATION;
      baseGestureNode.gesture = sequenceObj.AddComponent<DevCommandGesture>();
      gestureSequenceGraph[0] = baseGestureNode;

      var commandGestureNode = new GestureSequence.GestureSequenceNode();
      commandGestureNode.name = commandName + " Gesture";
      commandGestureNode.waitDuration = GestureSequence.DEFAULT_GESTURE_HOLD_DURATION;
      commandGestureNode.gesture = sequenceObj.AddComponent(gestureType) as IGesture;
      if (commandGestureNode.gesture == null) {
        throw new System.InvalidCastException(
          "Failed to create DevCommand gesture sequence; " + gestureType.Name + " is not "
        + "a MonoBehaviour (can't AddComponent).");
      }
      gestureSequenceGraph[1] = commandGestureNode;

      gestureSequence.sequenceGraph = gestureSequenceGraph;

      // Finally, add the gesture trigger component to trigger the dev command when the
      // gesture sequence fires.
      if (!isPositionGesture) {
        var gestureTrigger = sequenceObj.AddComponent<DevCommandGestureTrigger>();
        gestureTrigger.gesture = gestureSequence;
        gestureTrigger.devCommandName = commandName;
      }
      else {
        var gestureTrigger = sequenceObj.AddComponent<DevPositionCommandGestureTrigger>();
        gestureTrigger.gesture = gestureSequence;
        gestureTrigger.devCommandName = commandName;
      }

      return sequenceObj;
    }

    #endregion

  }

}

