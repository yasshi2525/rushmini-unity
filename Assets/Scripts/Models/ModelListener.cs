using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum EventType
{
  CREATED,
  MODIFIED,
  RIDDEN,
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

  private IDictionary<Type, object> FindListeners(EventType ev)
  {
    if (!storage.ContainsKey(ev))
    {
      storage.Add(ev, new Dictionary<Type, object>());
    }
    return storage[ev];
  }

  private ModelEvent<T> FindEvent<T>(EventType ev)
  {
    var key = typeof(T);
    var listeners = FindListeners(ev);
    if (!listeners.ContainsKey(key))
    {
      listeners.Add(key, new ModelEvent<T>());
    }
    return listeners[key] as ModelEvent<T>;
  }

  public void Add<T>(EventType ev, UnityAction<T> listener)
  {
    FindEvent<T>(ev).AddListener(listener);
  }

  public void Add<T>(EventType ev, T obj, UnityAction<T> listener)
  {
    FindEvent<T>(ev).AddListener(o => { if (o.Equals(obj)) listener(o); });
  }

  public void Fire<T>(EventType ev, T obj)
  {
    FindEvent<T>(ev).Invoke(obj);
  }
}