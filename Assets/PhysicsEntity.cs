using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Eppy;

public class CollisionInfo {
	private PhysicsEntity a, b;
	private Vector2 _normal;
	private bool _canceled = false;
	public bool canceled {
		get { return _canceled; }
	}
	public Vector2 normal {
		get { return _normal; }
	}
	public CollisionInfo (PhysicsEntity a, PhysicsEntity b, Vector2 normal) {
		this.a = a;
		this.b = b;
		this._normal = normal;
	}
	public PhysicsEntity GetOther () {
		return b;
	}
	public CollisionInfo GetOtherInfo () {
		return new CollisionInfo(b, a, -normal);
	}

	public void CancelCollision () {
		_canceled = true;
	}
}

// 2D physic object
[AddComponentMenu("Boss Physics/PhysicsEntity")]

public class PhysicsEntity : MonoBehaviour {
	public bool immovable = false;
	public bool trigger = false;

	public enum EntityType { OBJECT, EVENT }
	public EntityType entityType = EntityType.OBJECT;
	public int massGroup = 0;

	public bool debugLog = false;

	void Start () {
		PhysicsWorld.ins.Add(this);
	}
	public void OnDestroy() {
		PhysicsWorld.ins.Remove(this);
	}

	// only support box shapes
	public Rect shape = new Rect(0, 0, 16, 16);
	public float shapeRadius {
		get { return Mathf.Max(Mathf.Abs(shape.x + shape.y), Mathf.Abs(shape.x + shape.y + shape.width + shape.height)); }
	}

	public bool _RoughTestIntersecting (PhysicsEntity other) {
		return ((Vector2)this.transform.position - (Vector2)other.transform.position).magnitude <= this.shapeRadius + other.shapeRadius;
	}
	// return the shortest recover distance -> move `this` out of `other`
	public Vector2? Intersect (PhysicsEntity other) {
		return IntersectRect(other.shape, other.transform.position, other.transform.rotation);
	}

	public Vector2? IntersectRect (Rect rc, Vector3 offset, Quaternion otherRotation) {
		if (transform.rotation == Quaternion.identity && otherRotation == Quaternion.identity)
			return IntersectOrtho(GetWorldRectOrtho(rc, offset));
		else {
			// TODO Unchecked after refactoring
			Rect aRc = shape,
				 bRc = rc;

			Vector2? minCross = null;
			foreach (Vector2 axis in Geometry.AxesOfRotatedRect(transform.rotation)) {
				if (!_IntersectRectOnAxis (aRc, bRc, offset, otherRotation, axis, ref minCross)) return null;
			}
			foreach (Vector2 axis in Geometry.AxesOfRotatedRect(otherRotation)) {
				if (!_IntersectRectOnAxis (aRc, bRc, offset, otherRotation, axis, ref minCross)) return null;
			}
			return minCross;
		}
	}
	private bool _IntersectRectOnAxis (Rect aRc, Rect bRc, Vector3 offset, Quaternion rotation, Vector2 axis, ref Vector2? minCross) {
		var aR = Geometry.ProjectRect(aRc, transform.position, transform.rotation, axis);
		var bR = Geometry.ProjectRect(bRc, offset, rotation, axis);
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
	public Vector2? IntersectOrtho (Rect otherRect) {
		Rect rc = GetWorldRectOrtho();

		if (rc.x < otherRect.x + otherRect.width && rc.y < otherRect.y + otherRect.height
			&& rc.x + rc.width > otherRect.x && rc.y + rc.height > otherRect.y) {
			Vector2[] d = new Vector2[] { new Vector2(otherRect.x + otherRect.width - rc.x, 0),
										  new Vector2(otherRect.x - rc.x - rc.width, 0),
										  new Vector2(0, otherRect.y + otherRect.height - rc.y),
										  new Vector2(0, otherRect.y - rc.y - rc.height) };
			Vector2 ret = d[0];
			foreach (Vector2 v in d) {

				if (v.magnitude < ret.magnitude)
					ret = v;
			}
			return ret;
		} else return null;
	}

	static public Rect GetWorldRectOrtho (Rect shape, Vector3 offset) {
		Rect rc = shape;
		return new Rect(offset.x + rc.x, offset.y + rc.y, rc.width, rc.height);
	}

	public Rect GetWorldRectOrtho () {
		Rect rc = shape;
		return new Rect(transform.position.x + rc.x, transform.position.y + rc.y, rc.width, rc.height);
	}

	public bool Contains (Vector2 point) {
		if (transform.rotation == Quaternion.identity) {
			Rect rc = shape;
			return new Rect(rc.x + transform.position.x, rc.y + transform.position.y, rc.width, rc.height).Contains(point);
		}
		return false;//Todo: For rotated rects Undone yet
	}

	public bool HitTest (PhysicsEntity other) {
		return IntersectRect(other.shape, other.transform.position, other.transform.rotation).HasValue;
	}

	void OnDrawGizmos () {
		Color color = Color.blue;
		switch (entityType) {
		case EntityType.OBJECT:
			color = Color.blue;
			break;
		case EntityType.EVENT:
			color = Color.yellow;
			break;
		}
		Rect rc = shape;

		Utils.DrawRectGizmos(rc, color, transform.position, transform.rotation);
	}
}
