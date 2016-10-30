using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CharacterTest : MonoBehaviour 
{
	public float MovementSpeed = 5;

	[Header("Collision Detection")]
	public float Radius = 1;

	private Vector3 movementVector = Vector3.zero;
	private SkinnedMeshCollider[] skinnedMeshColliders;
	private List<SkinnedMeshHit> hits;

	private bool CheckForCollisions(Vector3 sphereCastPosition, SkinnedMeshCollider skinnedMeshCollider)
	{
		return skinnedMeshCollider.SphereCastAll(sphereCastPosition, Radius, ref hits);
	}

	private void Awake()
	{
		skinnedMeshColliders = GameObject.FindObjectsOfType<SkinnedMeshCollider>();	
		hits = new List<SkinnedMeshHit>(30);
	}

	private void Update()
	{
		movementVector = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));	
	}

	private void FixedUpdate()
	{
		Vector3 targetMovementPosition = transform.position + movementVector * MovementSpeed * Time.fixedDeltaTime;

		//if (!skinnedMeshColliders[0].RaycastAll(new Ray(transform.position, movementVector.normalized), ref hits, Radius))
		if (CheckForCollisions(targetMovementPosition, skinnedMeshColliders[0]) == false)
		{
			transform.position = targetMovementPosition;
		}
		else
		{
			lastHitPos.Clear();

			for (int i = 0; i < hits.Count; i++)
			{
				lastHitPos.Add(hits[i].point);
			}
		}
	}

	List<Vector3> lastHitPos = new List<Vector3>();

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		for (int i = 0; i < lastHitPos.Count; i++)
		{
			Gizmos.DrawSphere(lastHitPos[i], 0.05f);
		}

		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(transform.position + movementVector * MovementSpeed * Time.fixedDeltaTime, Radius);
	}
}
