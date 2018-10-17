using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Leap.Unity {

  /// <summary>
  /// Note: Config is only supported on Windows standalone builds.
  /// </summary>
  public static class ConfigUsingJsonUtility {

    public const string CONFIG_FILE_NAME = "config.json";

    private static Dictionary<string, string> s_backingConfig;
    private static Dictionary<string, string> s_config {
      get {
        if (s_backingConfig == null) {
          s_backingConfig = new Dictionary<string, string>();
        }
        return s_backingConfig;
      }
      set {
        s_backingConfig = value;
      }
    }

    private static string configFilePath {
      get {
        if (Application.isEditor) {
          return Path.Combine(
            Path.Combine(
              upOneDirectoryLevel(
                upOneDirectoryLevel(
                  Path.GetDirectoryName(
                    System.Reflection.Assembly.GetExecutingAssembly().CodeBase
                      .Substring(8) // remove "file:\C:"
                  )
                )
              )
            ),
            CONFIG_FILE_NAME
          );
        }
        else {
          return Path.Combine(
            Path.GetDirectoryName(
              System.Reflection.Assembly.GetExecutingAssembly().CodeBase),
            CONFIG_FILE_NAME);
        }
      }
    }

    public static bool TryRead<T>(string field, out T value) {
      readConfigFile();
      
      var expectedType = typeof(T);
      string configValueString;
      if (s_config.TryGetValue(field, out configValueString)) {
        if (expectedType == typeof(bool)) {
          value = (T)(object)parseBool(configValueString);
        }
        else if (expectedType == typeof(int)) {
          value = (T)(object)parseInt(configValueString);
        }
        else if (expectedType == typeof(float)) {
          value = (T)(object)parseFloat(configValueString);
        }
        else { // expectedType == typeof(string)
          // Config supports actual string values, written in quotes, as well as
          // whole JSON objects or arrays, wrapped in {} or [].
          // Both are stored as strings.
          value = (T)(object)configValueString;
        }
        return true;
      }
      else {
        value = default(T);
        return false;
      }
    }

    public static T Read<T>(string field) {
      T output;
      if (!TryRead<T>(field, out output)) {
        throw new System.Collections.Generic.KeyNotFoundException();
      }
      else {
        return output;
      }
    }

    public static void Write<T>(string field, T value) {
      readConfigFile();

      if (value is float) {
        // Round-trip write for floats.
        s_config[field] = ((float)(object)value).ToString("R");
        Debug.Log("Wrote a float: " + s_config[field]);
      }
      else if (value is string) {
        string valueString = value.ToString().TrimStart();
        if (valueString.StartsWith("{") || valueString.StartsWith("[")) {
          // Object or array, write as-is.
        }
        else {
          // Actual string, write wrapped in quotes.
          valueString = '"' + valueString + '"';
        }
        s_config[field] = valueString;
      }
      else {
        s_config[field] = value.ToString();
      }

      writeConfigFile();
    }

    private static void readConfigFile() {
      // Ensure the file exists.
      if (!File.Exists(configFilePath)) {
        Debug.Log("Trying to write at " + configFilePath);
        File.WriteAllText(configFilePath, "{}\n");
      }

      // Load into memory.
      var configFileJson = File.ReadAllText(configFilePath);
      JsonUtility.FromJsonOverwrite(configFileJson, s_config);
    }

    private static void writeConfigFile() {
      var configFileJson = JsonUtility.ToJson(s_config, prettyPrint: true);
      configFileJson = configFileJson.Trim().Replace("    ", "  ") + "\n";
      File.WriteAllText(configFilePath, configFileJson);
    }

    private static bool parseBool(string boolString) {
      var normString = boolString.ToLower();
      if (normString.Equals("true")) {
        return true;
      }
      if (normString.Equals("false")) {
        return false;
      }
      throw new FailedToParseException(boolString, typeof(bool));
    }

    private static int parseInt(string intString) {
      int result;
      if (int.TryParse(intString, out result)) {
        return result;
      }
      else {
        throw new FailedToParseException(intString, typeof(int));
      }
    }

    private static float parseFloat(string floatString) {
      float result;
      if (float.TryParse(floatString, out result)) {
        return result;
      }
      else {
        throw new FailedToParseException(floatString, typeof(float));
      }
    }

    private static string upOneDirectoryLevel(string directoryPath) {
      Debug.Log("running on: " + directoryPath);
      if (!directoryPath.EndsWith(Path.DirectorySeparatorChar.ToString())) {
        directoryPath = directoryPath + Path.DirectorySeparatorChar;
      }
      int lastSlashIdx = -1, secondToLastSlashIdx = -1;
      for (int i = directoryPath.Length - 1; i >= 0; i--) {
        if (directoryPath[i] == Path.DirectorySeparatorChar) {
          if (lastSlashIdx == -1) {
            lastSlashIdx = i;
          }
          else {
            secondToLastSlashIdx = i;
            break;
          }
        }
      }
      if (lastSlashIdx == -1 || secondToLastSlashIdx == -1) {
        throw new Exception("The specified file path, \"" +
          directoryPath + "\", couldn't be shifted one directory level up " +
          "(couldn't find two path separators).");
      }

      return directoryPath.Substring(0, secondToLastSlashIdx) +
        directoryPath.Substring(lastSlashIdx);
    }

    public class FieldNotFoundException : System.Exception {
      public string field;
      public FieldNotFoundException(string field) {
        this.field = field;
      }
      public override string ToString() {
        return "The specified field \"" + field + "\" was not found in the " +
          "configuration file.\n" +
          base.ToString();
      }
    }

    public class FailedToParseException : System.Exception {
      public string value;
      public string typeString;
      public FailedToParseException(string value, System.Type type) {
        this.value = value;
        this.typeString = type.ToString();
      }

      public override string ToString() {
        return "Unable to parse \"" + this.value + "\" to type " + typeString +
          ".\n" +
          base.ToString();
      }

  }

  }

}
