using System;
using System.Collections.Generic;
using UnityEngine;

public class ModelStorage : MonoBehaviour
{
  private IDictionary<Type, object> storage;

  private void Awake()
  {
    storage = new Dictionary<Type, object>();
  }

  public List<T> Find<T>()
  {
    var key = typeof(T);
    if (!storage.ContainsKey(key))
    {
      storage.Add(key, new List<T>());
    }
    return storage[key] as List<T>;
  }
}