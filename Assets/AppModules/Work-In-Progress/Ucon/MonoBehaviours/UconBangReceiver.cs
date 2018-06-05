using UnityEngine;
using UnityEngine.Events;

namespace Leap.Unity.UserContext {

  /// <summary>
  /// Simple glue component that listens to a Ucon Bang channel and fires a Unity Event
  /// at most once per Update when it sees the channel is non-empty.
  /// 
  /// TODO: This channel only supports a SINGLE-CONSUMER model! And signals are consumed
  /// once per update, NOT as soon as they are sent to the channel.
  /// </summary>
  public class UconBangReceiver : MonoBehaviour {

    public BangChannel singleConsumerChannel = new BangChannel("tool/action");

    public UnityEvent onBangReceived;

    private void Update() {
      if (!singleConsumerChannel.IsEmpty) {
        singleConsumerChannel.Clear();

        onBangReceived.Invoke();
      }
    }

  }


}