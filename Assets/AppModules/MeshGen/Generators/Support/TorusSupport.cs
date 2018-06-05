using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.MeshGen {

  public static partial class Generators {

    private static class TorusSupport {

      private static int getMaxNumMinorSegments(int numMinorSegments,
                                                float maxMinorArcAngle,
                                                out bool shouldCloseLoop) {
        numMinorSegments = Mathf.Max(numMinorSegments, 2);
        maxMinorArcAngle = Mathf.Clamp(maxMinorArcAngle, 0f, 360f);
        float anglePerMinorStep = 360f / numMinorSegments;
        int actualNumMinorSegments = 0;
        float totalAngle = 0f;
        while (totalAngle <= maxMinorArcAngle) {
          actualNumMinorSegments += 1;
          totalAngle += anglePerMinorStep;
        }

        if (maxMinorArcAngle < 360f) shouldCloseLoop = false;
        else                         shouldCloseLoop = true;

        return actualNumMinorSegments;
      }

      public static void AddIndices(List<int> outIndices, int startingVertCount,
                                    int numMajorSegments, int numMinorSegments,
                                    float maxMinorArcAngle = 360f) {
        List<int> i = outIndices;
        int v0 = startingVertCount;

        // Apply maxMinorArcAngle to get the actual number of minor segments to use.
        bool closeLoop;
        int maxNumMinorSegments = getMaxNumMinorSegments(numMinorSegments,
                                                         maxMinorArcAngle,
                                                         out closeLoop);
        if (maxNumMinorSegments == 0) return;
        int maybeMinusOneSegment = closeLoop ? 0 : -1;

        int totalNumVerts = (maxNumMinorSegments + 1) * numMajorSegments;

        
        int ring0StartIdx = 0;
        int ring1StartIdx = maxNumMinorSegments + 1;
        for (int m = 0; m < numMajorSegments; m++) {
          for (int n = 0; n < maxNumMinorSegments + maybeMinusOneSegment; n++) {
            int a = ring0StartIdx + n;
            int b = ring0StartIdx + n + 1;
            int c = ring1StartIdx + n;
            int d = ring1StartIdx + n + 1;
            a %= totalNumVerts;
            b %= totalNumVerts;
            c %= totalNumVerts;
            d %= totalNumVerts;
            i.AddTri(v0, a, c, b);
            i.AddTri(v0, b, c, d);
          }

          ring0StartIdx += (maxNumMinorSegments + 1);
          ring1StartIdx += (maxNumMinorSegments + 1);
        }
      }

      public static void AddVerts(List<Vector3> outVerts, List<Vector3> outNormals,
                                  List<Vector2> outUVs,
                                  float majorRadius, int numMajorSegments,
                                  float minorRadius, int numMinorSegments,
                                  float minorStartAngle = 0f,
                                  float maxMinorArcAngle = 360f) {
        List<Vector3> v = outVerts;

        // Apply maxMinorArcAngle to get the actual numbert of minor segments to use.
        bool unused_closeLoop;
        int maxNumMinorSegments = getMaxNumMinorSegments(numMinorSegments,
                                                         maxMinorArcAngle,
                                                         out unused_closeLoop);
        //int maybeMinusOneSegment = closeLoop ? 0 : -1;

        Vector3 majorNormal = Vector3.up;
        Vector3 majorRadialDir = Vector3.right;
        Vector3 minorNormal = Vector3.forward;
        float majorTheta = 360F / numMajorSegments;
        float minorTheta = 360F / numMinorSegments;

        Quaternion majorRotation = Quaternion.AngleAxis(majorTheta, majorNormal);
        for (int i = 0; i < numMajorSegments; i++) {
          for (int j = 0; j < maxNumMinorSegments + 1; j++) {

            var effRadialJ = j;
            if (j == maxNumMinorSegments) effRadialJ = 0;
            Vector3 minorRadial = Quaternion.AngleAxis(minorTheta
                                                       * effRadialJ + minorStartAngle,
                                                       minorNormal)
                                  * (majorRadialDir * minorRadius);

            v.Add((majorRadialDir * majorRadius) + minorRadial);

            outNormals.Add(minorRadial.normalized);

            outUVs.Add(Swizzle.Swizzle.xz(majorRadialDir * 0.2f
                                          + ((float)j).Map(0, numMinorSegments,
                                                           0f, 0.8f)
                                            * majorRadialDir));
          }

          majorRadialDir = majorRotation * majorRadialDir;
          minorNormal = majorRotation * minorNormal;
        }
      }
    }

  }

}