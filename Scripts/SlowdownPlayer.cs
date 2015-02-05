using UnityEngine;
using System.Collections;

/// <summary>
/// Script used to slowdown player when the GameObject it is attached to collides with the Player.
/// </summary>
public class SlowdownPlayer : MonoBehaviour {

	// Edited in Unity Editor.
	public float SlowTime = 4f;
	
	void OnTriggerEnter (Collider collider) {
		
		if (!collider.CompareTag("Player"))
			return;
			
		BoatController boatMechanics = collider.GetComponent<BoatController>();
		boatMechanics.StopEngineFor(SlowTime);
	}
}
