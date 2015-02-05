using UnityEngine;
using System.Collections;

public class CameraScript : MonoBehaviour
{
	// Variables edited in Unity Editor.
	// the target is needed.
	public Transform target;
	// properties to tweak camera positioning / rotation. 
	public float TargetHeight = 6f;
	public float Distance = 15f;
	public float MaxDistance = 20;
	public float MinDistance = .6f;
	public float XSpeed = 250.0f;
	public float YSpeed = 120.0f;
	public float StartRotateTime = 3f;
	public int YMinLimit = -20;
	public int YMaxLimit = 40;
	public int ZoomRate = 10;
	public float RotationDampening = 3.0f;
	public float ZoomDampening = 5.0f;
	public float DefaultFOV = 60;
	
	private float m_X;
	private float m_Y;
	private float m_CurrentDistance;
	private float m_DesiredDistance;
	private float m_CorrectedDistance;
	private float m_StartRotateTimer;
	
	/// <summary>
	/// Set all the initial variables.
	/// </summary>
	private void Start ()
	{
		Vector3 angles = transform.eulerAngles;
		m_X = target.eulerAngles.y;
		m_Y = angles.y;
		
		m_CurrentDistance = Distance;
		m_DesiredDistance = Distance;
		m_CorrectedDistance = Distance;
	}
	
	/// <summary>
	/// Camera logic on LateUpdate to only update after all character movement logic has been handled.
	/// </summary>
	private void LateUpdate ()
	{
		// Don't do anything if target is not defined
		if (!target) return;
		
		// If either mouse buttons are down, let the mouse govern camera position
		if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
		{
			m_X += Input.GetAxis("Mouse X") * XSpeed * 0.02f; // horizontal
			m_Y -= Input.GetAxis("Mouse Y") * YSpeed * 0.02f; // vertical
		}
		// otherwise, ease behind the target if any of the directional keys are pressed
		else
		{
			float targetRotationAngle = target.eulerAngles.y;
			float currentRotationAngle = transform.eulerAngles.y;
			m_X = Mathf.LerpAngle(currentRotationAngle, targetRotationAngle, RotationDampening * Time.deltaTime);
		}
		
		m_Y = ClampAngle(m_Y, YMinLimit, YMaxLimit);
		
		// set camera rotation
		Quaternion rotation = Quaternion.Euler(20, m_X, 0);
		
		// calculate the desired distance
		m_DesiredDistance = Mathf.Clamp(m_DesiredDistance, MinDistance, MaxDistance);
		m_CorrectedDistance = m_DesiredDistance;
		
		// calculate desired camera position
		Vector3 position = target.position - (rotation * Vector3.forward * m_DesiredDistance + new Vector3(0, -TargetHeight, 0));
		
		// For smoothing, lerp distance only if either distance wasn't corrected, or m_CorrectedDistance is more than m_CurrentDistance
		m_CurrentDistance = m_CorrectedDistance > m_CurrentDistance ? 
				Mathf.Lerp(m_CurrentDistance, m_CorrectedDistance, Time.deltaTime * ZoomDampening) : 
				m_CorrectedDistance;
		
		// recalculate position based on the new m_CurrentDistance
		position = target.position - (rotation * Vector3.forward * m_CurrentDistance + new Vector3(0, -TargetHeight, 0));
		
		// finally set both correct position and rotation.
		transform.rotation = rotation;
		transform.position = position;
	}
	
	/// <summary>
	/// Inside FixedUpdate, the Field of View of the camera is updated based on target's acceleration.
	/// </summary>
	private void FixedUpdate () {
		
		// Don't do anything if target is not defined
		if (!target) return;
	
		float acceleration = target.rigidbody.velocity.magnitude;
		camera.fieldOfView = DefaultFOV + acceleration * ZoomRate * Time.deltaTime;
	}
	
	private static float ClampAngle(float angle, float min, float max)
	{
		if (angle < -360)
			angle += 360;
		if (angle > 360)
			angle -= 360;
		return Mathf.Clamp(angle, min, max);
	}
}

