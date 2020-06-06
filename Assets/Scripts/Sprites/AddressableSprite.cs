using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AddressableSprite : MonoBehaviour
{
  public AssetReference address;
  private Sprite sprite;

  private void Awake()
  {
    var sr = gameObject.GetComponent<SpriteRenderer>();
    if (sr.sprite == null)
    {
      Addressables.LoadAssetAsync<Sprite>(address).Completed += obj =>
      {
        switch (obj.Status)
        {
          case AsyncOperationStatus.Succeeded:
            sr.sprite = obj.Result;
            break;
          case AsyncOperationStatus.Failed:
            Debug.LogWarning("failed to load key " + address);
            break;
        }
      };
    }
  }
}