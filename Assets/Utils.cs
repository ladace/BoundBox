using UnityEngine;
using System.Collections;
using Eppy;

public delegate void Proc();
public delegate IEnumerator CoProc();

class Utils {
    public static float Sqr (float x) { return x * x; }

    public static Vector3 RestrainRect (Vector3 pos, Rect rc) {
        Vector3 ret = pos;
        ret.x = Mathf.Min(rc.xMax, Mathf.Max(pos.x, rc.xMin));
        ret.y = Mathf.Min(rc.yMax, Mathf.Max(pos.y, rc.yMin));
        return ret;
    }

    public static float DistanceByPixel (PhysicsEntity e1, PhysicsEntity e2) {
        float x1 = e1.transform.position.x,
            y1 = -e1.transform.position.y,
            x2 = e2.transform.position.x,
            y2 = -e2.transform.position.y;
        var xDist = Mathf.Max(x1 - x2 + (e1.shape.xMin - e2.shape.xMax) / 100, x2 - x1 + (e2.shape.xMin - e1.shape.xMax) / 100);
        if (xDist < 0) xDist = 0;
        var yDist = Mathf.Max(y1 - y2 + (e1.shape.yMin - e2.shape.yMax) / 100, y2 - y1 + (e2.shape.yMin - e1.shape.yMax) / 100);
        if (yDist < 0) yDist = 0;
        return Mathf.Sqrt(xDist * xDist + yDist * yDist) * 100;
    }

    // return the directional shortest distance to move r1 out of r2
    public static float SegmentInto (Tuple<float, float> r1, Tuple<float, float> r2) {
        float a1 = r2.Item2 - r1.Item1,
              a2 = r1.Item2 - r2.Item1;
        if (a1 > 0 && a2 > 0) {
            return a1 > a2 ? -a2 : a1;
        }
        return 0;
    }

    public static void DelayCall (MonoBehaviour obj, Proc callback) {
        obj.StartCoroutine(_DelayCall(callback));
    }

    public static void WaitCall (MonoBehaviour obj, float delay, Proc callback) {
        obj.StartCoroutine(_WaitCall(delay, callback));
    }

    private static IEnumerator _DelayCall (Proc callback) {
        yield return null;
        callback();
    }

    private static IEnumerator _WaitCall (float delay, Proc callback) {
        yield return new WaitForSeconds(delay);;
        callback();
    }

    public static Color SetAlpha (Color c, float a) {
        Color color = c;
        color.a = a;
        return color;
    }

    public static Color SetRed (Color c, float r) {
        Color color = c;
        color.r = r;
        return color;
    }

    public static Color SetBlue (Color c, float b) {
        Color color = c;
        color.b = b;
        return color;
    }

    public static Color SetGreen (Color c, float g) {
        Color color = c;
        color.g = g;
        return color;
    }

    public static void DrawRectGizmos (Rect rc, Color color, Vector3 offset, Quaternion rotation) {
        Gizmos.color = color;
        Vector2? lastPoint = null;
        Vector2[] points = Geometry.PointsOfRect(rc);
        foreach (Vector2 p in points) {
            if (!lastPoint.HasValue) {
                lastPoint = p;
                continue;
            }
            Gizmos.DrawLine(offset + rotation * (Vector3)lastPoint, offset + rotation * (Vector3)p);
            lastPoint = p;
        }
        Gizmos.DrawLine(offset + rotation * (Vector3)lastPoint, offset + rotation * (Vector3)points[0]);
    }

    public static void DrawCircleGizmos (float radius, Color color, Vector3 offset) {
        Gizmos.color = color;
        for (int i = 0; i < 21; ++i) {
            float a1 = i * Mathf.PI / 10,
                  a2 = (i + 1) * Mathf.PI / 10;
            Gizmos.DrawLine(offset + new Vector3(Mathf.Cos(a1) * radius, Mathf.Sin(a1) * radius), offset + new Vector3(Mathf.Cos(a2) * radius, Mathf.Sin(a2) * radius));
        }
    }
}