using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Eppy;

public class Geometry {
    public static Vector2[] PointsOfRect (Rect rc) {
        return new Vector2[] { new Vector2(rc.x, rc.y),
                               new Vector2(rc.x, rc.y + rc.height),
                               new Vector2(rc.x + rc.width, rc.y + rc.height ),
                               new Vector2(rc.x + rc.width, rc.y) };
    }

    public static Vector2[] AxesOfRotatedRect (Quaternion rotation) {
        return new Vector2[] { rotation * Vector2.up, rotation * Vector2.right };
    }

    public static Vector2[] CrossAxesOfRotatedRect (Quaternion rotation) {
        return new Vector2[] { rotation * new Vector2(1, 1).normalized, rotation * new Vector2(1, -1).normalized };
    }

    public static Tuple<float, float> ProjectRect (Rect rc, Vector2 translation, Quaternion rotation, Vector2 axis) {
        var ps = PointsOfRect(rc).Select(p => Vector2.Dot((Vector3)translation + (rotation * p), axis));
        return new Tuple<float, float>(ps.Min(), ps.Max());
    }

    // never used?
    public static IEnumerable<Tuple<Vector2, Vector2> > EdgesOfRect (Rect rc) {
        var ps = PointsOfRect(rc);
        for (int i = 0; i < ps.Length; ++i) {
            for (int j = i + 1; j < ps.Length; ++j) {
                yield return new Tuple<Vector2, Vector2> (ps[i], ps[j]);
            }
        }
    }
}
