using UnityEngine;
using System.Collections;

public class DestroyBoatOnHitScript : MonoBehaviour {
	
	// Variables edited in Unity Editor.
	public Vector3 RespawnPosition = new Vector3(2624, 213, 962);

	/// <summary>
	/// Dispatched by Unity, calls Respawn at the Player when it collides with this GameObject.
	/// </summary>
	/// <param name="collider">The collider of the GameObject that hit this GameObject.</param>
	void OnTriggerEnter (Collider collider) {
		
		if (collider.CompareTag("Player")) {
			
			BoatController boatMechanics = collider.GetComponent<BoatController>();
			boatMechanics.Respawn(RespawnPosition);
		}
	}
}
