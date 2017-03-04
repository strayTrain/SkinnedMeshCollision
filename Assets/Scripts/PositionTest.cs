using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PositionTest : MonoBehaviour
{
	private Vector3 startPos;
	private Vector3 startRot;
	private bool isStuckToMesh = false;
	private Vector3 targetPosition;
	private Vector3 targetNormal;
	private int currentTriangleIndex;

	private List<SkinnedMeshHit> hits;

	public float SphereRadius = 1;
	public float ClimbingSpeed = 5;

	private void Start()
	{
		startPos = transform.position;
		startRot = transform.eulerAngles;
		hits = new List<SkinnedMeshHit>(32);
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			if (isStuckToMesh)
			{
				transform.position = startPos;
				transform.eulerAngles = startRot;
				isStuckToMesh = false;
			}
			else
			{
				if (SkinnedMeshCollisionController.Instance.RaycastAll(new Ray(transform.position, transform.forward), ref hits))
				{
					targetPosition = hits[0].point;
					targetNormal = -hits[0].normal;
					currentTriangleIndex = hits[0].triangleIndex;

					isStuckToMesh = true;
				}
			}
		}

		if (isStuckToMesh)
		{
			transform.position = hits[0].normal * SphereRadius + hits[0].skinnedMeshCollider.BarycentricCoordinateToWorldPos(hits[0].triangleIndex, hits[0].barycentricCoordinate);
			transform.forward = -hits[0].skinnedMeshCollider.GetWorldSpaceTriangleNormal(hits[0].triangleIndex);

			ProcessPlayerInput();
		}
	}

	private void CheckForCollision(Vector3 position)
	{
		if (SkinnedMeshCollisionController.Instance.RaycastAll(new Ray(position, transform.forward), ref hits))
		{
			if (hits[0].triangleIndex == currentTriangleIndex)
			{
				//Debug.Log("STILL IN SAME TRIANGLE");
			}
			else
			{
				//Debug.Log("NOW IN DIFFERENT TRIANGLE");
				currentTriangleIndex = hits[0].triangleIndex;
			}
		}	

	}

	private void ProcessPlayerInput()
	{
		if (Input.GetKey(KeyCode.W))
		{
			Vector3 targetPosition = (transform.position + hits[0].skinnedMeshCollider.GetWorldSpaceTriangleNormal(hits[0].triangleIndex) * 5) + transform.up * ClimbingSpeed * Time.deltaTime;
			CheckForCollision(targetPosition);
		}	

		if (Input.GetKey(KeyCode.S))
		{
			Vector3 targetPosition = transform.position - transform.up * ClimbingSpeed * Time.deltaTime;
			CheckForCollision(targetPosition);
		}
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.DrawWireSphere(transform.position, SphereRadius);
	}
}
