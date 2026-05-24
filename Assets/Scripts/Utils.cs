using UnityEngine;

static class Utils
{
    static public void DrawDebugCircle(Vector3 center, float radius, Color color, int segments = 32)
    {
        float angleStep = 2 * Mathf.PI / segments;
        for (int i = 0; i < segments; i++)
        {
            float a1 = angleStep * i;
            float a2 = angleStep * (i + 1);
            Vector3 p1 = center + new Vector3(Mathf.Cos(a1), 0f, Mathf.Sin(a1)) * radius;
            Vector3 p2 = center + new Vector3(Mathf.Cos(a2), 0f, Mathf.Sin(a2)) * radius;
            Debug.DrawLine(p1, p2, color);
        }
    }
}