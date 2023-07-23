using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugGizmos : MonoBehaviour
{
    public static void DrawSpehere(Vector3 position, float radius, Color color, float duration)
    {
        const int DETAIL = 16;

        float angle = 0.0f;
        float angleStep = (Mathf.PI * 2.0f) / DETAIL;
        for(int i =0;i < DETAIL; i++)
        {
            float sinValue = Mathf.Sin(angle) * radius;
            float cosValue = Mathf.Cos(angle) * radius;

            float sinValue1 = Mathf.Sin(angle + angleStep) * radius;
            float cosValue1 = Mathf.Cos(angle + angleStep) * radius;

            Vector3 position0 = new Vector3(position.x + sinValue, position.y, position.z + cosValue);
            Vector3 position1 = new Vector3(position.x + sinValue1, position.y, position.z + cosValue1);
            Debug.DrawLine(position0, position1, color, duration);

            position0 = new Vector3(position.x, position.y + sinValue, position.z + cosValue);
            position1 = new Vector3(position.x, position.y + sinValue1, position.z + cosValue1);
            Debug.DrawLine(position0, position1, color, duration);

            position0 = new Vector3(position.x + cosValue, position.y + sinValue, position.z);
            position1 = new Vector3(position.x + cosValue1, position.y + sinValue1, position.z);
            Debug.DrawLine(position0, position1, color, duration);

            angle += angleStep;
        }
    }
    public static void DrawCube(Vector3 center, float width, float height, float depth, Color color, float duration = 0.0f)
    {
        Vector3 min = new Vector3(-width / 2.0f, -height / 2.0f, -depth / 2.0f);
        Vector3 max = new Vector3(width / 2.0f, height / 2.0f, depth / 2.0f);

        Debug.DrawLine(center + new Vector3(min.x, min.y, min.z),  center + new Vector3(max.x, min.y, min.z), color, duration);
        Debug.DrawLine(center + new Vector3(min.x, min.y, min.z),  center + new Vector3(min.x, max.y, min.z), color, duration);
        Debug.DrawLine(center + new Vector3(min.x, min.y, min.z),  center + new Vector3(min.x, min.y, max.z), color, duration);

        Debug.DrawLine(center + new Vector3(max.x, max.y, max.z), center + new Vector3(min.x, max.y, max.z), color, duration);
        Debug.DrawLine(center + new Vector3(max.x, max.y, max.z), center + new Vector3(max.x, min.y, max.z), color, duration);
        Debug.DrawLine(center + new Vector3(max.x, max.y, max.z), center + new Vector3(max.x, max.y, min.z), color, duration);

        Debug.DrawLine(center + new Vector3(min.x, max.y, min.z), center + new Vector3(max.x, max.y, min.z), color, duration);
        Debug.DrawLine(center + new Vector3(max.x, max.y, min.z), center + new Vector3(max.x, min.y, min.z), color, duration);
        Debug.DrawLine(center + new Vector3(min.x, max.y, max.z), center + new Vector3(min.x, min.y, max.z), color, duration);

        Debug.DrawLine(center + new Vector3(min.x, max.y, min.z), center + new Vector3(min.x, max.y, max.z), color, duration);
        Debug.DrawLine(center + new Vector3(max.x, min.y, min.z), center + new Vector3(max.x, min.y, max.z), color, duration);
        Debug.DrawLine(center + new Vector3(min.x, min.y, max.z), center + new Vector3(max.x, min.y, max.z), color, duration);
    }
}
