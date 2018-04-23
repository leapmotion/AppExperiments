using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.UserContext {

  public class UconTestBehaviour : MonoBehaviour {

    public float foo;
    
    public FloatChannel radiusChannel;

    // could extend the idea to other collection types:
    // UconStack<float>
    // UconQueue<float>
    // UconList<float>
    // UconStream<float> + UconStreamReceiver<float>
    // UconDictionary<key, value>
    // UconHashSet<IntObj>
    // UconOrderedSet<IntObj>

    private void Start() {
      Debug.Log("Does float channel have a value on Start()? " + !radiusChannel.IsEmpty);

      Debug.Log("Pushing value 1f...");
      radiusChannel.Set(0f);

      Debug.Log("Is channel empty now? " + radiusChannel.IsEmpty);

      radiusChannel.Add(new float[] { 1f, 2f, 3f });

      Debug.Log("Channel contains: " + radiusChannel.Count + " elements (should be 4)");

      radiusChannel.Set(new float[] { 0f, 1f });

      Debug.Log("Channel contains: " + radiusChannel.Count + " elements (should be 2)");
    }

  }

}
