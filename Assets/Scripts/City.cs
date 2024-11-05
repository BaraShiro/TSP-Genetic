using UnityEngine;

public class City : MonoBehaviour
{
   public Vector2 Coordinates { get; set; }

   public float Distance(City other)
   {
      return Vector2.Distance(Coordinates, other.Coordinates);
   }

   public override string ToString()
   {
      return $"City {Coordinates}";
   }
}
