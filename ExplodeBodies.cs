using UnityEngine;
using System.Collections;

/// <summary>
/// Script used to make the GameObject it is attached to and all the children parented by that object,
/// to explode when colliding with another GameObject (i.e. bullets, boat), and sink it the ground after the explosion.
/// </summary>
public class ExplodeBodies : MonoBehaviour {
	
	// Variables edited in Unity Editor.
	public float ExplosionForce = 1500;
	public float ExplosionRadius = 20;
	public float FallSecondsAfterExplosion = 5;
	public float Mass = 500;
	public float Drag = 1;
	public float AngularDrag = 1;
	
	// flag indicating whether the explosion occurred.
	private bool m_HasExploded;
	// timer used to control when the GameObject is destroyed.
	private float m_DestroyTimer;
	// flag indicating whether the GameObject is falling or not.
	private bool m_Falling;
	
	/// <summary>
	/// Reset all the variables.
	/// </summary>
	private void Start () {
	
		m_HasExploded = false;
		m_Falling = false;
		m_DestroyTimer = 0;
		
		SwitchColliders(true);
	}
	
	/// <summary>
	/// Reset all the variables.
	/// </summary>
	private void AddBody () {
		if (!GetComponent<Rigidbody>()) {
			
			// used for iteration.
			Rigidbody body;
			
			// if it has a MeshRenderer attached (not just a Transform).
			if (GetComponent<MeshRenderer>()) {
			
				// attach a RigidBody.
				body = gameObject.AddComponent<Rigidbody>();
				body.useGravity = false;
				body.drag = Drag;
				body.angularDrag = AngularDrag;
				body.mass = Mass;
			}
			
			AddBodiesToChildren();
		}
	}
	
	private void AddBodiesToChildren() {
		
		// used for iteration.
		Rigidbody body;
		
		// grab the number of children, in order to iterate.
		int num_objects = transform.childCount;
		
		for (int i = 0; i < num_objects; i++) {
			
			// if the child doesn't already have a RigidBody, and it has a MeshRenderer (not just a Transform).
			if (!transform.GetChild(i).GetComponent<Rigidbody>() 
			    && transform.GetChild(i).GetComponent<MeshRenderer>()) {
			    
			    // Then, attach a RigidBody to the child and assign the values.
				body = transform.GetChild(i).gameObject.AddComponent<Rigidbody>();
				body.useGravity = false;
				body.drag = Drag;
				body.angularDrag = AngularDrag;
				body.mass = Mass;
			}
		}
	}
	
	/// <summary>
	/// Function used to attach MeshCollider to the GameObject this script is attached to (if it has a MeshRenderer component).
	/// </summary>
	/// <param name="on">Flag indicating whether the colliders should be attached, or destroyed. </param>
	private void SwitchColliders (bool on) {
		
		// used for iteration.
		MeshCollider meshC;
		
		// this GameObject should have mesh renderer.
		if (GetComponent<MeshRenderer>()) {
			
			// Grab the MeshCollider from this GameObject.
			meshC = GetComponent<MeshCollider>();
			
			// If it should be turned on and there is no already MeshCollider to this GameObject (no double).
			if (on && !meshC) {
				
				// Add the MeshCollider.
				meshC = gameObject.AddComponent<MeshCollider>();
				// Make it convex.
				meshC.convex = true;
				
			}
			// Otherwise it should turn off, and there needs to be a MeshCollider attached. 
			else if (meshC) {
				Destroy(meshC);
			}
		}
		
		// Do the same process to each child of this GameObject.
		SwitchCollidersToChildren(on);
	}
	
	/// <summary>
	/// Function used to attach MeshColliders to all children GameObjects parented by this GameObject. (if they have a MeshRenderer component).
	/// </summary>
	/// <param name="on">Flag indicating whether the colliders should be attached, or destroyed. </param>
	private void SwitchCollidersToChildren(bool on) {
		
		// used for iteration.
		MeshCollider meshC;
		
		// First get the number of children for iteration.
		int num_objects = transform.childCount;
		
		// than iterate through each child.
		for (int i = 0; i < num_objects; i++) {
			
			// Grab the MeshCollider from this GameObject.
			meshC = transform.GetChild(i).GetComponent<MeshCollider>();
			
			// the child GameObject should have MeshRenderer attached.
			if (transform.GetChild(i).GetComponent<MeshRenderer>()) {
				
				// If it should be turned on and there is no already MeshCollider to this GameObject (no double).
				if (on && !meshC) {
					
					// Add the MeshCollider.
					meshC = transform.GetChild(i).gameObject.AddComponent<MeshCollider>();
					// Make it convex.
					meshC.convex = true;
				}
				// Otherwise it should turn off, and there needs to be a MeshCollider attached. 
				else if (meshC) {
					Destroy(meshC);
				}
			}
		}
	}
	
	/// <summary>
	/// Dispatched by Unity, it will fire up the explosion of all the parts of this GameObject.
	/// </summary>
	/// <param name="collider">The collider of the GameObject that hit this GameObject.</param>
	private void OnTriggerEnter (Collider collider) {
		Explode();
	}
	
	/// <summary>
	/// Main function of the script, used to make the explosion happen 
	/// by manipulating the RigidBody of this GameObject and all its children.
	/// </summary>
	private void Explode () {
		
		AddBody();
	
		// turn on the flag (in order to prevent double call).
		m_HasExploded = true;
		
		// used for iteration.
		Rigidbody body;
		
		// make this body fall and explode.
		body = GetComponent<Rigidbody>();
		if (body) {
			body.useGravity = true;
			body.AddExplosionForce(ExplosionForce, transform.parent.position, ExplosionRadius, 0, ForceMode.Impulse);
		}
		
		// count the children for iteration.
		int num_objects = transform.childCount;
		
		// make children fall and explode.
		for (int i = 0; i < num_objects; i++) {
			body = transform.GetChild(i).GetComponent<Rigidbody>();
			if (body) {
				body.useGravity = true;
				body.AddExplosionForce(ExplosionForce, transform.position, ExplosionRadius, 0, ForceMode.Impulse);
			}
		}
	}

	/// <summary>
	/// Update loop where falling is enabled and GameObject is destroyed.
	/// </summary>
	private void Update () {
		
		if (m_HasExploded) {
			
			// add the timer. 
			m_DestroyTimer += Time.deltaTime;
			
			// if it reaches the Time specified for falling.
			if (m_DestroyTimer >= FallSecondsAfterExplosion) {
				
				// fall off the ground (turn off the colliders).
				if (!m_Falling) {
					m_Falling = true;
					SwitchColliders(false);
				}
				// twice after this time, the object is destroyed (out of player's vision).
				else if (m_DestroyTimer > FallSecondsAfterExplosion * 2) {
					Destroy(gameObject);
				}
			}
		}
	}
}
