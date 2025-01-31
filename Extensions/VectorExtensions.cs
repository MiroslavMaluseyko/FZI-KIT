using UnityEngine;

namespace FZI.Extensions
{
    public static class VectorExtensions
    {
        public static Vector2 RotatedByAngle(this Vector2 vec, float angleDegrees)
        {
            float angleRad = angleDegrees * Mathf.Deg2Rad;
            Vector2 newVec =
                new Vector2(
                   vec.x * Mathf.Cos(angleRad) - vec.y * Mathf.Sin(angleRad),
                   vec.x * Mathf.Sin(angleRad) + vec.y * Mathf.Cos(angleRad));
            return newVec;
        }
        
        public static Vector2 ToVectorXY(this Vector3 vec) => new Vector2(vec.x, vec.y);
        public static Vector2 ToVectorXZ(this Vector3 vec) => new Vector2(vec.x, vec.z);
    }
}