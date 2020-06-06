using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum EventType
{
  CREATED,
  MODIFIED,
  DELETED
}

class ModelEvent<T> : UnityEvent<T>
{
};

public class ModelListener : MonoBehaviour
{
  private IDictionary<EventType, IDictionary<Type, object>> storage;

  private void Awake()
  {
    storage = new Dictionary<EventType, IDictionary<Type, object>>();
  }

  private IDictionary<Type, object> FindHandlers(EventType ev)
  {
    if (!storage.ContainsKey(ev))
    {
      storage.Add(ev, new Dictionary<Type, object>());
    }
    return storage[ev];
  }

  public UnityEvent<T> Find<T>(EventType ev)
  {
    var key = typeof(T);
    var handlers = FindHandlers(ev);
    if (!handlers.ContainsKey(key))
    {
      handlers.Add(key, new ModelEvent<T>());
    }
    return handlers[key] as UnityEvent<T>;
  }

  public void Fire<T>(EventType ev, T obj)
  {
    Find<T>(ev).Invoke(obj);
  }
}