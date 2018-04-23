using Leap.Unity.RuntimeGizmos;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.PhysicalInterfaces {

  public class FingerSlideArm : MonoBehaviour, IRuntimeGizmoComponent {

    public Chirality whichHand = Chirality.Left;

    [Header("Cylinder Config")]
    public SlideArmCylinder[] cylinders;

    [Range(0.01f, 0.22f)]
    public float height = 0.10f;
    [Range(0.01f, 0.10f)]
    public float topRadius = 0.05f;
    [Range(0.01f, 0.10f)]
    public float bottomRadius = 0.07f;

    [Header("Arm Alignment")]
    [Range(0f, 1f)]
    public float useTrackedArmWeight = 0f;

    [Header("(Optional) Arm IK Alignment")]
    public float foreArmIKLength = 0.30f;
    public float upperArmIKLength = 0.30f;
    [Range(0f, 1f)]
    public float useIKArmWeight = 0f;

    public bool drawDebugIKArm = false;

    private void OnValidate() {
      updateCylinders();
    }

    private void Update() {
      updateCylinders();

      updatePose();
    }

    private void updateCylinders() {
      if (cylinders == null || cylinders.Length == 0) return;


      var heightPerCylinder = height / cylinders.Length;

      RuntimeGizmoDrawer debugDrawer = null;
      {
        RuntimeGizmoManager.TryGetGizmoDrawer(out debugDrawer);
      }

      for (int i = 0; i < cylinders.Length; i++) {
        var cylinder = cylinders[i];

        if (cylinder == null) { continue; }

        float cylinderIdxCoeff;
        if (cylinders.Length == 1) { cylinderIdxCoeff = 1; }
        else {
          cylinderIdxCoeff = (i / ((float)cylinders.Length - 1));
        }

        cylinder.height = heightPerCylinder;
        cylinder.width = Mathf.Lerp(bottomRadius, topRadius, cylinderIdxCoeff);
        cylinder.transform.position = this.transform.position
                                      - this.transform.up
                                        * (height - (heightPerCylinder * i) - heightPerCylinder / 2f);
      }
    }

    private Vector3[] armPointsIK = new Vector3[3];
    private float[] armLengthsIK = new float[2];
    private void updatePose() {
      var hand = Hands.Get(whichHand);

      if (hand != null) {
        this.transform.position = hand.WristPosition.ToVector3();

        var trackedArmDirection = hand.WristPosition.ToVector3().From(hand.Arm.PrevJoint.ToVector3()).normalized;
        var fakeArmDirection = hand.DistalAxis();

        var useArmDirection = Vector3.Slerp(fakeArmDirection, trackedArmDirection, useTrackedArmWeight);

        // IK arm direction (FABRIK)
        var target = hand.WristPosition.ToVector3();
        var cameraSideways = (whichHand == Chirality.Left ? -1 : 1) * Camera.main.transform.right;
        var approxShoulder = Camera.main.transform.position
                             - Camera.main.transform.up * 0.10f
                             + (cameraSideways * 0.20f);
        armPointsIK[0] = approxShoulder;
        armLengthsIK[0] = upperArmIKLength;
        armPointsIK[1] = (approxShoulder + Vector3.down * upperArmIKLength + cameraSideways * 0.15f).normalized * upperArmIKLength;
        armLengthsIK[1] = foreArmIKLength;
        armPointsIK[2] = armPointsIK[1] + Camera.main.transform.forward * foreArmIKLength;
        var fabrikChain = new IK.FABRIK.FABRIKChain(armPointsIK, armLengthsIK, target);
        IK.FABRIK.BackwardSolve(fabrikChain, warmStartDir: -fakeArmDirection);
        IK.FABRIK.ForwardSolve(fabrikChain);
        IK.FABRIK.Solve(fabrikChain);
        var ikResultArmDirection = armPointsIK[2].From(armPointsIK[1]).normalized;

        useArmDirection = Vector3.Slerp(useArmDirection, ikResultArmDirection, useIKArmWeight);

        this.transform.rotation = Quaternion.LookRotation(useArmDirection, hand.PalmarAxis())
                                            .Then(Quaternion.Euler(new Vector3(90f, 0f, 0f)));
      }
    }

    public void OnDrawRuntimeGizmos(RuntimeGizmoDrawer drawer) {
      if (drawDebugIKArm) {
        drawer.color = Color.red;
        for (int i = 0; i + 1 < armPointsIK.Length; i++) {
          drawer.DrawLine(armPointsIK[i], armPointsIK[i + 1]);
        }
      }
    }

  }

}
