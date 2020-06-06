﻿using UnityEngine;

public class Company : MonoBehaviour
{
  public ModelListener listener;
  public ModelStorage storage;
  public bool isTemplate = true;
  /**
   * 住民がこの会社を行き先として選ぶ度合い 自身/全会社の合計 の割合で行き先が選ばれる
   */
  public int attractiveness;

  private void Start()
  {
    if (isTemplate)
    {
      listener.Find<Company>(EventType.CREATED).AddListener(c => storage.Find<Company>().Add(c));
      listener.Find<Company>(EventType.DELETED).AddListener(c => storage.Find<Company>().Remove(c));
    }
  }

  private void Update()
  {

  }
}
