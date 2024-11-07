using UnityEngine;

/// <summary>
/// A representation of a city in a TSP tour.
/// </summary>
public class City : MonoBehaviour
{
   /// <summary>
   /// The coordinates of the city, in world space.
   /// </summary>
   public Vector2 Coordinates { get; set; }

   /// <summary>
   /// Measure the distance between two cities.
   /// </summary>
   /// <param name="other">The other city.</param>
   /// <returns>The distance between this city and the other city.</returns>
   public float Distance(City other)
   {
      return Vector2.Distance(Coordinates, other.Coordinates);
   }

   /// <summary>
   /// Returns a string with the citys coordinates prefixed by the word "City".
   /// </summary>
   /// <returns>A string representation of the city.</returns>
   public override string ToString()
   {
      return $"City {Coordinates}";
   }
}
