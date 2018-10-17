using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Leap.Unity {

  /// <summary>
  /// Note: Config is only supported on Windows standalone builds.
  /// </summary>
  public static class Config {

    public const string CONFIG_FILE_NAME = "config.json";
    public const string TAB = "  "; // Two spaces! :D

    /// <summary>
    /// Actual field/value pairs in the JSON file.
    /// </summary>
    private static Dictionary<string, ValueAndType> s_config {
      get {
        if (s_backingConfig == null) {
          s_backingConfig = new Dictionary<string, ValueAndType>();
        }
        return s_backingConfig;
      }
      set {
        s_backingConfig = value;
      }
    }
    private static Dictionary<string, ValueAndType> s_backingConfig;

    /// <summary>
    /// List of fields in the configuration, as well as nulls to indicate line
    /// breaks in the configuration JSON file.
    /// </summary>
    private static List<string> s_configFields {
      get {
        if (s_backingConfigFields == null) {
          s_backingConfigFields = new List<string>();
        }
        return s_backingConfigFields;
      }
      set {
        s_backingConfigFields = value;
      }
    }
    private static List<string> s_backingConfigFields;

    private static string configFilePath {
      get {
        // For both the editor and builds, the executable is two levels up from
        // the assembly directory.
        var configFilePath = Path.Combine(
          Path.Combine(
            upOneDirectoryLevel( upOneDirectoryLevel(
              Path.GetDirectoryName(
                System.Reflection.Assembly.GetExecutingAssembly().CodeBase
              )
            ))
          ),
          CONFIG_FILE_NAME
        );

        // Remove platform-specific prefix.
        var prefixLength = 0;
        #if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        prefixLength = 8; // Will remove "file:\C:" (or any other drive letter).
        #elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        prefixLength = 5; // Will remove "file:".
        #else
        Debug.LogError("Config.cs is not supported on this platform.");
        #endif
        configFilePath = configFilePath.Substring(prefixLength);

        return configFilePath;
      }
    }

    /// <summary>
    /// Attempts to read a value from the config file. If the field is found,
    /// the value passed by reference will be set to the value found in the
    /// config file and the method will return true. Otherwise, the value is
    /// not modified and the method will return false.
    /// </summary>
    public static bool TryRead<T>(string field, ref T value,
                                  bool verbose = false,
                                  Action<string> logFunc = null) {
      readConfigFile();
      
      var expectedType = typeof(T);
      ValueAndType configValueAndType;
      if (s_config.TryGetValue(field, out configValueAndType)) {
        var configValue = configValueAndType.value;
        //var configType = configValueAndType.type;

        if (expectedType == typeof(bool)) {
          value = (T)(object)parseBool(configValue);
        }
        else if (expectedType == typeof(int)) {
          value = (T)(object)parseInt(configValue);
        }
        else if (expectedType == typeof(float)) {
          value = (T)(object)parseFloat(configValue);
        }
        else if (expectedType == typeof(string)) {
          // Config supports actual string values, written in quotes, as well as
          // whole JSON objects or arrays, wrapped in {} or [].
          // Both are stored as strings.
          value = (T)(object)configValue;
        }
        else {
          if (expectedType.IsPrimitive) {
            throw new Exception("Cannot read value of type " +
              typeof(T).ToString() + ". The only supported primitive types in " +
              "Config are bool, int, and float. (Strings and anything " +
              "Unity-serializable are fine too.)");
          }

          // Any other type passed to config is converted from a string via
          // JsonUtility.
          value = (T)(object)fromJsonViaUtility<T>(configValue);
        }

        if (verbose) {
          var message = "Loaded " + field + " as " + value + " from Config.";
          if (logFunc != null) { logFunc(message); } else { Debug.Log(message); }
        }

        return true;
      }
      else {

        if (verbose) {
          var message = "Did not find " + field + " in Config.";
          if (logFunc != null) { logFunc(message); } else { Debug.Log(message); }
        }

        return false;
      }
    }

    public static T Read<T>(string field) {
      T output = default(T);
      if (!TryRead<T>(field, ref output)) {
        throw new System.Collections.Generic.KeyNotFoundException();
      }
      else {
        return output;
      }
    }

    public static void Write<T>(string field, T value) {
      readConfigFile();
      
      if (!s_config.ContainsKey(field)) {
        s_configFields.Add(field);
      }

      if (value is bool) {
        s_config[field] = new ValueAndType(
          value.ToString().ToLower(), typeof(bool));
      }
      else if (value is int) {
        s_config[field] = new ValueAndType(value.ToString(), typeof(int));
      }
      else if (value is float) {
        // Round-trip write for floats.
        s_config[field] = new ValueAndType(
          ((float)(object)value).ToString("R"), typeof(int));
      }
      else if (value is string) {
        s_config[field] = new ValueAndType(value.ToString(), typeof(string));
      }
      else {
        if (typeof(T).IsPrimitive) {
          throw new Exception("Cannot write value of type " +
            typeof(T).ToString() + ". The only supported primitive types in " +
            "Config are bool, int, and float. (Strings and anything " +
            "Unity-serializable are fine too.)");
        }
        
        s_config[field] = new ValueAndType(toJsonViaUtility(value),
          typeof(string));
      }

      writeConfigFile();
    }

    private static void readConfigFile() {
      if (!File.Exists(configFilePath)) {
        File.WriteAllText(configFilePath, "{}\n");
      }

      var configFileJson = File.ReadAllText(configFilePath);
      parseOverwrite(configFileJson, s_config, s_configFields);
    }

    private static void writeConfigFile() {
      var configFileJson = toJson(s_config, s_configFields);
      File.WriteAllText(configFilePath, configFileJson);
    }

    private struct ValueAndType {
      public string value;
      public Type type;
      public ValueAndType(string value, Type type) {
        this.value = value;
        this.type = type;
      }
    }
    private static void parseOverwrite(string json,
                                       Dictionary<string, ValueAndType> dict,
                                       List<string> fields) {
      string remaining = json.Trim();
      dict.Clear();
      fields.Clear();

      bool safeToBreak = false;
      while (!string.IsNullOrEmpty(remaining)) {
        var fieldStart = remaining.IndexOf('"');
        if (fieldStart == -1) {
          // Check if the JSON is simply empty.
          var jsonIsEmpty = false;
          var trimmedRemaining = remaining.Trim();
          if (trimmedRemaining[0] == '{') {
            trimmedRemaining = trimmedRemaining.Substring(1).TrimStart();
            if (trimmedRemaining[0] == '}') {
              jsonIsEmpty = true;
            }
          }

          if (jsonIsEmpty) {
            safeToBreak = true;
          }
          else {
            Debug.LogError("Error parsing config.json: Couldn't find next field.");
          }
          break;
        }
        var fieldEnd = remaining.IndexOf('"', fieldStart + 1);
        var fieldName = remaining.Substring(fieldStart + 1,
          fieldEnd - fieldStart - 1);
        if (string.IsNullOrEmpty(fieldName)) {
          Debug.LogError("Error parsing config.json: Empty field name.");
          break;
        }

        remaining = remaining.Substring(fieldEnd + 1);
        remaining.TrimStart();
        if (string.IsNullOrEmpty(remaining)) {
          Debug.LogError("Error parsing config.json: Json ended suddenly.");
          break;
        }
        if (remaining[0] != ':') {
          Debug.LogError("Error parsing config.json: Missing colon after field.");
          break;
        }
        remaining = remaining.Substring(1);

        ValueAndType valueAndType;
        if (!tryConsumeValue(remaining, out valueAndType, out remaining)) {
          Debug.LogError("Error parsing config.json: Couldn't consume value.");
          break;
        }
        if (string.IsNullOrEmpty(remaining)) {
          Debug.LogError("Error parsing config.json: Json ended suddenly.");
          break;
        }
        if (remaining[0] == ',') {
          remaining = remaining.Substring(1);
        }

        // Now we should have a valid field name, value, and type.
        if (!dict.ContainsKey(fieldName)) {
          fields.Add(fieldName);
        }
        dict[fieldName] = valueAndType;
        
        // Check for newlines for intentional spacing before trimming them out.
        bool hasExtraNewline = countNewlinesBeforeNonWhitespace(remaining) > 1;
        if (hasExtraNewline) {
          // Null is a sentinel value for whitespace in the list of fields.
          fields.Add(null);
        }

        remaining = remaining.TrimStart();
        if (string.IsNullOrEmpty(remaining)) {
          Debug.LogError("Error parsing config.json: Json ended suddenly.");
          break;
        }
        if (remaining[0] == '}') {
          safeToBreak = true;
          break;
        }
      }
      if (!safeToBreak) {
        Debug.LogError("Halted suddenly while parsing config.json. The remaining " + 
          "text was: " + remaining);
      }
    }

    private static bool tryConsumeValue(string input,
                                        out ValueAndType valueAndType,
                                        out string remainingInput) {
      valueAndType = default(ValueAndType);

      input = input.TrimStart();
      if (input[0] == '"') {
        // This is a literal string value.
        var quoteEnd = input.IndexOf('"', 1);
        if (quoteEnd == -1) {
          remainingInput = input;
          return false;
        }

        valueAndType.value = input.Substring(1, quoteEnd - 1);
        valueAndType.type = typeof(string);
        remainingInput = input.Substring(quoteEnd + 1);      
      }
      else if (input[0] == '{') {
        // This is some more JSON. We don't parse deeper JSON, but we do return
        // the whole sub-JSON as a string.
        var jsonEnd = findBalancedClose(input, '{', '}');
        
        valueAndType.value = input.Substring(0, jsonEnd + 1);
        valueAndType.type = typeof(string);
        remainingInput = input.Substring(jsonEnd + 1);
      }
      else if (input[0] == '[') {
        // As above.
        var jsonEnd = findBalancedClose(input, '[', ']');
        
        valueAndType.value = input.Substring(0, jsonEnd + 1);
        valueAndType.type = typeof(string);
        remainingInput = input.Substring(jsonEnd + 1);  
      }
      else {
        // Three possibilities left: Bool, int, and string. Check here for bool.
        if (input.Length >= 4 &&
            input.Substring(0, 4).ToLower().Equals("true")) {
          valueAndType.value = "true";
          valueAndType.type = typeof(bool);
          remainingInput = input.Substring(5);
        }
        else if (input.Length >= 5 &&
                 input.Substring(0, 5).ToLower().Equals("false")) {
          valueAndType.value = "false";
          valueAndType.type = typeof(bool);
          remainingInput = input.Substring(6);
        }
        else {
          // Now only float and int are left. Parse to comma or whitespace.
          var comma = input.IndexOf(',');
          var whitespace = findWhitespace(input);
          var earliestCommaOrWhitespace = pickLowestNonNegative(comma,
            whitespace);

          if (earliestCommaOrWhitespace == -1) {
            remainingInput = input;
            return false;
          }

          valueAndType.value = input.Substring(0, earliestCommaOrWhitespace);
          valueAndType.type = valueAndType.value.Contains(".") ? typeof(float)
                              : typeof(int);
          remainingInput = input.Substring(earliestCommaOrWhitespace + 1);
        }
      }

      return true;
    }

    private static int findBalancedClose(string input, char open, char close) {
      int height = 0;
      for (int i = 0; i < input.Length; i++) {
        var token = input[i];
        if (token == open) {
          height += 1;
        }
        else if (token == close) {
          height -= 1;
        }

        if (height < 0) {
          return -1;
        }
        if (height == 0) {
          return i;
        }
      }
      return -1;
    }

    private static int findWhitespace(string input) {
      for (int i = 0; i < input.Length; i++) {
        if (char.IsWhiteSpace(input[i])) {
          return i;
        }
      }
      return -1;
    }

    private static int pickLowestNonNegative(int a, int b) {
      if (a < 0 && b < 0) {
        return -1;
      }
      else if (a < 0) {
        return b;
      }
      else if (b < 0) {
        return a;
      }
      return Math.Min(a, b);
    }

    private static int countNewlinesBeforeNonWhitespace(string input) {
      int count = 0;
      for (int i = 0; i < input.Length; i++) {
        if (!char.IsWhiteSpace(input[i])) {
          return count;
        }
        else if (input[i] == '\n') {
          count += 1;
        }
      }
      return 0;
    }

    private static StringBuilder s_builder {
      get {
        if (s_backingBuilder == null) {
          s_backingBuilder = new StringBuilder();
        }
        return s_backingBuilder;
      }
    }
    [ThreadStatic]
    private static StringBuilder s_backingBuilder;

    private static string toJson(Dictionary<string, ValueAndType> dict,
                                 List<string> fields) {
      var json = s_builder;
      json.Clear();
      json.Append("{");
      json.Append("\n");
      var tab = TAB;

      int pairCount = 0;
      foreach (var field in fields) {
        json.Append(tab);

        if (string.IsNullOrEmpty(field)) {
          // Invalid in field list indicates a line break.
          json.Append("\n");
        }
        else {
          ValueAndType valueAndType;
          if (dict.TryGetValue(field, out valueAndType)) {
            var value = valueAndType.value;
            var type = valueAndType.type;

            json.Append("\"");
            json.Append(field);
            json.Append("\": ");

            if (type == typeof(bool)) {
              json.Append(value);
            }
            else if (type == typeof(int)) {
              json.Append(value);
            }
            else if (type == typeof(float)) {
              json.Append(value);
            }
            else { // type == typeof(string)
              // If the value is itself json, correct indentation and append.
              var trimmedValue = value.TrimStart();
              if (trimmedValue.StartsWith("{") ||
                  trimmedValue.StartsWith("[")) {
                bool onFirstLine = true;
                var lineEnumerator = new LineEnumerator(value);
                while (lineEnumerator.MoveNext()) {
                  var line = lineEnumerator.Current;
                  if (onFirstLine) {
                    json.Append(line.Trim());
                    onFirstLine = false;
                  }
                  else {
                    json.Append(line);
                  }
                  
                  if (!lineEnumerator.OnLastLine()) {
                    json.Append("\n");
                  }
                }
              }
              else {
                // Not json, so it must be an actual string; append that
                // instead, with quotes.
                json.Append("\"" + value + "\"");
              }
            }

            if (pairCount != fields.Count - 1) {
              json.Append(",");
            }
            json.Append("\n");
          }
          else {
            Debug.LogError("State mismatch. The field " + field + " in "
              + "the fields list had no corresponding value in the "
              + "config dictionary.");
          }
        }

        pairCount++;
      }

      json.Append("}");
      json.Append("\n");
      return json.ToString();
    }

    public struct LineEnumerator {
      private string remainingString;
      private string line;
      private bool gotLastLine;
      public LineEnumerator(string str) {
        remainingString = str;
        line = "";
        gotLastLine = false;
      }
      public LineEnumerator GetEnumerator() {
        return this;
      }
      public bool MoveNext() {
        if (gotLastLine) return false;

        int nextLineIdx = remainingString.IndexOf("\n");
        if (nextLineIdx == -1) {
          line = remainingString;
          gotLastLine = true;
        }
        else {
          line = remainingString.Substring(0, nextLineIdx);

          if (remainingString.Length == nextLineIdx + 1) {
            // There's just an empty newline at the end; this is the last line.
            gotLastLine = true;
            remainingString = "";
          }
          else {
            remainingString = remainingString.Substring(nextLineIdx + 1);
          }
        }
        return true;
      }
      public string Current { get { return line; } }
      public bool OnLastLine() {
        return gotLastLine;
      }
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

    /// <summary>
    /// Converts any Unity-serializable object to JSON via the JsonUtility.
    /// </summary>
    private static string toJsonViaUtility(object obj) {
      var objectJson = JsonUtility.ToJson(obj, prettyPrint: true);
      return objectJson.Trim().Replace("    ", TAB);
    }

    /// <summary>
    /// Converts JsonUtility JSON back to the type from which it came.
    /// </summary>
    private static T fromJsonViaUtility<T>(string json) {
      return JsonUtility.FromJson<T>(json);
    }

    public static void Test() {
      #pragma warning disable 0219 // Ignore "not used" warnings.
      Debug.Log("Testing fail on no element found.");
      try {
        var badValueMan = Config.Read<float>("basdfwerfwe");
      }
      catch (KeyNotFoundException e) {
        Debug.Log("Yep: " + e.ToString());
      }

      Debug.Log("Testing writing some stuff.");
      Config.Write("foo", 0.48f);
      Config.Write("bar", 389384);
      Config.Write("baz", true);
      Config.Write("internalJson", "   { \"herp\" : \"stuff\" }");
      Config.Write("anActualString", "this is not json, just a string");
      Config.Write("subparJsonWithNewLines",
        "\t{ \n  \"herp\": false, \n \"bleen\": 3.89 }");

      var foo = Config.Read<float>("foo");
      var bar = Config.Read<float>("bar");
      var baz = Config.Read<bool>("baz");
      var internalJson = Config.Read<string>("internalJson");
      var actualString = Config.Read<string>("anActualString");
      var subparJsonWithNewLines = Config.Read<string>("subparJsonWithNewLines");
      #pragma warning restore 0219 // Restore "not used" warnings.
    }

    public class FieldNotFoundException : Exception {
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

    public class FailedToParseException : Exception {
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

    public class TypeMismatchException : Exception {
      public Type expectedType;
      public Type gotType;
      public TypeMismatchException(Type expectedType, Type gotType) {
        this.expectedType = expectedType;
        this.gotType = gotType;
      }

      public override string ToString() {
        return "Type mismatch. Expected " + expectedType + " but got " +
          gotType + ".\n" +
          base.ToString();;
      }
    }

  }

}
