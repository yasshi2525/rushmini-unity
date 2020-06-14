using System.Collections.Generic;
using UnityEngine;

public class CityResource : MonoBehaviour
{
  private enum Chunk
  {
    NE, NW, SE, SW
  }

  public ModelFactory factory;
  public ModelStorage storage;
  public int MaxTry = 50;
  public float Width = 8.16f * 0.8f;
  public float Height = 6.24f * 0.8f;
  /**
   * 建物は最低この距離間をおいて建設する
   */
  public float Sparse = 1.50f;
  public float Padding = 1.00f;

  private bool isInited;

  private Queue<Chunk> residences;
  private Queue<Chunk> companies;
  private List<Vector3> positions;

  private void Awake()
  {
    residences = new Queue<Chunk>();
    residences.Enqueue(Chunk.NW);
    residences.Enqueue(Chunk.SW);
    residences.Enqueue(Chunk.NE);
    companies = new Queue<Chunk>();
    companies.Enqueue(Chunk.SE);
    companies.Enqueue(Chunk.SE);
    positions = new List<Vector3>();
  }

  // Update is called once per frame
  private void Update()
  {
    if (!isInited)
    {
      Init();
    }
    isInited = true;
  }

  private Vector3 ChunkSize
  {
    get { return new Vector3(Width / 2 - Padding, Height / 2 - Padding); }
  }

  private Vector3 ChunkCenter(Chunk ch)
  {
    var size = ChunkSize;
    float dx = 0;
    float dy = 0;
    switch (ch)
    {
      case Chunk.NE:
        dx = size.x / 2;
        dy = size.y / 2;
        break;
      case Chunk.SE:
        dx = size.x / 2;
        dy = -size.y / 2;
        break;
      case Chunk.NW:
        dx = -size.x / 2;
        dy = size.y / 2;
        break;
      case Chunk.SW:
        dx = -size.x / 2;
        dy = -size.y / 2;
        break;
    }
    return new Vector3(dx, dy);
  }

  private Vector3 Shuffle(Chunk ch)
  {
    var center = ChunkCenter(ch);
    var size = ChunkSize;
    return new Vector3(
      center.x + Random.Range(-size.x / 2, size.x / 2),
      center.y + Random.Range(-size.y / 2, size.y / 2)
    );
  }

  /**
    * チャンク内のランダムな位置で、他と一定距離離れた点を返す
    */
  private Vector3 ShuffleSparse(Chunk ch)
  {
    Vector3 pos;
    int i = 0;
    do
    {
      pos = Shuffle(ch);
      i++;
    } while (positions.Exists(p => Vector3.Distance(pos, p) < Sparse) && i < MaxTry);
    return pos;
  }

  private void Init()
  {
    // 初期会社
    factory.NewCompany(1 + companies.Count, new Vector3(Width / 2 - Padding, -Height / 2 + Padding));
    // 初期住宅
    factory.NewResidence(new Vector3(-Width / 2 + Padding, Height / 2 - Padding));
    // 追加会社
    var ch = companies.Dequeue();
    factory.NewCompany(1 + companies.Count, ShuffleSparse(ch));
  }

  public void AddResidence()
  {
    var ch = residences.Dequeue();
    factory.NewResidence(ShuffleSparse(ch));
  }

  public void AddResidence(Vector3 pos)
  {
    factory.NewResidence(pos);
  }

  public void AddCompany()
  {
    var ch = companies.Dequeue();
    factory.NewCompany(1 + companies.Count, ShuffleSparse(ch));
  }
}
