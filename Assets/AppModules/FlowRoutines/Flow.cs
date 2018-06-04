using System.Collections.Generic;

public struct Flow {
  public readonly CoValueType Type;
  public readonly float Arg;
  public readonly object Coroutine;

  public int IntArg {
    get {
      return (int)Arg;
    }
  }

  private Flow(CoValueType type, float arg) {
    Type = type;
    Arg = arg;
    Coroutine = null;
  }

  private Flow(IEnumerator<Flow> coroutine) {
    Type = CoValueType.ForCoroutine;
    Arg = 0;
    Coroutine = coroutine;
  }

  /// <summary>
  /// Yields for a given number of seconds.  The delay value is always rounded up to the nearest frame.
  /// </summary>
  public static Flow ForSeconds(float delay) {
    return new Flow(CoValueType.ForSeconds, delay);
  }

  /// <summary>
  /// Yields for a single frame.
  /// </summary>
  public static Flow ForFrame() {
    return new Flow(CoValueType.ForFrames, 1);
  }

  /// <summary>
  /// Yields for a given number of frames.
  /// </summary>
  public static Flow ForFrames(int frames) {
    return new Flow(CoValueType.ForFrames, frames);
  }

  /// <summary>
  /// Yields until the given coroutine is complete.
  /// </summary>
  public static Flow UntilCoroutine(IEnumerator<Flow> coroutine) {
    return new Flow(coroutine);
  }

  /// <summary>
  /// Execution is resumed during the next Update callback.
  /// </summary>
  public static Flow IntoUpdate() {
    return new Flow(CoValueType.IntoUpdate, 0);
  }

  /// <summary>
  /// Execution is resumed during the next FixedUpdate callback.
  /// </summary>
  public static Flow IntoFixedUpdate() {
    return new Flow(CoValueType.IntoFixedUpdate, 0);
  }

  /// <summary>
  /// Execution is resumed during the next LateUpdate callback.
  /// </summary>
  public static Flow IntoLateUpdate() {
    return new Flow(CoValueType.IntoLateUpdate, 0);
  }

  /// <summary>
  /// Execution is resumed during the next EndOfFrame callback.
  /// </summary>
  public static Flow IntoEndOfFrame() {
    return new Flow(CoValueType.IntoEndOfFrame, 0);
  }

  /// <summary>
  /// Execution is resumed in a non-unity thread.  You must yield back to a Unity
  /// callback before you can use any Unity API or any other yield type.
  /// </summary>
  public static Flow IntoNewThread() {
    return new Flow(CoValueType.IntoNewThread, 0);
  }

  /// <summary>
  /// Execution is resumed once time reaches a certain value.
  /// </summary>
  public static Flow UntilTime(float time) {
    return new Flow(CoValueType.UntilTime, 0);
  }

  /// <summary>
  /// Execution is resumed once frame count reaches a certain value.
  /// </summary>
  public static Flow UntilFrame(int frame) {
    return new Flow(CoValueType.UntilFrame, 0);
  }

  /// <summary>
  /// Only yield if a certain amount of time has passed since the most recent yield.
  /// </summary>
  public static Flow IfElapsed(float miliseconds) {
    long ticksPerMilisecond = System.Diagnostics.Stopwatch.Frequency / 1000;
    return new Flow(CoValueType.WhenElapsed, ticksPerMilisecond * miliseconds);
  }

  public enum CoValueType {
    ForSeconds,
    ForFrames,
    ForCoroutine,
    IntoUpdate,
    IntoFixedUpdate,
    IntoLateUpdate,
    IntoEndOfFrame,
    IntoNewThread,
    UntilTime,
    UntilFrame,
    UntilCallback,
    WhenElapsed
  }
}
