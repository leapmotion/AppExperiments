using UnityEngine;

namespace Leap.Unity.Apps.Lines {

  public interface ILineCreator {

    bool isCreatingLine { get; }

    /// <summary>
    /// Begins creating a line. No positional information is provided yet.
    /// </summary>
    void BeginLine();

    /// <summary>
    /// Updates the two line segment points defining the line currently being
    /// created.
    /// It is an error to call UpdateLine() before BeginLine().
    /// </summary>
    void UpdateLine(Vector3 a, Vector3 b);

    /// <summary>
    /// Finishes and finalizes the line currently being created.
    /// </summary>
    void FinishLine();

    /// <summary>
    /// As EndLine(), but deletes the line rather than finishing its creation.
    /// </summary>
    void CancelLine();

  }

}