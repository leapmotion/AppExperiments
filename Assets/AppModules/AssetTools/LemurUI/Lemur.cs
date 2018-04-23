using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Leap.Unity.LemurUI {

  /// <summary>
  /// Static API for LemurUI. Use this to, e.g., spawn new UI elements at runtime, or
  /// to replace the default Lemur types with your own custom types.
  /// </summary>
  public static class Lemur {

    private static Dictionary<Type, Type> _baseToDefault = new Dictionary<Type, Type>();

    static Lemur() {
      SetDefault(typeof(Label), typeof(Label<TextMesh, LabelTextMeshDriver>));
    }

    public static void SetDefault(Type abstractType, Type defaultInstanceType) {
      if (!abstractType.IsAssignableFrom(defaultInstanceType)) {
        throw new ArgumentException(
          "defaultInstanceType argument must be assignable to abstractType. "
        + "(" + defaultInstanceType.Name + " cannot be assigned as a "
        + abstractType.Name + ")");
      }
      if (!defaultInstanceType.IsClass
          || defaultInstanceType.IsAbstract
          || defaultInstanceType.ContainsGenericParameters
          || defaultInstanceType.GetConstructor(Type.EmptyTypes) == null) {
        throw new ArgumentException(
          "The default instance type (" + defaultInstanceType.Name + ") must have a "
        + "public parameterless constructor in order to be a valid default.");
      }

      _baseToDefault[abstractType] = defaultInstanceType;
    }

    public static T Default<T>() where T : LemurUIElement, IDefaultableLemurType {
      Type toConstruct;
      if (_baseToDefault.TryGetValue(typeof(T), out toConstruct)) {
        return Activator.CreateInstance(toConstruct) as T;
      }
      else {
        throw new InvalidOperationException(
          "No default instantiatable type defined for base Lemur type: " + typeof(T).Name);
      }
    }

  }

}
