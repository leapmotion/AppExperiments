using Leap.Unity.Splines;
using Leap.Unity.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Examples.Lines {

  [RequireComponent(typeof(SplineFragmentCapsuleGenerator))]
  public class InteractionSpline : InteractionBehaviour {

    private SplineFragmentCapsuleGenerator _splineCapsuleGen;

    protected override void Start() {
      base.Start();

      _splineCapsuleGen = GetComponent<SplineFragmentCapsuleGenerator>();
      _splineCapsuleGen.OnRefreshSplineCapsules += onRefreshSplineCapsules;
    }

    private void onRefreshSplineCapsules() {
      RefreshInteractionColliders();
    }

  }

}