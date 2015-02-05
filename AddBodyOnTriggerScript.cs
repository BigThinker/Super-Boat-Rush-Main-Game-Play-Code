using UnityEngine;
using System.Collections;

/// <summary>
/// Script used to attach a RigidBody to the GameObject it is attached to, when it
/// collides with the Player.
/// </summary>
public class AddBodyOnTriggerScript : MonoBehaviour {
	
	// Variables edited in Unity Editor.
	public float Mass = 200000;
	public float Drag = 0;
	public float AngularDrag = 0;

	private Rigidbody m_Body;
	
	/// <summary>
	/// Dispatched by Unity, it attaches a RigidBody component
	/// to the object with this attached Script, putting it
	/// through the physics engine simulation.
	/// </summary>
	/// <param name="collider">The collider of the GameObject that hit this GameObject.</param>
	void OnTriggerEnter (Collider collider) {
		
		// Player is tagger with the keyword "Player" in Unity Editor.
		if (!collider.CompareTag("Player")) 
			return;
		
		// it the body is not initialized, than do so 
		// and attach the physical properties assigned by the designer.
		if (!m_Body) {
			gameObject.AddComponent<Rigidbody>();
			m_Body = GetComponent<Rigidbody>();
			m_Body.mass = Mass;
			m_Body.drag = Drag;
			m_Body.angularDrag = AngularDrag;
		}
	}
}
