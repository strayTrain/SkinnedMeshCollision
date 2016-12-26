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

		if (TestType == CollisionTestType.Raycast)
		{
			if (RaycastAll(new Ray(transform.position, transform.forward), ref hits, RaycastLength))
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
			if (SphereCastAll(transform.position, SphereCastRadius, ref hits))
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

	private bool RaycastAll(Ray ray, ref List<SkinnedMeshHit> hits, float distance = Mathf.Infinity)
	{
		List<SkinnedMeshHit> tmpHits = new List<SkinnedMeshHit>(32);
		SkinnedMeshCollider[] skinnedMeshColliders = GameObject.FindObjectsOfType<SkinnedMeshCollider>();

		bool wasHitDetected = false;

		for (int i = 0; i < skinnedMeshColliders.Length; i++)
		{
			if (skinnedMeshColliders[i].RaycastAll(ray, ref tmpHits, distance))
			{
				wasHitDetected = true;
				hits.AddRange(tmpHits);
			}	
		}

		SkinnedMeshCollisionUtilities.SortHitsByDistance(ref hits, ray.origin);

		return wasHitDetected;
	}

	public bool SphereCastAll(Vector3 origin, float radius, ref List<SkinnedMeshHit> hits)
	{
		List<SkinnedMeshHit> tmpHits = new List<SkinnedMeshHit>(32);
		SkinnedMeshCollider[] skinnedMeshColliders = GameObject.FindObjectsOfType<SkinnedMeshCollider>();

		bool wasHitSuccessful = false;

		for (int i = 0; i < skinnedMeshColliders.Length; i++)
		{
			tmpHits.Clear();

			if (skinnedMeshColliders[i].SphereCastAll(origin, radius, ref tmpHits))
			{
				wasHitSuccessful = true;
				hits.AddRange(tmpHits);
			}
		}

		return wasHitSuccessful;
	}
}
