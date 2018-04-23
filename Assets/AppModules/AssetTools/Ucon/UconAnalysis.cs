using Leap.Unity.Query;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Leap.Unity.UserContext {

  /// <summary>
  /// Statically receives a callback from Unity whenever scripts are reloaded (in-editor)
  /// or on application launch (runtime), at which time this class scans types that
  /// contain UconChannels and exposes this data for validation and visualization.
  /// </summary>
  public static class UconAnalysis {

    /// <summary>
    /// Wrapper around a compiled Type that has been found to contain at least one
    /// Unity-serialized UconChannel field.
    /// </summary>
    public class AnalyzedUconType : IComparable<AnalyzedUconType> {
      public Type type;
      public List<FieldInfo> channelFields;

      public int CompareTo(AnalyzedUconType other) {
        return type.Name.CompareTo(other.type.Name);
      }

      public void SortFields() {
        channelFields.Sort((a, b) => a.Name.CompareTo(b.Name));
      }
    }

    private static Type[] s_allTypes;
    private static List<AnalyzedUconType> s_channelTypes = new List<AnalyzedUconType>();
    public static ReadonlyList<AnalyzedUconType> uconChannelTypes {
      get { return s_channelTypes; }
    }

    #if UNITY_EDITOR
    [UnityEditor.Callbacks.DidReloadScripts]
    private static void OnScriptsReloaded() {
      refreshAnalysis();
    }
    #endif

    [RuntimeInitializeOnLoadMethod]
    private static void RuntimeInitializeOnLoad() {
      refreshAnalysis();
    }

    private static void refreshAnalysis() {
      // Find all types that serialize a UconChannel<>.
      s_allTypes = Assembly.GetCallingAssembly().GetTypes();

      if (s_channelTypes == null) { s_channelTypes = new List<AnalyzedUconType>(); }
      s_channelTypes.Clear();

      AnalyzedUconType curChannelType = new AnalyzedUconType();
      foreach (var type in s_allTypes) {

        // Prep the channel type object for the scan on this type.
        curChannelType.type = type;

        if (curChannelType.channelFields == null) {
          curChannelType.channelFields = new List<FieldInfo>();
        }

        // Query all fields of this type that are 'nonpublic and serializable' or 'public',
        // and that are UconChannel fields.
        foreach (var field in type.GetFields(BindingFlags.Instance
                                             | BindingFlags.NonPublic)
                                  .Query()
                                  .Where(f => f.GetCustomAttributes(
                                                  typeof(System.SerializableAttribute),
                                                  true).Length > 0)
                                  .Concat(
                                     type.GetFields(BindingFlags.Instance
                                                    | BindingFlags.Public)
                                         .Query())
                                  .Where(f => typeof(UconChannel)
                                                .IsAssignableFrom(f.FieldType))) {

          curChannelType.channelFields.Add(field);
        }

        // If we found valid Ucon channel fields in this type, remember this channel type
        // and ready a new, empty one.
        if (curChannelType.channelFields.Count > 0) {
          // Sort the channel fields.
          curChannelType.SortFields();

          s_channelTypes.Add(curChannelType);
          curChannelType = new AnalyzedUconType();
        }
      }

      // Sort the list of channel types.
      s_channelTypes.Sort();
    }

  }

}