import UnityEngine

class Test (MonoBehaviour): 

	def Start ():
		pass
	
	def Update ():
		transform.position.x += Input.GetAxis("Horizontal") * Time.deltaTime
		transform.position.y += Input.GetAxis("Vertical") * Time.deltaTime
