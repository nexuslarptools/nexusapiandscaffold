using System.Collections.Generic;

namespace NEXUSDataLayerScaffold.Entities;

public interface INullValueDictionary<T, U>
    where U : class
{
    U this[T key] { get; }
}

public class NullValueDictionary<T, U> : Dictionary<T, U>, INullValueDictionary<T, U>
    where U : class
{
    U INullValueDictionary<T, U>.this[T key]
    {
        get
        {
            U val;
            TryGetValue(key, out val);
            return val;
        }
    }
}