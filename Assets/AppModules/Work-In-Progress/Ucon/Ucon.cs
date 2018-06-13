using Leap.Unity.Query;
using System.Collections.Generic;

namespace Leap.Unity.UserContext {

  public enum UserContextType {
    Local  = 0,
    Remote = 1,
    Bot    = 2
  }

  /// <summary>
  /// Contextual data-channel API. Get a context, get a channel path, and read/write
  /// typed data at the generic channel as desired.
  /// 
  /// Intended originally for sharing simple state amongst classes that are "attached" to
  /// a user context, such as "the local player", or "the tutorial bot", for simple
  /// data situations such as "the color of the user's brush", e.g., a Color channel
  /// at "brush/color".
  /// </summary>
  public static class Ucon {

    // Example API usage:
    //
    // Ucon.Get(UserContextType.Remote).At("brush/color").Get<Color>()
    // Ucon.Get(UserContextType.Local ).At("brush/color").GetIsEmpty<Color>()
    // Ucon.Get(UserContextType.Bot   ).At("brush/color").TryGet(out color)
    // Ucon.Get(UserContextType.Local ).At("brush/color").GetEach() // synonym for Query()
    // Ucon.Get(UserContextType.Local ).At("brush/color").Query()   // QueryWrapper! :D
    // Ucon.Get(UserContextType.Local ).At("brush/color").Set(swatchColor)
    // Ucon.Get(UserContextType.Local ).At("brush/color").Set(swatchColors)
    // Ucon.Get(UserContextType.Local ).At("brush/color").Append(swatchColor);
    // Ucon.Get(UserContextType.Local ).At("brush/color").Remove(swatchColor);
    // Ucon.Get(UserContextType.Local ).At("brush/color").Clear<Color>()
    //
    // var ucontext = Ucon.Get(UserContextType.Local);
    // var channel = ucontext.At("brush/color");
    // var typedChannel = channel.TypedFor<Color>(); // if you want.
    //     typedChannel.Get();
    //     typedChannel.GetIsEmpty();
    //     typedChannel.TryGet(out Color c) -> bool
    //     typedChannel.GetEach()
    //     typedChannel.Query()
    //     typedChannel.Set(Color c);
    //     typedChannel.Set(IList<Color> c);
    //     typedChannel.Clear();
    //     typedChannel.Add(Color c);
    //     typedChannel.Add(IList<Color> c);
    //     typedChannel.Remove(Color c);
    //

    #region Context
    
    /// <summary>
    /// Shorthand for "Context", retrieves a context for the context type.
    /// 
    /// To pass data through Ucon, use a standard context type (or a custom context) and
    /// get a channel in that context by specifying its channel path, then use the
    /// channel API to read or write data at the channel.
    /// </summary>
    public static ContextWrapper<UserContextType> C(UserContextType contextType) {
      return Context<UserContextType>(contextType);
    }

    /// <summary>
    /// Retrieves a context for the specified context type. You can also just type "C".
    /// 
    /// To pass data through Ucon, use a standard context type (or a custom context) and
    /// get a channel in that context by specifying its channel path, then use the
    /// channel API to read or write data at the channel.
    /// </summary>
    public static ContextWrapper<UserContextType> Context(UserContextType contextType) {
      return Context<UserContextType>(contextType);
    }

    /// <summary>
    /// Shorthand for "Context", retrieves a context for the context type.
    /// 
    /// To pass data through Ucon, pass any object (e.g. a custom enum) as a context and
    /// get a channel in that context by specifying its channel path, then use the
    /// channel API to read or write data at the channel.
    /// </summary>
    public static ContextWrapper<T> C<T>(T contextObj) {
      return Context<T>(contextObj);
    }

    /// <summary>
    /// Retrieves a context for the specified context type. You can also just type "C".
    /// 
    /// To pass data through Ucon, pass any object (e.g. a custom enum) as a context and
    /// get a channel in that context by specifying its channel path, then use the
    /// channel API to read or write data at the channel.
    /// </summary>
    public static ContextWrapper<T> Context<T>(T contextObj) {
      return new ContextWrapper<T>() {
        contextObj = contextObj
      };
    }

    public struct ContextWrapper<T> {
      public T contextObj;

      public Channel At(string channelPath) {
        return new Channel(contextObj, channelPath);
      }
    }

    #endregion

    #region Channel

    public struct Channel {
      public readonly Hash channelHash;

      public Channel(object contextObj, string channelPath) {
        channelHash = new Hash() { contextObj, channelPath };
      }

      #region Internal API

      /// <summary>
      /// Returns a typed channel for the argument type. The generic API on Channels is
      /// just sugar for calls on the corresponding TypedChannel.
      /// </summary>
      public TypedChannel<T> TypedFor<T>() {
        return new TypedChannel<T>(this);
      }

      #endregion

      #region Channel API

      /// <summary>
      /// Gets the first (and perhaps only) value of the argument type that is associated
      /// with this channel. If there was no data set of the argument type at this
      /// channel, the default value for the type is returned.
      /// 
      /// Use TryGet() or GetIsEmpty() to know whether or not there was data set at the
      /// channel.
      /// 
      /// Channels that receive Append(value) or Set(IList) calls may store a sequence of
      /// values of a given type. To query the whole sequence, use GetEach() or Query().
      /// </summary>
      public T Get<T>() {
        return TypedFor<T>().Get();
      }

      /// <summary>
      /// Returns whether or not any data has been set of the argument type at this
      /// channel.
      /// 
      /// Channels are specific to their type and context as well as the channel path.
      /// </summary>
      public bool GetIsEmpty<T>() {
        return TypedFor<T>().GetIsEmpty();
      }

      /// <summary>
      /// Returns whether or not there is at least value of the specified type at this
      /// channel, and outputs the first (and perhaps only) value into the out argument
      /// if there was, otherwise outputs the default value for the type argument.
      /// 
      /// Channels that receive Append(value) or Set(IList) calls may store a sequence of
      /// values of a given type. To query the whole sequence, use GetEach() or Query().
      /// </summary>
      public bool TryGet<T>(out T value) {
        return TypedFor<T>().TryGet(out value);
      }

      ///// <summary>
      ///// Synonym for Query().
      ///// 
      ///// Returns a Queryable representation of the data at the channel for the argument
      ///// type. If the channel is empty or has been Clear()'d, the query will be empty;
      ///// if only Set(value) calls have been performed on this channel, the channel will
      ///// contain a single element. If Append(value) or Set(IList) calls have been
      ///// performed on the channel for the argument type, the channel may contain more
      ///// than one element of the argument type.
      ///// </summary>
      //public QueryWrapper<T, QueryConversionExtensions.ListQueryOp<T>> GetEach<T>() {
      //  return TypedFor<T>().GetEach();
      //}

      ///// <summary>
      ///// Returns a Queryable representation of the data at the channel for the argument
      ///// type. If the channel is empty or has been Clear()'d, the query will be empty;
      ///// if only Set(value) calls have been performed on this channel, the channel will
      ///// contain a single element. If Append(value) or Set(IList) calls have been
      ///// performed on the channel for the argument type, the channel may contain more
      ///// than one element of the argument type.
      ///// </summary>
      //public QueryWrapper<T, QueryConversionExtensions.ListQueryOp<T>> Query<T>() {
      //  return TypedFor<T>().Query();
      //}

      /// <summary>
      /// Clears the channel of the argument type of any value or values that may exist
      /// of that type already and sets a single value at the channel of the argument
      /// type.
      /// </summary>
      public void Set<T>(T value) {
        TypedFor<T>().Set(value);
      }

      /// <summary>
      /// Clears the channel of the argument type of any value or values that may exist
      /// of that type already and shallow-copies the values from the argument value list
      /// into the channel.
      /// </summary>
      public void Set<T>(IList<T> values) {
        TypedFor<T>().Set(values);
      }

      /// <summary>
      /// Clears the channel of the argument type of any value or values that may exist
      /// of that type already and shallow-copies the values from the argument value
      /// sequence into the channel.
      /// </summary>
      public void Set<T>(IIndexable<T> values) {
        TypedFor<T>().Set(values);
      }

      /// <summary>
      /// Clears the channel of the argument type of any value or values that may exist
      /// of that type already and shallow-copies the values from the argument value
      /// collection into the channel.
      /// </summary>
      public void Set<T>(ICollection<T> values) {
        TypedFor<T>().Set(values);
      }

      /// <summary>
      /// Adds the value to the channel of that value's type.
      /// Values are appended to the end of the channel if the channel already has one
      /// or more values. To clear a channel, use Set() functions or call Clear().
      /// 
      /// Two channels with the same context and channel path are further distinguished
      /// by their channel types, meaning the two channels have independent storage.
      /// </summary>
      public void Add<T>(T value) {
        TypedFor<T>().Add(value);
      }

      /// <summary>
      /// Adds the values in the argument list to the channel of that values' type.
      /// Values are appended to the end of the channel if the channel already has one
      /// or more values. To clear a channel, use Set() functions or call Clear().
      /// 
      /// Two channels with the same context and channel path are further distinguished
      /// by their channel types, meaning the two channels will have independent storage
      /// if their types are different.
      /// </summary>
      public void Add<T>(IList<T> values) {
        TypedFor<T>().Add(values);
      }

      /// <summary>
      /// Adds the values in the argument indexable to the channel of that values' type.
      /// Values are appended to the end of the channel if the channel already has one
      /// or more values. To clear a channel, use Set() functions or call Clear().
      /// 
      /// Two channels with the same context and channel path are further distinguished
      /// by their channel types, meaning the two channels will have independent storage
      /// if their types are different.
      /// </summary>
      public void Add<T>(IIndexable<T> values) {
        TypedFor<T>().Add(values);
      }

      /// <summary>
      /// Adds the values in the argument collection to the channel of that values' type.
      /// Values are appended to the end of the channel if the channel already has one
      /// or more values. To clear a channel, use Set() functions or call Clear().
      /// 
      /// Two channels with the same context and channel path are further distinguished
      /// by their channel types, meaning the two channels will have independent storage
      /// if their types are different.
      /// </summary>
      public void Add<T>(ICollection<T> values) {
        TypedFor<T>().Add(values);
      }

      /// <summary>
      /// Clears the channel of any data of the specified type.
      /// </summary>
      public void Clear<T>() {
        TypedFor<T>().Clear();
      }

      /// <summary>
      /// Removes the first match for value in the channel.
      /// </summary>
      public void Remove<T>(T value) {
        TypedFor<T>().Remove(value);
      }

      /// <summary>
      /// Returns the underlying buffer object.
      /// </summary>
      public List<T> GetBuffer<T>() {
        return TypedFor<T>().GetBuffer();
      }

      #endregion

    }

    #endregion

    #region TypedChannel (Memory read/write implementation)

    public struct TypedChannel<T> {
      public readonly Hash channelHash;

      public TypedChannel(Channel untypedChannel) {
        channelHash = untypedChannel.channelHash;
      }

      #region Reading

      public T Get() {
        List<T> outTs;
        if (Board<T>.memory.TryGetValue(channelHash, out outTs)) {
          return outTs.Count == 0 ? default(T) : outTs[0];
        }
        else {
          outTs = new List<T>();
          Board<T>.memory[channelHash] = outTs;
          return default(T);
        }
      }

      public bool GetIsEmpty() {
        List<T> values;
        return !(Board<T>.memory.TryGetValue(channelHash, out values))
               || values.Count == 0;
      }

      public bool TryGet(out T value) {
        value = default(T);
        List<T> values;
        return (Board<T>.memory.TryGetValue(channelHash, out values))
               && (TryGetFirst(values, out value));
      }
      private bool TryGetFirst(List<T> values, out T first) {
        first = default(T);
        if (values.Count == 0) return false;
        first = values[0];
        return true;
      }

      //public QueryWrapper<T, QueryConversionExtensions.ListQueryOp<T>> GetEach() {
      //  return Query();
      //}

      //public QueryWrapper<T, QueryConversionExtensions.ListQueryOp<T>> Query() {
      //  List<T> outTs;
      //  if (!Board<T>.memory.TryGetValue(channelHash, out outTs)) {
      //    outTs = new List<T>();
      //    Board<T>.memory[channelHash] = outTs;
      //  }
      //  return new QueryWrapper<T, QueryConversionExtensions.ListQueryOp<T>>(
      //           new QueryConversionExtensions.ListQueryOp<T>(outTs));
      //}

      #endregion

      #region Writing

      private List<T> getListAndEnsureExists() {
        List<T> outTs;
        if (!Board<T>.memory.TryGetValue(channelHash, out outTs)) {
          outTs = new List<T>();
          Board<T>.memory[channelHash] = outTs;
        }
        return outTs;
      }

      public void Set(T value) {
        var ts = getListAndEnsureExists();
        ts.Clear();

        ts.Add(value);
      }

      public void Set(IList<T> values) {
        var ts = getListAndEnsureExists();
        ts.Clear();

        ts.AddRange(values);
      }

      public void Set(IIndexable<T> values) {
        var ts = getListAndEnsureExists();
        ts.Clear();

        for (int i = 0; i < values.Count; i++) {
          ts.Add(values[i]);
        }
      }

      public void Set(ICollection<T> values) {
        var ts = getListAndEnsureExists();
        ts.Clear();

        ts.AddRange(values);
      }

      public void Add(T value) {
        var ts = getListAndEnsureExists();

        ts.Add(value);
      }

      public void Add(IList<T> values) {
        var ts = getListAndEnsureExists();

        ts.AddRange(values);
      }

      public void Add(IIndexable<T> values) {
        var ts = getListAndEnsureExists();

        for (int i = 0; i < values.Count; i++) {
          ts.Add(values[i]);
        }
      }

      public void Add(ICollection<T> values) {
        var ts = getListAndEnsureExists();

        ts.AddRange(values);
      }

      public void Clear() {
        var ts = getListAndEnsureExists();

        ts.Clear();
      }

      public void Remove(T value) {
        var ts = getListAndEnsureExists();

        ts.Remove(value);
      }

      #endregion

      public List<T> GetBuffer() {
        List<T> outTs;
        if (!Board<T>.memory.TryGetValue(channelHash, out outTs)) {
          outTs = new List<T>();
          Board<T>.memory[channelHash] = outTs;
        }
        return outTs;
      }

    }

    #endregion

    private static class Board<T> {
      public static Dictionary<Hash, List<T>> memory = new Dictionary<Hash, List<T>>();
    }
    
  }

}
