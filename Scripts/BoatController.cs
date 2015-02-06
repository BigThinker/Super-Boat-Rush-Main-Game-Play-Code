// =================================================================================================
//
// Super Boat Rush Main Game Play Scripts.
// Copyright (c) 2014 Aldo Leka
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
// =================================================================================================

using UnityEngine;
using System.Collections;

/// <summary>
/// BoatController script is the core script of the game since it get the input of the player
/// and manipulates the boat accordingly using wheel colliders, torque force, break, spinning (by changing
/// the friction of the wheels) etc.
/// The boat mechanics here work the same way as a normal car mechanics, via 4 wheels attached to a main
/// box collider (or more complex collider system). 
/// If you want to use this script, you have to create a game object and put 4 child game objects with wheel collider 
/// (change naming of the wheels appropriately at the vars below) and a collider to attach the wheels to (for correct physics).
/// Use W to activate / deactivate engine.
/// Use E to activate / deactivate turbo.
/// Use Arrows / WASD to control the boat (steering + reverse movement).
/// Use Space to break the boat.
/// </summary>
public class BoatController : MonoBehaviour {

	// Variables edited in Unity Editor.
	public float MinimumSpeed = 50;
	public float EngineSpeed = 350;
	public float TurboSpeed = 500;
	public float HillAngle = 60;
	public float HighSpeedSteerAngle = 1;
	public float LowSpeedSteerAngle = 4;
	public float DecelerationSpeed = 100;
	public float TurboTime = 5;
	public float SpinImpactTime = 5f;
	public float SpinYPush = 100;
	public float MaxTorque = 200;
	public float MaxBreakTorque = 100;
	public float MaxReverseSpeed = -50;
	public Vector3 CenterOfMass = new Vector3(0, -2.2f, 0.25f);
	public string FrontLeftWheel = "wheelFL";
	public string FrontRightWheel = "wheelFR";
	public string RearLeftWheel = "wheelRL";
	public string RearRightWheel = "wheelRR";

	// reference to all 4 wheel colliders of the Boat GameObject.
	private WheelCollider m_WheelFL;
	private WheelCollider m_WheelFR;
	private WheelCollider m_WheelRL;
	private WheelCollider m_WheelRR;
	
	private float m_MaxTorque;
	private float m_MaxBrakeTorque;
	private float m_CurrentSpeed;
	private float m_StopEngineTimer;
	private float m_StopEngineTime;
	private float m_TopSpeed;
	private bool m_Spinning;
	private bool m_Braked;
	private bool m_GoingUpHill;
	private bool m_EngineStopped;
	private bool m_EngineOn;
	
	// friction information for the wheels.
	private float m_SidewayFriction;
	private float m_ForwardFriction;
	private float m_SlipSidewayFriction;
	private float m_SlipForwardFriction;
	
	// turbo information.
	private bool m_IsTurbo;
	private float m_TurboTimer;
	
	private void Start () {
		
		SetEngineValues();
		GetWheelColliders();
		SetCenterOfMass();
		SetDriftValues();
		
		// balance script can start working now.
		GetComponent<BoatBalanceScript>().StartBalance();
	}
	
	/// <summary>
	/// Reset all the variables.
	/// </summary>
	private void SetEngineValues () {
		m_MaxTorque = MaxTorque;
		m_Braked = false;
		m_MaxBrakeTorque = MaxBreakTorque;
		m_CurrentSpeed = 0;
		m_Spinning = false;
		m_EngineOn = false;
		m_TopSpeed = 0;
		m_IsTurbo = false;
		m_TurboTimer = 0;
		m_GoingUpHill = false;
		m_StopEngineTimer = 0;
		m_StopEngineTime = 0;
		m_EngineStopped = false;
	}
	
	private void SetCenterOfMass () {
		Vector3 centerOfMass = rigidbody.centerOfMass;
		centerOfMass.y = CenterOfMass.y;
		centerOfMass.z = CenterOfMass.z;
		rigidbody.centerOfMass = centerOfMass;
	}
	
	/// <summary>
	/// Used to grab the Wheel Collider component of each wheel GameObject parented by this Boat GameObject.
	/// </summary>
	private void GetWheelColliders () {
		m_WheelFL = transform.Find(FrontLeftWheel).collider as WheelCollider;
		m_WheelFR = transform.Find(FrontRightWheel).collider as WheelCollider;
		m_WheelRL = transform.Find(RearLeftWheel).collider as WheelCollider;
		m_WheelRR = transform.Find(RearRightWheel).collider as WheelCollider;
	}
	
	/// <summary>
	/// Initialize friction values of the wheels.
	/// </summary>
	private void SetDriftValues() {
		
		// normal friction values are retrieved from the wheels.
		m_ForwardFriction = m_WheelRR.forwardFriction.stiffness;
		m_SidewayFriction = m_WheelRR.sidewaysFriction.stiffness;
		
		// slip friction values are edited by the designer in Unity Editor inspector.
		m_SlipForwardFriction = 2; // 0.04
		m_SlipSidewayFriction = 2; // 0.08
	}
	
	/// <summary>
	/// Main loop of the game that calls control of the boat when it is not spinning uncontrollably (i.e. tornado).
	/// </summary>
	private void FixedUpdate () {
		
		if (m_Spinning) return;
		
		Control();
	}
	
	/// <summary>
	/// The core of the script where torque, brake, steering is applied to control the boat.
	/// </summary>
	private void Control () {
		
		CalculateSpeed();
		ApplyTorque();
		ApplyBrakes();
		Steer();
		ApplyHandBrakes();
	}
	
	private void CalculateSpeed() {
		
		// calculate speed 2 * PI * Radius * RPM (cm / hour to km / hour conversion 60 / 1000)
		m_CurrentSpeed = 2 * Mathf.PI * m_WheelRL.radius * m_WheelRL.rpm * 60 / 1000;
		// round it.
		m_CurrentSpeed = Mathf.Round(m_CurrentSpeed);
	}
	
	/// <summary>
	/// Apply torque on the rear wheels!
	/// </summary>
	private void ApplyTorque()
	{
		// get the input on the vertical axis to control how fast player goes on reverse.
		float verticalAxis = Input.GetAxis("Vertical");
		
		// if the boat is not going with top speed or top negative reverse speed, apply torque.
		if (m_CurrentSpeed < m_TopSpeed && m_CurrentSpeed > MaxReverseSpeed && !m_Braked) {
			
			// no turbo and it's going uphill! 
			if (!m_IsTurbo && (verticalAxis < 0 || m_GoingUpHill)) {
				
				// go with maximum speed on reverse.
				if (m_GoingUpHill) verticalAxis = -1;
				
				// force negative torque - to prevent climbing the hill.
				m_WheelRR.motorTorque = (m_MaxTorque / 2) * verticalAxis;
				m_WheelRL.motorTorque = (m_MaxTorque / 2) * verticalAxis;
				
				m_GoingUpHill = false;
			}
			// power on the rear wheels!
			else {
				m_WheelRR.motorTorque = m_MaxTorque;
				m_WheelRL.motorTorque = m_MaxTorque;
			}
		}
		// otherwise stop generating torque on the wheels.
		else {
			m_WheelRR.motorTorque = 0;
			m_WheelRL.motorTorque = 0;
		}
	}
	
	/// <summary>
	/// Apply brakes to stop in place!
	/// </summary>
	private void ApplyBrakes() {
		
		// Apply brakes.
		if (!Input.GetButton("Vertical")) {
			m_WheelRR.brakeTorque = DecelerationSpeed;
			m_WheelRL.brakeTorque = DecelerationSpeed;
		}
		else {
			m_WheelRR.brakeTorque = 0;
			m_WheelRL.brakeTorque = 0;
		}
	}
	
	/// <summary>
	/// Apply steering on front wheels!
	/// </summary>
	private void Steer() {
		
		// calculate the steer angle.
		float speedFactor = m_CurrentSpeed / TurboSpeed;
		float currentSteerAngle = Mathf.Lerp(LowSpeedSteerAngle, HighSpeedSteerAngle, speedFactor);
		
		// mutiply the angle with player input.
		currentSteerAngle *= Input.GetAxis("Horizontal");
		
		// apply it to the front wheels.
		m_WheelFL.steerAngle = currentSteerAngle;
		m_WheelFR.steerAngle = currentSteerAngle;
	}
	
	/// <summary>
	/// Apply hand brakes freeze the rear wheels and change the friction of the boat!
	/// </summary>
	void ApplyHandBrakes () {
		
		// if pressing jump, braked is true.
		m_Braked = Input.GetKey(KeyCode.Space);
		
		if (m_Braked) {
			
			m_WheelRR.brakeTorque = m_MaxBrakeTorque;
			m_WheelRL.brakeTorque = m_MaxBrakeTorque;
			m_WheelRR.motorTorque = 0;
			m_WheelRL.motorTorque = 0;
			
			if (rigidbody.velocity.magnitude > 1) {
				SetSlip(m_ForwardFriction, m_SidewayFriction);
			}
			else {
				SetSlip(m_ForwardFriction, m_SidewayFriction);
			}
		}
		else {
			
			SetSlip(m_ForwardFriction, m_SidewayFriction);
		}
	}
	
	/// <summary>
	/// Loop of the script where the input of the player to start / stop engine 
	/// and / or turbo is retrieved and used.
	/// Also the boat is checked against the terrain hills to make sure it doesn't climb wrongly.
	/// </summary>
	void Update () {
		
		CheckUpHill();
		
		if (m_EngineStopped) {
			UpdateStopEngineFor();
		}
		
		// no need to go further is engine is stopped.
		if (m_Spinning || m_EngineStopped) 
			return;
		
		// Grab the input for starting / stopping engine and use it if it's active.
		if (Input.GetKeyDown(KeyCode.W)) {
			if (m_EngineOn) StopEngine();
			else StartEngine();
		}
		
		// Grab the input for starting turbo and use it if it's active and turbo is not on already.
		if (Input.GetKeyDown(KeyCode.E) && !m_IsTurbo) {
			Turbo(true);
		}
		
		UpdateTurbo();
	}
	
	/// <summary>
	/// Use to turn Turbo on or off.
	/// </summary>
	/// <param name="on">Flag indicating whether Turbo should be turned on.</param>
	public void Turbo (bool on) {
		
		// wait for first turbo to be done, then you can do again.
		if (on) {
			m_IsTurbo = true;
			m_TopSpeed = TurboSpeed;
			m_TurboTimer = 0;
		}
		else {
			m_IsTurbo = false;
			m_TopSpeed = EngineSpeed;
		}
	}
	
	public void StartEngine() {
	
		// not while Turbo is on.
		if (!m_IsTurbo)
		{
			m_TopSpeed = EngineSpeed;
			m_EngineOn = true;
		}
	}
	
	public void StopEngine() {
	
		// not while Turbo is on.
		if (!m_IsTurbo) {
			m_TopSpeed = MinimumSpeed;
			m_EngineOn = false;
		}
	}
	
	/// <summary>
	/// Update Turbo is used to remove the Turbo after the timer has ran out.
	/// </summary>
	private void UpdateTurbo() {
		
		if (m_IsTurbo) {
			m_TurboTimer += Time.deltaTime;
			
			if (m_TurboTimer > TurboTime) {
				Turbo(false);
			}
		}
	}
	
	/// <summary>
	/// Function used to check when boat is going into too steep terrain in order to prevent climbing too high.
	/// </summary>
	private void CheckUpHill () {
	
		// check the angle, if it's too high than set the flag to true and deactivate Turbo.
		if (transform.eulerAngles.x < HillAngle + 270 && transform.eulerAngles.x > 270) {
			m_GoingUpHill = true;
			
			// sorry turbo
			Turbo(false);
		}
	}
	
	/// <summary>
	/// Use to update the friction values of the rear wheels.
	/// </summary>
	private void SetSlip (float currentForwardFriction, float currentSidewayFriction) {
		
		WheelFrictionCurve wfc = m_WheelRR.forwardFriction;
		
		wfc.stiffness = currentForwardFriction;
		
		m_WheelRR.forwardFriction = wfc;
		m_WheelRL.forwardFriction = wfc;
		
		wfc.stiffness = currentSidewayFriction;
		
		m_WheelRR.sidewaysFriction = wfc;
		m_WheelRL.sidewaysFriction = wfc;
	}
	
	/// <summary>
	/// Use to stop the engine for an amount of time.
	/// </summary>
	/// <param name="seconds">The number (float) of seconds to stop the engine for.</param>
	public void StopEngineFor (float seconds) {
		
		// set the variables to control this functionality.
		m_StopEngineTimer = 0;
		m_StopEngineTime = seconds;
		m_EngineStopped = true;
		
		// and stop the engine.
		StopEngine();
	}
	
	/// <summary>
	/// Use to remove the StopEngine call before the timer has ran out.
	/// </summary>
	public void RemoveStopEngineFor () {
		
		m_StopEngineTimer = 0;
		m_EngineStopped = false;
		
		StartEngine();
	}
	
	/// <summary>
	/// Used to update the StopEngine functionality.
	/// </summary>
	private void UpdateStopEngineFor () {
		
		m_StopEngineTimer += Time.deltaTime;
		
		if (m_StopEngineTimer > m_StopEngineTime) {
			
			RemoveStopEngineFor();
		}
	}
	
	/// <summary>
	/// Use to spin the boat uncontrollably (rotation / movement with freezing player controls).
	/// </summary>
	public void SpinIt (int numberOfSpins = 1) {
		m_Spinning = true;
		iTween.RotateBy(gameObject, new Vector3(0, Random.value < .5 ? -numberOfSpins : numberOfSpins, 0), SpinImpactTime);
		iTween.MoveBy(gameObject, new Vector3(0, SpinYPush, 0), SpinImpactTime);
		
		StartCoroutine(DisableSpin());
	}
	
	private IEnumerator DisableSpin () {
		
		yield return new WaitForSeconds(SpinImpactTime / 2);
		
		m_Spinning = false;
	}
	
	public void SetStartingSpeed(){
		m_TopSpeed = MinimumSpeed;
	}
	
	public void FreezeBoat () {
		StopEngineFor(float.MaxValue);
	}
	
	public void Respawn (Vector3 pos) {
		transform.position = new Vector3(pos.x, pos.y + 30f, pos.z);
	}
	
	public float GetCurrentSpeed () {
		return m_CurrentSpeed;
	}
	
	public WheelCollider GetFrontLeftWheel () {
		return m_WheelFL;
	}
	
	public WheelCollider GetFrontRightWheel () {
		return m_WheelFR;
	}
	
	public WheelCollider GetRearLeftWheel () {
		return m_WheelRL;
	}
	
	public WheelCollider GetRearRightWheel () {
		return m_WheelRR;
	}
	
	public bool IsEngineOn () {
		return m_EngineOn;
	}
	
	public bool HasMinimumSpeed () {
		return m_TopSpeed > 0;
	}
}
