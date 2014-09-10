using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Eppy;


public class PhysicsWorld : MonoBehaviour {
	static public PhysicsWorld ins;
	public List<PhysicsEntity> stcList;
	public List<PhysicsEntity> dynList;

	static private float EPS = 1e-6f;

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
		SolveAll();
		foreach (var e in dynList) {
			e.PhysicsUpdate();
		}
	}

	void SolveAll () {
		for (int i = 0; i < dynList.Count; ++i) {
			var obj = dynList[i];

			if ((Vector2)obj.moveVector != Vector2.zero) {

				var hitlst = GetHitObjects(i).ToList();

				hitlst.Sort((a, b) => Compare(Project(a.GetWorldRectOrtho(), obj.moveVector.normalized), Project(b.GetWorldRectOrtho(), obj.moveVector.normalized)));
				// resolve
				foreach (var other in hitlst) {
					if (obj.isOrtho && other.isOrtho) {
						// resolve in the ortho way
						var lRc = obj.GetWorldRectOrtho();
						var rRc = other.GetOldWorldRectOrtho();

						if (RectOverlap(lRc, rRc)) {
							float hl = rRc.xMin - lRc.xMax;
							float hr = rRc.xMax - lRc.xMin;
							float h = obj.moveVector.x - other.moveVector.x > 0 ? hl : hr;

							float vd = rRc.yMin - lRc.yMax;
							float vu = rRc.yMax - lRc.yMin;
							float v = obj.moveVector.y - other.moveVector.y > 0 ? vd : vu;

							float rh = h * obj.moveVector.x > 0 ? 0 : Mathf.Abs(h) > Mathf.Abs(obj.moveVector.x) ? -obj.moveVector.x : h;
							float rv = v * obj.moveVector.y > 0 ? 0 : Mathf.Abs(v) > Mathf.Abs(obj.moveVector.y) ? -obj.moveVector.y : v;

							Vector2 resDir = Mathf.Abs(h) < Mathf.Abs(v) ? rh * Vector2.right : rv * Vector2.up;

							if (resDir != Vector2.zero) {
								if (!obj.trigger && !other.trigger) obj.transform.position += (Vector3)resDir;

								SendCollisionMessage(resDir.normalized, obj, other);							
							}
						}
					} else {
						// resolve in the rotated way
						Rect aRc = obj.shape,
							 bRc = other.shape;

						bool collide = true;

						Vector2? resDir = null;
						foreach (Vector2 axis in Geometry.AxesOfRotatedRect(obj.transform.rotation)) {
							if (!_IntersectRectOnAxis (aRc, bRc, obj.transform, other.transform, axis, ref resDir)) {
								collide = false;
								break;
							}
						}

						if (collide) {
							foreach (Vector2 axis in Geometry.AxesOfRotatedRect(other.transform.rotation)) {
								if (!_IntersectRectOnAxis (aRc, bRc, obj.transform, other.transform, axis, ref resDir)) {
									collide = false;
									break;
								}
							}
						}

						if (collide && resDir.HasValue) {
							float h = resDir.Value.x;
							float v = resDir.Value.y;

							float rh = h * obj.moveVector.x > 0 ? 0 : Mathf.Abs(h) > Mathf.Abs(obj.moveVector.x) ? -obj.moveVector.x : h;
							float rv = v * obj.moveVector.y > 0 ? 0 : Mathf.Abs(v) > Mathf.Abs(obj.moveVector.y) ? -obj.moveVector.y : v;

							if (!obj.trigger && !other.trigger) obj.transform.position += new Vector3(rh, rv);
							SendCollisionMessage(resDir.Value.normalized, obj, other);							
						}
					}
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
		for (int j = 0; j < stcList.Count; ++j) {
			if (dynList[i]._RoughTestIntersecting(stcList[j]))
				yield return stcList[j];
		}
		for (int j = 0; j < dynList.Count; ++j) {
			if (j == i) continue;
			if (dynList[i]._RoughTestIntersecting(dynList[j]))
				yield return dynList[j];
		}
	}

	private bool IsHitting (int i) {
		var objRc = dynList[i].GetWorldRectOrtho();
		foreach (PhysicsEntity o in GetHitObjects(i)) {
			if (RectOverlap(objRc, o.GetWorldRectOrtho()))
				return true;
		}
		return false;
	}

	public bool RectOverlap (Rect a, Rect b) {
		return a.xMax > b.xMin + EPS && a.xMin + EPS < b.xMax && a.yMax > b.yMin + EPS && a.yMin + EPS < b.yMax;
	}

	static protected Rect GetOffsetRect (float x, float y, Rect rc) {
		return new Rect(x + rc.x, y + rc.y, rc.width, rc.height);
	}

	static protected int Compare (float a, float b) {
		return a < b ? -1 : (a > b ? 1 : 0);
	}

	static protected float Project (Rect rc, Vector2 direction) {
		return Mathf.Min(Vector2.Dot(new Vector2(rc.xMin, rc.yMin), direction),
					     Vector2.Dot(new Vector2(rc.xMax, rc.yMin), direction),
						 Vector2.Dot(new Vector2(rc.xMax, rc.yMax), direction),
						 Vector2.Dot(new Vector2(rc.xMin, rc.yMax), direction));
	}

	static protected bool _IntersectRectOnAxis (Rect aRc, Rect bRc, Transform aTrans, Transform bTrans, Vector2 axis, ref Vector2? minCross) {
		var aR = Geometry.ProjectRect(aRc, aTrans.position, aTrans.rotation, axis);
		var bR = Geometry.ProjectRect(bRc, bTrans.position, bTrans.rotation, axis);
		// the shortest distance to move aR out of bR
		float overl = Utils.SegmentInto(aR, bR);
		if (overl == 0) return false;
		else {
			if (minCross.HasValue) {
				if (Mathf.Abs(overl) < minCross.Value.magnitude) {
					minCross = overl * axis;
				}
			} else minCross = overl * axis;
			return true;
		}
	}
}