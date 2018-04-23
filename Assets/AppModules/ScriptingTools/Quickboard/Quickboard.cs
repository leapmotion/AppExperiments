using System.Collections.Generic;

namespace Leap.Unity {

  public class Quickboard {

    #region Static Board & Memory

    private static Quickboard _backingStaticBoard = null;
    /// <summary>
    /// There is always a static Quickboard context (lazily constructed). You can access
    /// it with this property, but it is usually more convenient to use the static
    /// Quickboard methods, e.g. Get, Set, Has, Remove.
    /// 
    /// Use Quickboard.Context(string or object) or Quickboard.C(string or object) to get
    /// a Quickboard for a particular object context or string context.
    /// </summary>
    public static Quickboard staticBoard {
      get {
        if (_backingStaticBoard == null) {
          _backingStaticBoard = new Quickboard();
        }
        return _backingStaticBoard;
      }
    }

    private static Dictionary<object, QuickboardObjContext> _objContexts = new Dictionary<object, QuickboardObjContext>();
    private static Dictionary<string, QuickboardStrContext> _strContexts = new Dictionary<string, QuickboardStrContext>();

    #endregion

    #region Contexts

    public interface IQuickboardContext {
      Quickboard QB();
    }

    public struct QuickboardStrContext : IQuickboardContext {
      public string contextStr;
      public Quickboard quickboard;

      Quickboard IQuickboardContext.QB() { return quickboard; }
    }

    public struct QuickboardObjContext : IQuickboardContext {
      public object contextObj;
      public Quickboard quickboard;

      Quickboard IQuickboardContext.QB() { return quickboard; }
    }

    public static QuickboardObjContext Context<T>(T contextObj) where T : new() {
      QuickboardObjContext context;
      if (_objContexts.TryGetValue(contextObj, out context)) {
        return context;
      }
      else {
        context = new QuickboardObjContext() {
          contextObj = contextObj,
          quickboard = new Quickboard()
        };
        _objContexts[contextObj] = context;
        return context;
      }
    }

    /// <summary>
    /// Gets the Quickboard associated with the context string. A new, empty Quickboard
    /// will be created if the context string has never been seen before.
    /// </summary>
    public static QuickboardStrContext Context(string contextStr) {
      QuickboardStrContext context;
      if (_strContexts.TryGetValue(contextStr, out context)) {
        return context;
      }
      else {
        context = new QuickboardStrContext() {
          contextStr = contextStr,
          quickboard = new Quickboard()
        };
        _strContexts[contextStr] = context;
        return context;
      }
    }

    /// <summary>
    /// Short-hand for getting the Quickboard context for the argument object reference.
    /// </summary>
    public static QuickboardObjContext C(object contextObj) {
      return Context(contextObj);
    }

    /// <summary>
    /// Short-hand for getting the Quickboard context for the argument string.
    /// </summary>
    public static QuickboardStrContext C(string contextStr) {
      return Context(contextStr);
    }

    #endregion

    #region Instances

    private Dictionary<string, int>    _strInts   = new Dictionary<string, int>();
    private Dictionary<string, float>  _strFloats = new Dictionary<string, float>();
    private Dictionary<string, string> _strStrs   = new Dictionary<string, string>();
    private Dictionary<string, object> _strObjs   = new Dictionary<string, object>();

    private Quickboard() { }

    private Dictionary<string, object> _typedBoards = new Dictionary<string, object>();

    /// <summary>
    /// Returns a cached Quickboard for a generic type T. For internal use only.
    /// </summary>
    public Quickboard<T> ForType<T>() {
      object typedBoardObj;
      var typeName = typeof(T).FullName;
      if (_typedBoards.TryGetValue(typeName, out typedBoardObj)) {
        return typedBoardObj as Quickboard<T>;
      }
      else {
        var newTypedBoard = Quickboard<T>.CreateTypedQuickboard();
        _typedBoards.Add(typeName, newTypedBoard);
        return newTypedBoard;
      }
    }

    #endregion

    #region Static API

    #region Set

    #region Instance

    public void Instance_SetInt(string key, int value) {
      _strInts[key] = value;
    }
    public void Instance_SetFloat(string key, float value) {
      _strFloats[key] = value;
    }
    public void Instance_SetStr(string key, string value) {
      _strStrs[key] = value;
    }
    public void Instance_SetObj(string key, object value) {
      _strObjs[key] = value;
    }

    #endregion

    /// <summary>
    /// Sets a named integer value on the static Quickboard.
    /// </summary>
    public static void Set(string key, int value) {
      staticBoard.Instance_SetInt(key, value);
    }

    /// <summary>
    /// Sets a named float value on the static Quickboard.
    /// </summary>
    public static void Set(string key, float value) {
      staticBoard._strFloats[key] = value;
    }

    /// <summary>
    /// Sets a named string value on the static Quickboard.
    /// </summary>
    public static void Set(string key, string value) {
      staticBoard.Instance_SetStr(key, value);
    }

    /// <summary>
    /// Sets a named object reference value on the static Quickboard.
    /// </summary>
    public static void Set(string key, object value) {
      staticBoard.Instance_SetObj(key, value);
    }

    #endregion

    #region Get

    #region Instance

    public int Instance_GetInt(string key) {
      int value;
      if (_strInts.TryGetValue(key, out value)) {
        return value;
      }
      else {
        return default(int);
      }
    }
    public float Instance_GetFloat(string key) {
      float value;
      if (_strFloats.TryGetValue(key, out value)) {
        return value;
      }
      else {
        return default(float);
      }
    }
    public string Instance_GetStr(string key) {
      string value;
      if (_strStrs.TryGetValue(key, out value)) {
        return value;
      }
      else {
        return default(string);
      }
    }
    public object Instance_GetObj(string key) {
      object value;
      if (_strObjs.TryGetValue(key, out value)) {
        return value;
      }
      else {
        return default(object);
      }
    }

    #endregion

    /// <summary>
    /// Gets the last integer value that was Set using the specified key.
    /// 
    /// If no key has been set, the value will be the default value. Use "Has" methods to
    /// check if a key has ever been added to a Quickboard.
    /// </summary>
    public static int GetInt(string key) {
      return staticBoard.Instance_GetInt(key);
    }

    /// <summary>
    /// Gets the last float value that was Set using the specified key.
    /// 
    /// If no key has been set, the value will be the default value. Use "Has" methods to
    /// check if a key has ever been added to a Quickboard.
    /// </summary>
    public static float GetFloat(string key) {
      return staticBoard.Instance_GetFloat(key);
    }

    /// <summary>
    /// Gets the last string value that was Set using the specified key.
    /// 
    /// If no key has been set, the value will be the default value. Use "Has" methods to
    /// check if a key has ever been added to a Quickboard.
    /// </summary>
    public static string GetStr(string key) {
      return staticBoard.Instance_GetStr(key);
    }


    /// <summary>
    /// Gets the last object reference value that was Set using the specified key.
    /// 
    /// If no key has been set, the value will be the default value. Use "Has" methods to
    /// check if a key has ever been added to a Quickboard.
    /// </summary>
    public static object GetObj(string key) {
      return staticBoard.Instance_GetObj(key);
    }

    #endregion

    #region Has

    #region Instance
    
    public bool Instance_HasInt(string key) {
      return _strInts.ContainsKey(key);
    }

    public bool Instance_HasFloat(string key) {
      return _strFloats.ContainsKey(key);
    }

    public bool Instance_HasStr(string key) {
      return _strStrs.ContainsKey(key);
    }

    public bool Instance_HasObj(string key) {
      return _strObjs.ContainsKey(key);
    }

    #endregion

    /// <summary>
    /// Returns whether the static Quickboard has an integer value with the
    /// argument key.
    /// </summary>
    public static bool HasInt(string key) {
      return staticBoard.Instance_HasInt(key);
    }

    /// <summary>
    /// Returns whether the static Quickboard has an float value with the
    /// argument key.
    /// </summary>
    public static bool HasFloat(string key) {
      return staticBoard.Instance_HasFloat(key);
    }

    /// <summary>
    /// Returns whether the static Quickboard has an string value with the
    /// argument key.
    /// </summary>
    public static bool HasStr(string key) {
      return staticBoard.Instance_HasStr(key);
    }

    /// <summary>
    /// Returns whether the static Quickboard has an object reference value with the
    /// argument key.
    /// </summary>
    public static bool HasObj(string key) {
      return staticBoard.Instance_HasObj(key);
    }

    #endregion

    #region Remove

    #region Instance

    public bool Instance_RemoveInt(string key) {
      return _strInts.Remove(key);
    }
    public bool Instance_RemoveFloat(string key) {
      return _strFloats.Remove(key);
    }
    public bool Instance_RemoveStr(string key) {
      return _strStrs.Remove(key);
    }
    public bool Instance_RemoveObj(string key) {
      return _strObjs.Remove(key);
    }

    public bool Instance_RemoveAll(string key) {
      bool result = false;
      result |= Instance_RemoveInt(key);
      result |= Instance_RemoveFloat(key);
      result |= Instance_RemoveStr(key);
      result |= Instance_RemoveObj(key);
      return result;
    }

    #endregion

    /// <summary>
    /// Removes the integer value at the specified key from the static Quickboard.
    /// 
    /// Returns false if there was no value at the specified key.
    /// </summary>
    public static bool RemoveInt(string key) {
      return staticBoard.Instance_RemoveInt(key);
    }

    /// <summary>
    /// Removes the float value at the specified key from the static Quickboard.
    /// 
    /// Returns false if there was no value at the specified key.
    /// </summary>
    public static bool RemoveFloat(string key) {
      return staticBoard.Instance_RemoveFloat(key);
    }

    /// <summary>
    /// Removes the string value at the specified key from the static Quickboard.
    /// 
    /// Returns false if there was no value at the specified key.
    /// </summary>
    public static bool RemoveStr(string key) {
      return staticBoard.Instance_RemoveStr(key);
    }

    /// <summary>
    /// Removes the object reference value at the specified key from the static Quickboard.
    /// 
    /// Returns false if there was no value at the specified key.
    /// </summary>
    public static bool RemoveObj(string key) {
      return staticBoard.Instance_RemoveObj(key);
    }

    /// <summary>
    /// Removes all values at the specified key from the static Quickboard.
    /// 
    /// Returns true if there were any integer, float, string, or object reference values
    /// associated with the specified key.
    /// </summary>
    public static bool RemoveAll(string key) {
      return staticBoard.Instance_RemoveAll(key);
    }

    #endregion

    #region Clear

    public void Instance_ClearAll() {
      Instance_ClearInts();
      Instance_ClearFloats();
      Instance_ClearStrs();
      Instance_ClearObjs();
    }
    public void Instance_ClearInts() {
      _strInts.Clear();
    }
    public void Instance_ClearFloats() {
      _strFloats.Clear();
    }
    public void Instance_ClearStrs() {
      _strStrs.Clear();
    }
    public void Instance_ClearObjs() {
      _strObjs.Clear();
    }

    // Note: It's probably NEVER a good idea to "clear" things from the static Quickboard,
    // since it's intended for quick use across multiple systems.
    // As a result, there are no static Clear methods.

    #endregion

    #endregion

  }

  public class Quickboard<T> {

    #region Instances

    private Dictionary<string, T> _strTs = new Dictionary<string, T>();

    private Quickboard() { }

    /// <summary>
    /// Returns a new typed Quickboard. Internal use only.
    /// </summary>
    public static Quickboard<T> CreateTypedQuickboard() {
      return new Quickboard<T>();
    }

    #endregion

    #region Static API

    #region Set

    public void Instance_Set(string key, T value) {
      _strTs[key] = value;
    }

    /// <summary>
    /// Sets a named value on the static Quickboard.
    /// </summary>
    public static void Set(string key, T value) {
      Quickboard.staticBoard.ForType<T>().Instance_Set(key, value);
    }

    #endregion

    #region Get

    public T Instance_Get(string key) {
      T value;
      if (_strTs.TryGetValue(key, out value)) {
        return value;
      }
      else {
        return default(T);
      }
    }

    /// <summary>
    /// Gets the last value that was Set using the specified key.
    /// 
    /// If no key has been set, the value will be the default value. Use "Has" methods to
    /// check if a key has ever been added to a Quickboard.
    /// </summary>
    public static T Get(string key) {
      return Quickboard.staticBoard.ForType<T>().Instance_Get(key);
    }

    #endregion

    #region Has

    public bool Instance_Has(string key) {
      return _strTs.ContainsKey(key);
    }

    /// <summary>
    /// Returns whether the static Quickboard has a value with the argument key.
    /// </summary>
    public static bool Has(string key) {
      return Quickboard.staticBoard.ForType<T>().Instance_Has(key);
    }

    #endregion

    #region Remove

    public bool Instance_Remove(string key) {
      return _strTs.Remove(key);
    }

    /// <summary>
    /// Removes the value at the specified key from the static Quickboard.
    /// 
    /// Returns false if there was no value at the specified key.
    /// </summary>
    public static bool Remove(string key) {
      return Quickboard.staticBoard.ForType<T>().Instance_Remove(key);
    }

    #endregion

    #region Clear

    public void Instance_Clear() {
      _strTs.Clear();
    }

    // Note: It's probably NEVER a good idea to "clear" things from the static Quickboard,
    // since it's intended for quick use across multiple systems.
    // As a result, there are no static Clear methods.

    #endregion

    #endregion

  }

  public static class QuickboardExtensions {

    #region Instance API

    #region Set

    /// <summary>
    /// Sets a named value on this Quickboard context. The same name can be used with
    /// different type arguments without any name collision.
    /// </summary>
    public static void Set<T>(this Quickboard.IQuickboardContext context,
                              string key, T value) {
      context.QB().ForType<T>().Instance_Set(key, value);
    }

    /// <summary>
    /// Sets a named integer value on this Quickboard context.
    /// </summary>
    public static void Set(this Quickboard.IQuickboardContext context,
                           string key, int value) {
      context.QB().Instance_SetInt(key, value);
    }

    /// <summary>
    /// Sets a named float value on this Quickboard context.
    /// </summary>
    public static void Set(this Quickboard.IQuickboardContext context,
                           string key, float value) {
      context.QB().Instance_SetFloat(key, value);
    }

    /// <summary>
    /// Sets a named string value on this Quickboard context.
    /// </summary>
    public static void Set(this Quickboard.IQuickboardContext context,
                           string key, string value) {
      context.QB().Instance_SetStr(key, value);
    }

    /// <summary>
    /// Sets a named object reference value on this Quickboard context.
    /// </summary>
    public static void Set(this Quickboard.IQuickboardContext context,
                           string key, object value) {
      context.QB().Instance_SetObj(key, value);
    }

    #endregion

    #region Get

    /// <summary>
    /// Gets the last value that was Set using the specified key. The value must have
    /// been Set with the same type argument used for this Get.
    /// 
    /// If no key has been set, the value will be the default value. Use "Has" methods to
    /// check if a key has ever been added to a Quickboard.
    /// </summary>
    public static T Get<T>(this Quickboard.IQuickboardContext context,
                           string key) {
      return context.QB().ForType<T>().Instance_Get(key);
    }

    /// <summary>
    /// Gets the last integer value that was Set using the specified key.
    /// 
    /// If no key has been set, the value will be the default value. Use "Has" methods to
    /// check if a key has ever been added to a Quickboard.
    /// </summary>
    public static int GetInt(this Quickboard.IQuickboardContext context,
                             string key) {
      return context.QB().Instance_GetInt(key);
    }

    /// <summary>
    /// Gets the last float value that was Set using the specified key.
    /// 
    /// If no key has been set, the value will be the default value. Use "Has" methods to
    /// check if a key has ever been added to a Quickboard.
    /// </summary>
    public static float GetFloat(this Quickboard.IQuickboardContext context,
                                 string key) {
      return context.QB().Instance_GetFloat(key);
    }

    /// <summary>
    /// Gets the last string value that was Set using the specified key.
    /// 
    /// If no key has been set, the value will be the default value. Use "Has" methods to
    /// check if a key has ever been added to a Quickboard.
    /// </summary>
    public static string GetStr(this Quickboard.IQuickboardContext context,
                                string key) {
      return context.QB().Instance_GetStr(key);
    }


    /// <summary>
    /// Gets the last object reference value that was Set using the specified key.
    /// 
    /// If no key has been set, the value will be the default value. Use "Has" methods to
    /// check if a key has ever been added to a Quickboard.
    /// </summary>
    public static object GetObj(this Quickboard.IQuickboardContext context,
                                string key) {
      return context.QB().Instance_GetObj(key);
    }

    #endregion

    #region Has

    /// <summary>
    /// Returns whether this Quickboard context has a value at the specified key and the
    /// provided type argument.
    /// 
    /// If there is no value at the specified key/type combination, Get methods return
    /// the default value for the type argument.
    /// </summary>
    public static bool Has<T>(this Quickboard.IQuickboardContext context,
                              string key) {
      return context.QB().ForType<T>().Instance_Has(key);
    }

    /// <summary>
    /// Returns whether this Quickboard context has an integer value at the specified key.
    /// 
    /// If there is no value at the specified key, Get methods return the default value.
    /// </summary>
    public static bool HasInt(this Quickboard.IQuickboardContext context,
                              string key) {
      return context.QB().Instance_HasInt(key);
    }

    /// <summary>
    /// Returns whether this Quickboard context has a float value at the specified key.
    /// 
    /// If there is no value at the specified key, Get methods return the default value.
    /// </summary>
    public static bool HasFloat(this Quickboard.IQuickboardContext context,
                                string key) {
      return context.QB().Instance_HasFloat(key);
    }

    /// <summary>
    /// Returns whether this Quickboard context has a string value at the specified key.
    /// 
    /// If there is no value at the specified key, Get methods return the default value.
    /// </summary>
    public static bool HasStr(this Quickboard.IQuickboardContext context,
                              string key) {
      return context.QB().Instance_HasStr(key);
    }

    /// <summary>
    /// Returns whether this Quickboard context has an object reference value at the
    /// specified key.
    /// 
    /// If there is no value at the specified key, Get methods return the default value.
    /// </summary>
    public static bool HasObj(this Quickboard.IQuickboardContext context,
                              string key) {
      return context.QB().Instance_HasObj(key);
    }

    #endregion

    #region Remove

    /// <summary>
    /// Removes the value at the specified key and type argument from this Quickboard.
    /// 
    /// Returns false if there was no value at the specified key and type argument.
    /// </summary>
    public static bool Remove<T>(this Quickboard.IQuickboardContext context,
                                 string key) {
      return context.QB().ForType<T>().Instance_Remove(key);
    }

    /// <summary>
    /// Removes the integer value at the specified key from this Quickboard.
    /// 
    /// Returns false if there was no value at the specified key.
    /// </summary>
    public static bool RemoveInt(this Quickboard.IQuickboardContext context,
                                 string key) {
      return context.QB().Instance_RemoveInt(key);
    }

    /// <summary>
    /// Removes the float value at the specified key from this Quickboard.
    /// 
    /// Returns false if there was no value at the specified key.
    /// </summary>
    public static bool RemoveFloat(this Quickboard.IQuickboardContext context,
                                   string key) {
      return context.QB().Instance_RemoveFloat(key);
    }

    /// <summary>
    /// Removes the string value at the specified key from this Quickboard.
    /// 
    /// Returns false if there was no value at the specified key.
    /// </summary>
    public static bool RemoveStr(this Quickboard.IQuickboardContext context,
                                 string key) {
      return context.QB().Instance_RemoveStr(key);
    }

    /// <summary>
    /// Removes the object reference value at the specified key from this Quickboard.
    /// 
    /// Returns false if there was no value at the specified key.
    /// </summary>
    public static bool RemoveObj(this Quickboard.IQuickboardContext context,
                                 string key) {
      return context.QB().Instance_RemoveObj(key);
    }

    #endregion

    #region Clear

    /// <summary>
    /// Clears all key/value pairs from this Quickboard that correspond to the type
    /// argument.
    /// </summary>
    public static void Clear<T>(this Quickboard.IQuickboardContext context) {
      context.QB().ForType<T>().Instance_Clear();
    }

    /// <summary>
    /// Clears the keys for all integers, floats, strings, and object references from
    /// this Quickboard.
    /// 
    /// Does not clear typed quickboard arguments. TODO: Also remove typed quickboard
    /// data with this context.
    /// </summary>
    public static void ClearAll(this Quickboard.IQuickboardContext context) {
      context.QB().Instance_ClearAll();
    }

    /// <summary>
    /// Clears all keys for integer values from this Quickboard.
    /// </summary>
    public static void ClearInts(this Quickboard.IQuickboardContext context) {
      context.QB().Instance_ClearInts();
    }

    /// <summary>
    /// Clears all keys for float values from this Quickboard.
    /// </summary>
    public static void ClearFloats(this Quickboard.IQuickboardContext context) {
      context.QB().Instance_ClearFloats();
    }

    /// <summary>
    /// Clears all keys for string values from this Quickboard.
    /// </summary>
    public static void ClearStrs(this Quickboard.IQuickboardContext context) {
      context.QB().Instance_ClearStrs();
    }

    /// <summary>
    /// Clears all keys for object reference values from this Quickboard.
    /// </summary>
    public static void ClearObjs(this Quickboard.IQuickboardContext context) {
      context.QB().Instance_ClearObjs();
    }

    #endregion

    #endregion

  }

}
