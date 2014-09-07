using UnityEngine;
using System.Collections;

public class CollisionTest : MonoBehaviour {

	private string touching = "";
	
	void Update () {
		transform.position += new Vector3(Input.GetAxis("Horizontal") * Time.deltaTime * 4, Input.GetAxis("Vertical") * Time.deltaTime * 4);
		touching = "";
	}

	void OnCollide (CollisionInfo info) {
		touching += info.normal.ToString();
	}

	void OnGUI () {
		GUI.Box(new Rect(0, 0, Screen.width, 40), touching);
		if (touching != "") Debug.Log(touching);
	}

}
