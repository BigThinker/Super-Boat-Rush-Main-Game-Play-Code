using UnityEngine;
using System.Collections;

/// <summary>
/// Script used to make the dragon bones follow the head of the dragon (game object where this script should be attached),
/// and be always on top of the terrain.
/// </summary>
public class DragonBoneController : MonoBehaviour {
	
	// Variables edited in Unity Editor.
	public int NumberOfBones = 15;
	public string BoneKeyword = "bone";
	
	private GameObject[] m_Bones;
	
	void Start () {
		
		m_Bones = new GameObject[NumberOfBones];
		
		// the head.
		m_Bones[0] = gameObject;
		
		// the bones.
		for (int i = 1; i < NumberOfBones; i++) {
			m_Bones[i] = transform.parent.Find(BoneKeyword + (i + 1)).gameObject;
		}
	}
	
	/// <summary>
	/// Late Update called after the position of the head (GameObject) has been processed by SplineController attached to it.
	/// </summary>
	void LateUpdate () {
	
		RaycastHit hit;
		// hit a ray below to find the terrain.
		Physics.Raycast(transform.position, Vector3.down, out hit);
		
		// if it is not the terrain, return.
		if (hit.collider.name != "Terrain") return;
		
		// otherwise, bring the head to the point of the terrain.
		transform.position = hit.point;
		
		for (int i = 1; i < NumberOfBones; i++) {
			// make current bone look at the bone at the front (accessed by i-1)
			m_Bones[i].transform.LookAt(m_Bones[i - 1].transform);
			
			// advance and keep the same distance between all the bones.
			Vector3 targetPos = m_Bones[i].transform.TransformPoint (new Vector3 (0, 0, 15f));
			m_Bones[i].transform.position += (m_Bones[i - 1].transform.position - targetPos);
		}
	}
}
