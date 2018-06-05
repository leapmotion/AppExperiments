using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Meshing {
  
  // Pair:
  //   a = List of PolyMesh position indices corresponding to the keyed object's
  //       positions in the PolyMesh
  //   b = List of PolyMesh Polygon indices corresponding to the keyed object's
  //       polygons in the PolyMesh
  using PolygonData = Pair<List<int>, List<int>>;

  /// <summary>
  /// A PolyMeshObject that provides data support for adding and modifying specific
  /// submesh data to its mesh, where polygon and position indices in the underlying
  /// PolyMesh are "keyed" by an arbitrary object. These indices can later be queried
  /// and further modified or amended.
  /// </summary>
  public class KeyedPolyMeshObject : PolyMeshObject {

    #region Unity Events

    protected override void Awake() {
      base.Awake();

      _notifyPolyMeshModifiedAction = notifyPolyMeshModified;
    }

    protected override void OnDestroy() {
      base.OnDestroy();

      // Pool the polygon data lists we allocated for objectPolygonData.
      foreach (var objPolyDataPair in objectPolygonData) {
        var polygonData = objPolyDataPair.Value;

        var positionIndices = polygonData.a;
        var polygonIndices = polygonData.b;

        positionIndices.Clear();
        Pool<List<int>>.Recycle(positionIndices);

        // The individual polygon data would have been invalidated and returned to the
        // pool when the PolyMesh was cleared; so just recycle the polygons.
        polygonIndices.Clear();
        Pool<List<int>>.Recycle(polygonIndices);
      }
    }

    #endregion

    #region Object Submesh Keying

    private Dictionary<object, PolygonData> objectPolygonData
      = new Dictionary<object, PolygonData>();

    /// <summary>
    /// Adds new positions and new polygons to the underlying PolyMesh of this object,
    /// keying their indices using the provided key.
    /// 
    /// The Polygons in newPolygons should index the newPositions directly; they are
    /// both added using the PolyMesh Append() operation, which will modify the polygons
    /// in the newPolygons list (specifically, their underlying vertex indices lists will
    /// be modified to match the indices of the positions that are added to the PolyMesh).
    /// 
    /// If you'd like to retrieve this data to modify it later, provide the same key to
    /// the ModifyDataFor() method, which will return a PolyMesh object and indices into
    /// its data that you can modify freely.
    /// 
    /// Positions are never re-used by LivePolyMeshObjects, so they are less optimal than
    /// manipulating a PolyMesh directly (however, this has no impact on the resulting
    /// Unity mesh representation).
    /// </summary>
    public void AddDataFor(object key, List<Vector3> newPositions,
                                       List<Polygon> newPolygons,
                                       List<Edge>    newSmoothEdges,
                                       List<Color>   newColors = null) {
      PolygonData polygonData;
      if (objectPolygonData.TryGetValue(key, out polygonData)) {
        throw new System.InvalidOperationException(
          "Data for the provided key already exists: " + key.ToString());
      }

      var newPositionIndices = Pool<List<int>>.Spawn();
      newPositionIndices.Clear();
      var newPolygonIndices = Pool<List<int>>.Spawn();
      newPolygonIndices.Clear();
      polyMesh.Append(newPositions,
                      newPolygons,
                      newPositionIndices,
                      newPolygonIndices,
                      newSmoothEdges: newSmoothEdges,
                      newColors: newColors);
      
      objectPolygonData[key] = new PolygonData() {
        a = newPositionIndices,
        b = newPolygonIndices
      };

      RefreshUnityMesh();
    }

    private bool _modificationPending = false;
    /// <summary>
    /// Provide the object whose mesh representation you'd like to modify. You will
    /// receive a PolyMesh object, position indices into that PolyMesh, and polygon
    /// indices into that PolyMesh; these indices are the polygon mesh data associated
    /// with the keyed object.
    /// 
    /// Valid modifications currently only _ADD_ positions or polygons to the PolyMesh.
    /// You must at least re-use all of the existing positions and polygons for the
    /// keyed object (you can modify the values at the existing indices), but you can
    /// also add new positions and new polygons as long as you report them in using the
    /// callback Action.
    /// 
    /// When you are finished modifying the PolyMesh, call the provided Action, which
    /// will update the Unity mesh representation of the PolyMesh, and allow future
    /// modifications. You must also provide the indices of any additional positions or
    /// polygons you added to the PolyMesh as keyed by the key object.
    /// (Hint: PolyMesh modification methods can pass back the added indices of any new
    /// positions or polygons.)
    /// </summary>
    public void ModifyDataFor(object key,
                              out PolyMesh polyMesh,
                              List<int> keyedPositionIndices,
                              List<int> keyedPolygonIndices,
                              out NotifyPolyMeshModifiedAction
                                callWhenDoneModifyingPolyMesh) {

      if (_modificationPending) {
        throw new InvalidOperationException(
          "A PolyMesh modification is already in progress for this LivePolyMeshObject. "
          + "(Did you forget to call the Action when the modification was finished?)");
      }

      callWhenDoneModifyingPolyMesh = _notifyPolyMeshModifiedAction;
      polyMesh = this.polyMesh;

      PolygonData polyData;
      if (!objectPolygonData.TryGetValue(key, out polyData)) {
        throw new InvalidOperationException(
          "No polygon data was found for key: " + key.ToString() + "; "
          + "Did you add data for this key first?");
      }

      keyedPositionIndices.AddRange(polyData.a);
      keyedPolygonIndices.AddRange(polyData.b);
    }


    /// <summary>
    /// This Action is passed to objects that request to be able to directly modify
    /// the PolyMesh of a KeyedPolyMeshObject. You must call it when you are done
    /// modifying the PolyMesh of this PolyMeshObject. You must also provide the indices
    /// of any positions or polygons that were added to the PolyMesh.
    /// 
    /// You can provide null for the newPositionIndices or newPolygonIndices if no new
    /// positions or polygons were added.
    /// </summary>
    public delegate void NotifyPolyMeshModifiedAction(object key,
                                                      List<int> addedPositionIndices,
                                                      List<int> addedPolygonIndices);
    private NotifyPolyMeshModifiedAction _notifyPolyMeshModifiedAction;

    private void notifyPolyMeshModified(object key,
                                        List<int> newPositionIndices,
                                        List<int> newPolygonIndices) {
      _modificationPending = false;

      PolygonData polyData;
      if (!objectPolygonData.TryGetValue(key, out polyData)) {
        throw new InvalidOperationException(
          "No polygon data was found for key: " + key.ToString());
      }
      if (newPositionIndices != null) {
        polyData.a.AddRange(newPositionIndices);
      }
      if (newPolygonIndices != null) {
        polyData.b.AddRange(newPolygonIndices);
      }

      RefreshUnityMesh();
    }

    #endregion

  }
  
}
