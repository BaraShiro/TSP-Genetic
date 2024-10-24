using UnityEngine;

public class City
{
   public Vector2 Coordinates { get; }

   public City()
   {
      Coordinates = new Vector2(Random.value, Random.value);
   }

   public City(Vector2 coordinates)
   {
      Coordinates = coordinates;
   }

   public float Distance(City other)
   {
      return Vector2.Distance(Coordinates, other.Coordinates);
   }

   public override string ToString()
   {
      return $"City {Coordinates}";
   }
}
