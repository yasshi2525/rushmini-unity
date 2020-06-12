using System.Collections.Generic;

public interface ILineTask
{
  RailLine Parent { get; }
  ILineTask Prev { get; set; }
  ILineTask Next { get; set; }
  List<Train> Trains { get; }
  RailNode Departure();
  RailNode Destination();
  float SignedAngle(RailEdge edge);
  float Length { get; }
  /**
   * 指定された線路と隣接しているか判定します
   */
  bool IsNeighbor(RailEdge edge);
  void InsertEdge(RailEdge edge);
  void InsertPlatform(Platform platform);
  void Remove();
  void Shrink(ILineTask to);
}