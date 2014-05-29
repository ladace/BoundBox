using UnityEngine;
using System.Collections.Generic;
using Eppy;


public class PhysicsWorld : MonoBehaviour {
	static public PhysicsWorld ins;
	public List<PhysicsEntity> stcList;
	public List<PhysicsEntity> dynList;

	public PhysicsWorld () {
		ins = this;
		stcList = new List<PhysicsEntity>();
		dynList = new List<PhysicsEntity>();
	}
	public void Add(PhysicsEntity obj) {
		if (!obj.immovable) dynList.Add(obj);
		else stcList.Add(obj);
	}
	public void Remove(PhysicsEntity obj) {
		if (!obj.immovable) dynList.Remove(obj);
		else stcList.Remove(obj);
	}

	void Update () {
		Pass();
		Pass();
	}

	void Pass () {
		for (int i = 0; i < dynList.Count; ++i) {
			var obj = dynList[i];
			var h = CheckH(obj, i);
			var v = CheckV(obj, i);
			if (h.Item1 == 0 || v.Item1 == 0) continue;
			if (Mathf.Abs(h.Item1) < Mathf.Abs(v.Item1)) {
				obj.transform.position += h.Item1 * Vector3.right;

				var normal = (h.Item1 * Vector3.right).normalized;
				var collisionInfo = new CollisionInfo(obj, h.Item2, normal);
				obj.SendMessage("OnCollide", collisionInfo, SendMessageOptions.DontRequireReceiver);
				h.Item2.SendMessage("OnCollide", collisionInfo.GetOtherInfo(), SendMessageOptions.DontRequireReceiver);;
			} else {
				obj.transform.position += v.Item1 * Vector3.up;
			
				var normal = (v.Item1 * Vector3.up).normalized;
				var collisionInfo = new CollisionInfo(obj, v.Item2, normal);
				obj.SendMessage("OnCollide", collisionInfo, SendMessageOptions.DontRequireReceiver);
				v.Item2.SendMessage("OnCollide", collisionInfo.GetOtherInfo(), SendMessageOptions.DontRequireReceiver);;
			}
		}
	}

	private Tuple<float, PhysicsEntity> CheckH (PhysicsEntity obj, int i) {
		var objRc = obj.GetWorldRectOrtho();
		float hShortest = 0;
		PhysicsEntity other = null;
		for (int j = 0; j < stcList.Count; ++j) {
			float h = HIntersectDepth(objRc, stcList[j].GetWorldRectOrtho());
			if (Mathf.Abs(h) > Mathf.Abs(hShortest)) {
				hShortest = h;
				other = stcList[j];
			}
		}
		for (int j = i + 1; j < dynList.Count; ++j) {
			float h = HIntersectDepth(objRc, dynList[j].GetWorldRectOrtho());
			if (Mathf.Abs(h) > Mathf.Abs(hShortest)) {
				hShortest = h;
				other = dynList[j];
			}
		}

		return new Tuple<float, PhysicsEntity>(hShortest, other);
	}

	private Tuple<float, PhysicsEntity> CheckV (PhysicsEntity obj, int i) {
		var objRc = obj.GetWorldRectOrtho();
		float vShortest = 0;
		PhysicsEntity other = null;
		for (int j = 0; j < stcList.Count; ++j) {
			float v = VIntersectDepth(objRc, stcList[j].GetWorldRectOrtho());
			if (Mathf.Abs(v) > Mathf.Abs(vShortest)) {
				vShortest = v;
				other = stcList[j];
			}
		}
		for (int j = i + 1; j < dynList.Count; ++j) {
			float v = VIntersectDepth(objRc, dynList[j].GetWorldRectOrtho());
			if (Mathf.Abs(v) > Mathf.Abs(vShortest)) {
				vShortest = v;
				other = dynList[j];
			}
		}

		return new Tuple<float, PhysicsEntity>(vShortest, other);
	}

	public bool RectOverlap (Rect a, Rect b) {
		return a.xMax > b.xMin && a.xMin < b.xMax && a.yMax > b.yMin && a.yMin < b.yMax;
	}

	public float HIntersectDepth (Rect a, Rect b) {
		if (RectOverlap(a, b)) {
			float d1 = a.xMax - b.xMin,
				  d2 = b.xMax - a.xMin;
			if (d1 < d2) return -d1;
			else return d2;
		} else return 0;
	}

	public float VIntersectDepth (Rect a, Rect b) {
		if (RectOverlap(a, b)) {
			float d1 = a.yMax - b.yMin,
				  d2 = b.yMax - a.yMin;
			if (d1 < d2) return -d1;
			else return d2;
		} else return 0;
	}

	// --
	// Available functions

	// public bool CheckRect (Rect rc) { // unchecked
	// 	foreach (PhysicsEntity obj in dynList) {
	// 		if (obj.IntersectRect(rc, Vector3.zero).HasValue)
	// 			return true;
	// 	}
	// 	foreach (PhysicsEntity obj in stcList) {
	// 		if (obj.IntersectRect(rc, Vector3.zero).HasValue)
	// 			return true;
	// 	}
	// 	return false;
	// }
}