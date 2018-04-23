using UnityEngine;

namespace Leap.Unity.PhysicalInterfaces {

  public static class PivotLook {

    public struct PivotLookConstraint {
      public Pose panel;
      public Pose panelToPivot;

      public Vector3 pivotTarget;
      public Vector3 lookTarget;
      public Vector3 horizonNormal;

      public bool flip180;
    }

    /// <summary>
    /// Returns the Panel pose necessary to solve the look constraint that places
    /// the pivot defined by panelToPivot at the pivotTarget, and the panel looking at
    /// the lookTarget. Also provides the solved handle pose as an output parameter.
    /// </summary>
    public static Pose Solve(Pose panel,
                             Pose handlePose,
                             Vector3 lookTarget,
                             out Pose solvedHandlePose,
                             Maybe<Vector3> horizonNormal = default(Maybe<Vector3>),
                             int maxIterations = 8,
                             float solveAngle = 0.1f,
                             bool flip180 = false) {
      if (!horizonNormal.hasValue) {
        horizonNormal = Vector3.up;
      };
      var panelToPivot = handlePose.From(panel);

      var solved = Solve(
        new PivotLookConstraint() {
          panel = panel,
          panelToPivot = panelToPivot,
          //pivotTarget = pivotTarget.valueOrDefault,
          pivotTarget = panel.Then(panelToPivot).position,
          lookTarget = lookTarget,
          horizonNormal = horizonNormal.valueOrDefault,
          flip180 = flip180
        },
        maxIterations,
        solveAngle
      );

      solvedHandlePose = solved.panel.Then(solved.panelToPivot);

      return solved.panel;
    }

    /// <summary>
    /// Returns the Panel pose necessary to solve the look constraint that places
    /// the pivot defined by panelToPivot at the pivotTarget, and the panel looking at
    /// the lookTarget.
    /// </summary>
    public static Pose Solve(Pose panel,
                             Vector3 pivotPoint,
                             Vector3 lookTarget,
                             Maybe<Vector3> horizonNormal = default(Maybe<Vector3>),
                             int maxIterations = 8,
                             float solveAngle = 0.1f,
                             bool flip180 = false) {
      Pose outHandlePose;

      return Solve(panel, panel.Then(pivotPoint.From(panel)), lookTarget,
        out outHandlePose,
        horizonNormal, maxIterations, solveAngle, flip180);
    }

      private static PivotLookConstraint Solve(PivotLookConstraint pivotLook,
                                            int maxIterations = 8,
                                            float solveAngle = 0.1f) {
      var lookTarget = pivotLook.lookTarget;
      var pivotTarget = pivotLook.pivotTarget;
      var horizonNormal = pivotLook.horizonNormal;
      var panelToPivot = pivotLook.panelToPivot;
      var flip180 = pivotLook.flip180;

      var panelPivotSqrDist = pivotLook.panelToPivot.position.sqrMagnitude;
      var lookPivotSqrDist = (lookTarget - pivotTarget).sqrMagnitude;
      if (lookPivotSqrDist <= panelPivotSqrDist) {
        Debug.LogError("Pivot too close to look target; no solution.");
        return pivotLook;
      }
      
      var iterations = 0;
      var angleToCam = Vector3.Angle((pivotLook.panel.rotation
                                      * Vector3.forward
                                      * (flip180 ? -1 : 1)),
                                     (lookTarget - pivotLook.panel.position));
      while (angleToCam > solveAngle && iterations++ < maxIterations) {
        // Panel look at camera.
        pivotLook.panel.rotation = Utils.FaceTargetWithoutTwist(pivotLook.panel.position,
                                                                lookTarget,
                                                                horizonNormal,
                                                                flip180);

        // Restore pivot position relative to panel.
        var newPivotPosition = pivotLook.panel.Then(panelToPivot).position;

        // Shift panel by translation away from pivotTarget.
        var newPivotToPivotTarget = pivotTarget - newPivotPosition;

        pivotLook.panel = pivotLook.panel.WithPosition(pivotLook.panel.position
                                                       + newPivotToPivotTarget);

        angleToCam = Vector3.Angle((pivotLook.panel.rotation
                                    * Vector3.forward
                                    * (flip180 ? -1 : 1)),
                                     (lookTarget - pivotLook.panel.position));
      }

      return pivotLook;
    }

  }

}
