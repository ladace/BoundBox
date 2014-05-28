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

	void Update() {
		for (int i = 0; i < dynList.Count; ++i) {
			for (int j = 0; j < stcList.Count; ++j) {
				Solve(dynList[i], stcList[j]);
			}
		}

		for (int i = 0; i < dynList.Count; ++i) {
			for (int j = i + 1; j < dynList.Count; ++j) {
				Solve(dynList[i], dynList[j]);
			}
		}
	}

	public void Solve (PhysicsEntity a, PhysicsEntity b) {
		if (!a._RoughTestIntersecting(b)) return;

		Vector2? srp = a.Intersect(b);
		if (srp.HasValue) {
			CollisionInfo info = new CollisionInfo(a, b, srp.Value.normalized);
			CollisionInfo oInfo = info.GetOtherInfo();
			a.SendMessage("OnColliding", info, SendMessageOptions.DontRequireReceiver);
			b.SendMessage("OnColliding", oInfo, SendMessageOptions.DontRequireReceiver);
			if (!(a.trigger || b.trigger || info.canceled || oInfo.canceled)){
				if (!a.immovable && (a.massGroup <= b.massGroup || b.immovable)) a.transform.position += (Vector3)srp.Value;
				else if (!b.immovable && (a.massGroup >= b.massGroup || a.immovable)) b.transform.position -= (Vector3)srp.Value;
			}
		}
	}

	// --
	// Available functions

	public bool CheckRect (Rect rc) { // unchecked
		foreach (PhysicsEntity obj in dynList) {
			if (obj.IntersectRect(rc, Vector3.zero, Quaternion.identity).HasValue)
				return true;
		}
		foreach (PhysicsEntity obj in stcList) {
			if (obj.IntersectRect(rc, Vector3.zero, Quaternion.identity).HasValue)
				return true;
		}
		return false;
	}
}