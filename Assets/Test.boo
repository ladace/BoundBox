import UnityEngine

class Test (MonoBehaviour): 

	def Start ():
		pass
	
	def Update ():
		transform.position.x += Input.GetAxis("Horizontal") * Time.deltaTime * 4
		transform.position.y += Input.GetAxis("Vertical") * Time.deltaTime * 4

	def OnCollide (info):
		print ("Collide!")
