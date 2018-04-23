using Leap.Unity.Interaction;
using UnityEngine;
using UnityEngine.Events;

namespace Leap.Unity.Examples.Lines {

  public class InteractionEvent : UnityEvent<InteractionHand, InteractionBehaviour> { }

  /// <summary>
  /// A UnityEvent containing arguments for the InteractionHand that performed the
  /// interaction, the InteractionBehaviour object interacted with, and the point at
  /// which the interaction occurred.
  /// </summary>
  [System.Serializable]
  public class PointInteractionEvent : UnityEvent<InteractionHand, InteractionBehaviour, Vector3> { }

}