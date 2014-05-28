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
		return IntersectRect(other.shape, other.transform.position);
	}

	public Vector2? IntersectRect (Rect rc, Vector3 offset) {
		return IntersectOrtho(GetWorldRectOrtho(rc, offset));
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
		return IntersectRect(other.shape, other.transform.position).HasValue;
	}

	void OnDrawGizmos () {
		Color color = Color.blue;
		Rect rc = shape;
		Utils.DrawRectGizmos(rc, color, transform.position, Quaternion.identity);//transform.rotation);
	}
}
