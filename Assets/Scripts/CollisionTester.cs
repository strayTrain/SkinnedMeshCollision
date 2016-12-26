using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CollisionTester : MonoBehaviour 
{
	public enum CollisionTestType {Raycast, SphereCast}

	private List<SkinnedMeshHit> hits = new List<SkinnedMeshHit>(32);

	public CollisionTestType TestType;

	public float RaycastLength = 10;
	public float SphereCastRadius = 1;

	public Color CastColourHit = Color.green;
	public Color CastColourMiss = Color.red;
	public Color CastHitNormalsColour = Color.yellow;

	private void DrawHitNormals()
	{
		Gizmos.color = CastHitNormalsColour;
		for (int i = 0; i < hits.Count; i++)
		{
			Gizmos.DrawRay(hits[i].point, hits[i].normal);	
		}
	}

	private void OnDrawGizmosSelected()
	{
		hits.Clear();
		SkinnedMeshCollider[] skinnedMeshColliders = GameObject.FindObjectsOfType<SkinnedMeshCollider>();

		if (TestType == CollisionTestType.Raycast)
		{

			if (SkinnedMeshCollisionController.RaycastAll(new Ray(transform.position, transform.forward), ref hits, skinnedMeshColliders,RaycastLength))
			{
				DrawHitNormals();
				Gizmos.color = CastColourHit;
			}
			else
			{
				Gizmos.color = CastColourMiss;
			}

			Gizmos.DrawLine(transform.position, transform.position + transform.forward * RaycastLength);
		}
		else
		{
			if (SkinnedMeshCollisionController.SphereCastAll(transform.position, SphereCastRadius, ref hits, skinnedMeshColliders))
			{
				DrawHitNormals();

				Gizmos.color = CastColourHit;
			}
			else
			{
				Gizmos.color = CastColourMiss;
			}

			Gizmos.DrawWireSphere(transform.position, SphereCastRadius);	
		}
	}
}
