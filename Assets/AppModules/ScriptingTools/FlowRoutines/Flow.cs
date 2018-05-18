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

  /* Yields for a given number of seconds.  The delay value is always rounded up to the nearest frame. */
  public static Flow ForSeconds(float delay) {
    return new Flow(CoValueType.ForSeconds, delay);
  }

  /* Yields for a single frame. */
  public static Flow ForFrame() {
    return new Flow(CoValueType.ForFrames, 1);
  }

  /* Yields for a given number of frames. */
  public static Flow ForFrames(int frames) {
    return new Flow(CoValueType.ForFrames, frames);
  }

  /* Yields until the given coroutine is complete. */
  public static Flow ForCoroutine(IEnumerator<Flow> coroutine) {
    return new Flow(coroutine);
  }

  /* Execution is resumed during the next Update callback. */
  public static Flow IntoUpdate() {
    return new Flow(CoValueType.IntoUpdate, 0);
  }

  /* Execution is resumed during the next FixedUpdate callback. */
  public static Flow IntoFixedUpdate() {
    return new Flow(CoValueType.IntoFixedUpdate, 0);
  }

  /* Execution is resumed during the next LateUpdate callback. */
  public static Flow IntoLateUpdate() {
    return new Flow(CoValueType.IntoLateUpdate, 0);
  }

  /* Execution is resumed during the next EndOfFrame callback. */
  public static Flow IntoEndOfFrame() {
    return new Flow(CoValueType.IntoEndOfFrame, 0);
  }

  /* Execution is resumed in a non-unity thread.  You must yield back to a Unity
   * callback before you can use any Unity API or any other yield type.
   */
  public static Flow IntoNewThread() {
    return new Flow(CoValueType.IntoNewThread, 0);
  }

  /* Execution is resumed once time reaches a certain value. */
  public static Flow UntilTime(float time) {
    return new Flow(CoValueType.UntilTime, 0);
  }

  /* Execution is resumed once frame count reaches a certain value. */
  public static Flow UntilFrame(int frame) {
    return new Flow(CoValueType.UntilFrame, 0);
  }

  /* Only yield if a certain amount of time has passed since the most recent yield. */
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
