using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SkinnedMeshCollisionController : Singleton<SkinnedMeshCollisionController> 
{
	private List<SkinnedMeshCollider> skinnedMeshColliders = new List<SkinnedMeshCollider>(32);

	public void RegisterSkinnedMeshCollider(SkinnedMeshCollider collider)
	{
		if (!skinnedMeshColliders.Contains(collider))
		{
			skinnedMeshColliders.Add(collider);
		}
	}

	public void DeregisterSkinnedMeshCollider(SkinnedMeshCollider collider)
	{
		skinnedMeshColliders.Remove(collider);
	}

	public bool RaycastAll(Ray ray, ref List<SkinnedMeshHit> hits, float distance = Mathf.Infinity)
	{
		List<SkinnedMeshHit> tmpHits = new List<SkinnedMeshHit>(32);

		bool wasHitDetected = false;

		for (int i = 0; i < skinnedMeshColliders.Count; i++)
		{
			tmpHits.Clear();

			if (skinnedMeshColliders[i].RaycastAll(ray, ref tmpHits, distance))
			{
				wasHitDetected = true;
				hits.AddRange(tmpHits);
			}	
		}

		SkinnedMeshCollisionUtilities.SortHitsByDistance(ref hits, ray.origin);

		return wasHitDetected;
	}

	public static bool RaycastAll(Ray ray, ref List<SkinnedMeshHit> hits,  SkinnedMeshCollider[] collidersToCheckAgainst, float distance = Mathf.Infinity)
	{
		List<SkinnedMeshHit> tmpHits = new List<SkinnedMeshHit>(32);

		bool wasHitDetected = false;

		for (int i = 0; i < collidersToCheckAgainst.Length; i++)
		{
			tmpHits.Clear();

			if (collidersToCheckAgainst[i].RaycastAll(ray, ref tmpHits, distance))
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
		bool wasHitSuccessful = false;

		for (int i = 0; i < skinnedMeshColliders.Count; i++)
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

	public static bool SphereCastAll(Vector3 origin, float radius, ref List<SkinnedMeshHit> hits, SkinnedMeshCollider[] collidersToCheckAgainst)
	{
		List<SkinnedMeshHit> tmpHits = new List<SkinnedMeshHit>(32);
		bool wasHitSuccessful = false;

		for (int i = 0; i < collidersToCheckAgainst.Length; i++)
		{
			tmpHits.Clear();

			if (collidersToCheckAgainst[i].SphereCastAll(origin, radius, ref tmpHits))
			{
				wasHitSuccessful = true;
				hits.AddRange(tmpHits);
			}
		}

		return wasHitSuccessful;
	}
}
