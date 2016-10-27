using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum UpdateType {Update, Fixed, Late, Manual}
public delegate void OnCollisionEnter();
public delegate void OnCollisionExit();

public class SkinnedMeshCollisionListener : MonoBehaviour 
{
	private SkinnedMeshCollider[] skinnedMeshColliders;
	private List<SkinnedMeshHit> hits;

	public float Radius = 1;
	public Vector3 Offset;
	public UpdateType UpdatesOn = UpdateType.Fixed;

	private void Awake()
	{
		skinnedMeshColliders = GameObject.FindObjectsOfType<SkinnedMeshCollider>();	
		hits = new List<SkinnedMeshHit>(30);
	}

	private void Update()
	{
		if (UpdatesOn == UpdateType.Update)
		{
			CheckForCollisions();
		}
	}

	private void FixedUpdate()
	{
		if (UpdatesOn == UpdateType.Fixed)
		{
			CheckForCollisions();
		}
	}

	private void LateUpdate()
	{
		if (UpdatesOn == UpdateType.Late)
		{
			CheckForCollisions();
		}
	}

	private void CheckForCollisions()
	{
		Vector3 sphereCastPosition = transform.position + Offset;

		for (int i = 0; i < skinnedMeshColliders.Length; i++)
		{
			if (skinnedMeshColliders[i].SphereCastAll(sphereCastPosition, Radius, ref hits))
			{
				Debug.Log("Collision!");
			}
		}
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(transform.position + Offset, Radius);
	}
}
