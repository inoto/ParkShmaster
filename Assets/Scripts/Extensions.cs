using UnityEngine;

public static class Extensions
{
    public static Vector3 Y(this Vector3 vec, float y)
    {
        vec.y = y;
        return vec;
    }
}
