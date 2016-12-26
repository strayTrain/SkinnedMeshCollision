using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PositionTest : MonoBehaviour
{
	public SkinnedMeshCollider sm;
	public float Radius = 1;
	public float ClimbingSpeed = 1;
	public Vector3 RaycastOffset;

	private List<SkinnedMeshHit> hits = new List<SkinnedMeshHit>();
	private Vector3 startPos;
	private Vector3 startRotation;

	private bool isStuckToSkinnedMesh = false;

	// When we're stuck to a skinned mesh we want to track this position
	private SkinnedMeshHit currentHitPoint;
	private SkinnedMeshHit previousHitPoint;

	private void Awake()
	{
		startPos = transform.position;
		startRotation = transform.eulerAngles;
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.R))
		{
			ResetPos();
		}

		if (Input.GetKeyDown(KeyCode.Space))
		{
			SetPos();
		}
	}


	// Update our position once the skinned mesh is done updating
	private void FixedUpdate()
	{
		if (Input.GetKey(KeyCode.D))
		{
			Ray ray = GetRaycastRay();
			ray.origin = ray.origin + transform.right * Time.deltaTime;

			UpdateCurrentHitPoint(ray, transform.right);
		}

		if (Input.GetKey(KeyCode.A))
		{
			Ray ray = GetRaycastRay();
			ray.origin = ray.origin - transform.right * Time.deltaTime;

			UpdateCurrentHitPoint(ray, -transform.right);
		}

		if (Input.GetKey(KeyCode.W))
		{
			Ray ray = GetRaycastRay();
			ray.origin = ray.origin + transform.up * Time.deltaTime;

			UpdateCurrentHitPoint(ray, transform.up);
		}

		if (Input.GetKey(KeyCode.S))
		{
			Ray ray = GetRaycastRay();
			ray.origin = ray.origin - transform.up * Time.deltaTime;

			UpdateCurrentHitPoint(ray, -transform.up);
		}

		if (isStuckToSkinnedMesh)
		{
			if (Vector3.Dot(previousHitPoint.normal, currentHitPoint.normal) > 0.9f)
			{
				MoveTowardsNewLocation();
			}
			else
			{
				SnapToNewLocation();
			}
		}
	}

	private void UpdateCurrentHitPoint(Ray ray, Vector3 movementDirection)
	{
		if (sm.RaycastAll(ray, ref hits))
		{
			previousHitPoint = currentHitPoint;
			currentHitPoint = hits[0];

			// If we've shifted over to another triangle, try the raycast again with a better normal for a smoother transition
			if (previousHitPoint.triangleIndex != currentHitPoint.triangleIndex)
			{
				//ray.origin
				ray.direction = -currentHitPoint.normal;
				//UpdateCurrentHitPoint(ray);
			}
			//Debug.Log(Vector3.Dot(previousHitPoint.normal, currentHitPoint.normal));
		}
	}

	private void MoveTowardsNewLocation()
	{
		Vector3 targetPoint = currentHitPoint.skinnedMeshCollider.BarycentricCoordinateToWorldPos(currentHitPoint.triangleIndex, 
			                      currentHitPoint.barycentricCoordinate) + currentHitPoint.normal * Radius; 

		transform.position = Vector3.MoveTowards(transform.position, targetPoint, Time.deltaTime * 15);	
		transform.forward = Vector3.MoveTowards(transform.forward, -currentHitPoint.normal, Time.deltaTime * 5);	
	}

	private void SnapToNewLocation()
	{
		transform.position = currentHitPoint.skinnedMeshCollider.BarycentricCoordinateToWorldPos(currentHitPoint.triangleIndex, 
			currentHitPoint.barycentricCoordinate)
		+ currentHitPoint.normal * Radius; 

		transform.forward = -currentHitPoint.normal;	
	}

	private Ray GetRaycastRay()
	{
		return new Ray(transform.position + transform.forward * -1, transform.forward);
	}

	private void ResetPos()
	{
		transform.parent = null;
		transform.position = startPos;
		transform.eulerAngles = startRotation;	
		isStuckToSkinnedMesh = false;
	}

	private void SetPos()
	{
		if (sm != null)
		{
			if (sm.RaycastAll(GetRaycastRay(), ref hits))
			{
				isStuckToSkinnedMesh = true;
				currentHitPoint = hits[0];
				previousHitPoint = hits[0];

				SnapToNewLocation();
			}
		}	
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.yellow;

		Gizmos.DrawLine(transform.position + transform.forward * -1, transform.position + (transform.forward) * 100);

		Gizmos.color = Color.red;

		if (sm != null)
		{
			if (sm.RaycastAll(GetRaycastRay(), ref hits))
			{
				for (int i = 0; i < hits.Count; i++)
				{
					SkinnedMeshCollider currentCollider = hits[i].skinnedMeshCollider;

					Gizmos.DrawSphere(currentCollider.BarycentricCoordinateToWorldPos(hits[i].triangleIndex, hits[i].barycentricCoordinate), 0.05f);

					//Debug.Log(hits[i].barycentricCoordinate);
				}
			}
		}

		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(transform.position, Radius);

		Debug.DrawRay(transform.position, transform.up, Color.blue);
		Debug.DrawRay(transform.position, transform.right, Color.blue);
	}
}
