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
		Pass1();
		Pass2();
		foreach (var e in dynList) {
			e.PhysicsUpdate();
		}
	}

	void Pass1 () {
		for (int i = 0; i < dynList.Count; ++i) {
			var obj = dynList[i];
			var h = CheckH(obj, i);
			var v = CheckV(obj, i);
			if (h.Item1 == 0 && v.Item1 == 0) {
				if (IsHitting(i)) { // it might be still hitting
					// shift the horizontal position first
					// the second pass will deal with the vertical movement
					obj.oldPosition.x = obj.transform.position.x;
				}
				continue;
			}
			if (h.Item1 != 0 && (v.Item1 == 0 || Mathf.Abs(h.Item1) < Mathf.Abs(v.Item1))) {
				obj.transform.position += h.Item1 * Vector3.right;
				obj.oldPosition.x = obj.transform.position.x;

				// send message
				var normal = (h.Item1 * Vector3.right).normalized;
				SendCollisionMessage(normal, obj, h.Item2);

			} else {
				obj.transform.position += v.Item1 * Vector3.up;
				obj.oldPosition.y = obj.transform.position.y;
			
				// send message
				var normal = (v.Item1 * Vector3.up).normalized;
				SendCollisionMessage(normal, obj, v.Item2);
			}
		}
	}

	void Pass2 () {
		for (int i = 0; i < dynList.Count; ++i) {
			var obj = dynList[i];
			if (obj.transform.position.x != obj.oldPosition.x) {
				var h = CheckH(obj, i);

				if (h.Item1 != 0) {
					obj.transform.position += h.Item1 * Vector3.right;

					// send message
					var normal = (h.Item1 * Vector3.right).normalized;
					SendCollisionMessage(normal, obj, h.Item2);
				}
			}

			if (obj.transform.position.y != obj.oldPosition.y) {
				var v = CheckV(obj, i);

				if (v.Item1 != 0) {
					obj.transform.position += v.Item1 * Vector3.up;

					// send message
					var normal = (v.Item1 * Vector3.up).normalized;
					SendCollisionMessage(normal, obj, v.Item2);
				}
			}
		}
	}

	private void SendCollisionMessage (Vector3 normal, PhysicsEntity obj, PhysicsEntity other) {
		var collisionInfo = new CollisionInfo(obj, other, normal);
		obj.SendMessage("OnCollide", collisionInfo, SendMessageOptions.DontRequireReceiver);
		other.SendMessage("OnCollide", collisionInfo.GetOtherInfo(), SendMessageOptions.DontRequireReceiver);
	}

	private IEnumerable<PhysicsEntity> GetHitObjects (int i) {
		for (int j = 0; j < stcList.Count; ++j)
			yield return stcList[j];
		for (int j = i + 1; j < dynList.Count; ++j)
			yield return dynList[j];
	}

	private bool IsHitting (int i) {
		var objRc = dynList[i].GetWorldRectOrtho();
		foreach (PhysicsEntity o in GetHitObjects(i)) {
			if (RectOverlap(objRc, o.GetWorldRectOrtho()))
				return true;
		}
		return false;
	}

	private Tuple<float, PhysicsEntity> CheckH (PhysicsEntity obj, int i) {
		var objRc = GetOffsetRect(obj.transform.position.x, obj.oldPosition.y, obj.shape);
		float hLeft = 0,
			  hRight = 0;
		PhysicsEntity oLeft = null,
					  oRight = null;

		foreach (PhysicsEntity o in GetHitObjects(i)) {
			var h = HIntersectDepth(objRc, o.GetWorldRectOrtho());
			if (h.Item1 < hLeft) { hLeft = h.Item1; oLeft = o; }
			if (h.Item2 > hRight) { hRight = h.Item2; oRight = o; }
		}
		if (Mathf.Abs(hLeft) < Mathf.Abs(hRight)) {
			return new Tuple<float, PhysicsEntity>(hLeft, oLeft);
		} else {
			return new Tuple<float, PhysicsEntity>(hRight, oRight);
		}
	}

	private Tuple<float, PhysicsEntity> CheckV (PhysicsEntity obj, int i) {
		var objRc = GetOffsetRect(obj.oldPosition.x, obj.transform.position.y, obj.shape);
		float vUp = 0,
			  vDown = 0;
		PhysicsEntity oUp = null,
					  oDown = null;

		foreach (PhysicsEntity o in GetHitObjects(i)) {
			var v = VIntersectDepth(objRc, o.GetWorldRectOrtho());
			if (v.Item1 < vDown) { vDown = v.Item1; oDown = o; }
			if (v.Item2 > vUp) { vUp = v.Item2; oUp = o; }
		}

		if (Mathf.Abs(vUp) < Mathf.Abs(vDown)) {
			return new Tuple<float, PhysicsEntity>(vUp, oUp);
		} else {
			return new Tuple<float, PhysicsEntity>(vDown, oDown);
		}

	}

	public bool RectOverlap (Rect a, Rect b) {
		return a.xMax > b.xMin && a.xMin < b.xMax && a.yMax > b.yMin && a.yMin < b.yMax;
	}

	// return the distance a should move
	// position - move right
	// negative - move left
	public Tuple<float, float> HIntersectDepth (Rect a, Rect b) {
		if (RectOverlap(a, b)) {
			float d1 = a.xMax - b.xMin,
				  d2 = b.xMax - a.xMin;
			return new Tuple<float, float>(-d1, d2);
		} else return new Tuple<float, float>(0, 0);
	}

	// return the distance a should move
	// position - move up
	// negative - move down
	public Tuple<float, float> VIntersectDepth (Rect a, Rect b) {
		if (RectOverlap(a, b)) {
			float d1 = a.yMax - b.yMin,
				  d2 = b.yMax - a.yMin;
			return new Tuple<float, float>(-d1, d2);
		} else return new Tuple<float, float>(0, 0);
	}

	static protected Rect GetOffsetRect(float x, float y, Rect rc) {
		return new Rect(x + rc.x, y + rc.y, rc.width, rc.height);
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