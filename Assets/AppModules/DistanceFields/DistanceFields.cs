using System.Collections.Generic;
using Leap.Unity.Attributes;
using UnityEngine;

namespace Leap.Unity.DistanceFields.DistanceFieldsImpl {

  public enum OpType {
    Sphere
  }

  public struct DistanceFieldOp {
    public OpType type;
    public Vector3 radius;
  }

  public interface IDistanceField {
    void GetOps(List<DistanceFieldOp> ops);
  }

  public class DistanceFieldFilter : MonoBehaviour, IDistanceField {

    public OpType opType = OpType.Sphere;
  
    [DisableIf("opType", isNotEqualTo: OpType.Sphere)]
    [MinValue(0f)]
    public float radius = 1f;

    void IDistanceField.GetOps(List<DistanceFieldOp> ops) {
      ops.Add(new DistanceFieldOp() {
        type = opType,
        radius = Vector3.right * radius
      });
    }

  }

}
