using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Eppy;

public class CollisionInfo {
	private PhysicsEntity a, b;
	private Vector2 _normal;

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
}

[AddComponentMenu("Physics/PhysicsEntity")]

public class PhysicsEntity : MonoBehaviour {
	public bool immovable = false; // now it can't be changed during the game
	public bool trigger = false; // the trigger won't push objects back

	public bool debugLog = false;

	public Vector3 oldPosition;

	public Vector3 moveVector {
		get { return transform.position - oldPosition; }
	}

	public bool isOrtho {
		get { return transform.rotation == Quaternion.identity; }
	}

	void Awake () {
		oldPosition = transform.position;
	}

	void OnEnable () {
		PhysicsWorld.ins.Add(this);
	}

	void OnDisable () {
		PhysicsWorld.ins.Remove(this);	

	}

	public void PhysicsUpdate () {
		oldPosition = transform.position;
	}
	// only support box shapes
	public Rect shape = new Rect(0, 0, 16, 16);
	public float shapeRadius {
		get { return Mathf.Max(Mathf.Abs(shape.x + shape.y), Mathf.Abs(shape.x + shape.y + shape.width + shape.height)); }
	}

	public bool _RoughTestIntersecting (PhysicsEntity other) {
		return ((Vector2)this.transform.position - (Vector2)other.transform.position).magnitude <= this.shapeRadius + other.shapeRadius;
	}

	static public Rect GetWorldRectOrtho (Rect shape, Vector3 offset) {
		Rect rc = shape;
		return new Rect(offset.x + rc.x, offset.y + rc.y, rc.width, rc.height);
	}

	public Rect GetWorldRectOrtho () {
		Rect rc = shape;
		return new Rect(transform.position.x + rc.x, transform.position.y + rc.y, rc.width, rc.height);
	}

	public Rect GetOldWorldRectOrtho () {
		Rect rc = shape;
		return new Rect(oldPosition.x + rc.x, oldPosition.y + rc.y, rc.width, rc.height);
	}


	public bool Contains (Vector2 point) {
		if (transform.rotation == Quaternion.identity) {
			Rect rc = shape;
			return new Rect(rc.x + transform.position.x, rc.y + transform.position.y, rc.width, rc.height).Contains(point);
		}
		return false;
	}

	void OnDrawGizmos () {
		Color color = Color.blue;
		Rect rc = shape;
		Utils.DrawRectGizmos(rc, color, transform.position, transform.rotation);
	}
}
