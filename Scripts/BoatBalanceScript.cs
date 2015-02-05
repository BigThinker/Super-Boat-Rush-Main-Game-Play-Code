using UnityEngine;
using System.Collections;

/// <summary>
/// BoatBalance script makes sure the boat doesn't get stuck on the terrain because of high speed
/// and it also prevents flipping of the boat.
/// </summary>
public class BoatBalanceScript : MonoBehaviour {
	
	// how much units to prevent flipping under.
	public float PreventFlippingUnderUnits = 5;
	// how many seconds to wait while one of the wheels is underground (this is for double safety check).
	public float WaitStuckWheelsTime = 1f;
	// tolerance (in percentage) = how much can the wheel be inside and considered safe.
	public float WheelOnGroundTolerance = 0.2f;
	
	// reference of the wheel colliders.
	private WheelCollider m_WheelFL;
	private WheelCollider m_WheelFR;
	private WheelCollider m_WheelRL;
	private WheelCollider m_WheelRR;
	
	// reference of the radius for a wheel.
	private float m_Radius;
	
	private float m_WaitStuckWheelsTimer;
	// used to save the normal distance of the wheel from the ground.
	private float m_DistanceFromGround;
	
	public void StartBalance () {
	
		// grab the references from the controller attached in the same GameObject.
		m_WheelFL = GetComponent<BoatController>().GetFrontLeftWheel();
		m_WheelFR = GetComponent<BoatController>().GetFrontRightWheel();
		m_WheelRL = GetComponent<BoatController>().GetRearLeftWheel();
		m_WheelRR = GetComponent<BoatController>().GetRearRightWheel();
		
		m_WaitStuckWheelsTimer = 0;
		m_Radius = m_WheelFL.radius;
		
		// Have to make sure the boats are on ground level when starting game.
		CalculateDistanceFromGround();
	}
	
	void CalculateDistanceFromGround () {
		// Calculate the normal distance of the ship from the ground.
		RaycastHit hit;
		Physics.Raycast(transform.position, -transform.up, out hit);
		m_DistanceFromGround = hit.distance;
	}
	
	void Update () {
		
		if (!GetComponent<BoatController>().HasMinimumSpeed()) 
			return;
		
		// above 5.
		if (!IsLessThenUnitsAboveGround(PreventFlippingUnderUnits))
			PreventFlip();
		// wheels + time test.
		else if (AreWheelsStuck()) {
			UnstuckWheels();
		}
	}
	
	/// <summary>
	/// Function to test whether wheels are on the ground based on 2 factors:
	/// 1. Any of the wheels is under the ground for more than a specific amount of time.
	/// 2. Velocity magnitude of the boat is under 1, which would happen only when boat is stuck.
	/// </summary>
	bool AreWheelsStuck () {
		
		bool stuck = false;
		
		// stuck = no speed (or very low) almost 
		if (IsAnyOfWheelsBelowGround() || rigidbody.velocity.magnitude < 1) {
		
			m_WaitStuckWheelsTimer += Time.deltaTime;
			
			if (m_WaitStuckWheelsTimer >= WaitStuckWheelsTime) {
				m_WaitStuckWheelsTimer = 0;
				
				stuck = true;
			}
		} else {
			m_WaitStuckWheelsTimer = 0;
		}
		
		return stuck;
	}
	
	/// <summary>
	/// Function to "keep the composure" of the boat in order to prevent flip.
	/// It doesn't allow rotation on the Y axis.
	/// </summary>
	void PreventFlip () {
		
		Quaternion resetRotation = Quaternion.identity;
		Vector3 eulerRotation = resetRotation.eulerAngles;
		eulerRotation.y = transform.eulerAngles.y; // don't change y rotation.
		resetRotation.eulerAngles = eulerRotation;
		transform.rotation = Quaternion.Lerp(transform.rotation, resetRotation, 0.25f);
	}
	
	/// <summary>
	/// Function to test if any of the wheels is under the ground by using RayCast on each wheel.
	/// </summary>
	bool IsAnyOfWheelsBelowGround () {
		
		float wheelUnderGround = m_Radius - m_Radius * WheelOnGroundTolerance;
		float wheelAboveGround = m_Radius + m_Radius * WheelOnGroundTolerance;
		
		bool stuck = false;
		RaycastHit hit;
		
		Physics.Raycast(m_WheelFL.transform.position, -m_WheelFL.transform.up, out hit);
		
		if (hit.distance < wheelUnderGround || hit.distance > wheelAboveGround) {
			stuck = true;
		}
		
		Physics.Raycast(m_WheelFR.transform.position, -m_WheelFR.transform.up, out hit);
		
		if (hit.distance < wheelUnderGround || hit.distance > wheelAboveGround) {
			stuck = true;
		}
		
		Physics.Raycast(m_WheelRL.transform.position, -m_WheelRL.transform.up, out hit);
		
		if (hit.distance < wheelUnderGround || hit.distance > wheelAboveGround) {
			stuck = true;
		}
		
		Physics.Raycast(m_WheelRR.transform.position, -m_WheelRR.transform.up, out hit);
		
		if (hit.distance < wheelUnderGround || hit.distance > wheelAboveGround) {
			stuck = true;
		}
		
		return stuck;
	}
	
	/// <summary>
	/// Function used to push the boat a little bit "up" to reverse the stucking under terrain.
	/// </summary>
	void UnstuckWheels () {
		
		// Just push the vector up.
		Vector3 sendUp = new Vector3(m_DistanceFromGround * transform.up.x, 
										m_DistanceFromGround * transform.up.y, 
										m_DistanceFromGround * transform.up.z);
		transform.position += sendUp;
	}
	
	/// <summary>
	/// Returns true if the object is less then 'units' above ground.
	/// </summary>
	bool IsLessThenUnitsAboveGround (float units) {
	
		RaycastHit hit;
		Physics.Raycast(transform.position, -transform.up, out hit);
		
		return hit.distance < units;
	}
}
