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
					var lRc = obj.GetWorldRectOrtho();
					var rRc = other.GetWorldRectOrtho();

					if (RectOverlap(lRc, rRc)) {
						float hl = rRc.xMin - lRc.xMax;
						float hr = rRc.xMax - lRc.xMin;
						float h = obj.moveVector.x - other.moveVector.x > 0 ? hl : hr;

						float rh = h * obj.moveVector.x > 0 ? 0 : Mathf.Abs(h) > Mathf.Abs(obj.moveVector.x) ? -obj.moveVector.x : h;

						float vd = rRc.yMin - lRc.yMax;
						float vu = rRc.yMax - lRc.yMin;
						float v = obj.moveVector.y - other.moveVector.y > 0 ? vd : vu;

						float rv = v * obj.moveVector.y > 0 ? 0 : Mathf.Abs(v) > Mathf.Abs(obj.moveVector.y) ? -obj.moveVector.y : v;

						Vector2 resDir = Mathf.Abs(h) < Mathf.Abs(v) ? rh * Vector2.right : rv * Vector2.up;

						if (resDir != Vector2.zero) {
							obj.transform.position += (Vector3)resDir;

							SendCollisionMessage(resDir.normalized, obj, other);							
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
			if (dynList[i]._RoughTestIntersecting(stcList[j]) && RectOverlap(dynList[i].GetWorldRectOrtho(), stcList[j].GetWorldRectOrtho()))
				yield return stcList[j];
		}
		for (int j = 0; j < dynList.Count; ++j) {
			if (j == i) continue;
			if (dynList[i]._RoughTestIntersecting(dynList[j]) && RectOverlap(dynList[i].GetWorldRectOrtho(), dynList[j].GetWorldRectOrtho()))
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
}