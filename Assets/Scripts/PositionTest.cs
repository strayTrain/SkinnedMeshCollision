using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PositionTest : MonoBehaviour 
{
	public SkinnedMeshCollider sm;
	public float Radius = 1;

	private List<SkinnedMeshHit> hits = new List<SkinnedMeshHit>();
	private Vector3 startPos;
	private Vector3 startRotation;

	private bool isStuck = false;

	private void Awake()
	{
		startPos = transform.position;
		startRotation = transform.eulerAngles;
	}

	private void FixedUpdate()
	{
		if (Input.GetKeyDown(KeyCode.R))
		{
			ResetPos();
		}

		if (Input.GetKeyDown(KeyCode.Space))
		{
			SetPos();
		}

		if (isStuck && Input.GetKey(KeyCode.W))
		{
			transform.localPosition += new Vector3(0, 1 * Time.deltaTime, 0);
		}

		if (isStuck && Input.GetKey(KeyCode.A))
		{
			transform.position -= transform.right * 1 * Time.deltaTime;
			SetPos();
		}

		if (isStuck && Input.GetKey(KeyCode.D))
		{
			transform.position += transform.right * 1 * Time.deltaTime;
			SetPos();
		}
	}

	private void ResetPos()
	{
		transform.parent = null;
		transform.position = startPos;
		transform.eulerAngles = startRotation;	
		isStuck = false;
	}

	private void SetPos()
	{
		if (sm != null)
		{
			if (sm.RaycastAll(new Ray(transform.position, transform.forward), ref hits))
			{
				//if (!isStuck)
				{
					Vector3 newPos = hits[0].point + (transform.position - hits[0].point).normalized * Radius;
					newPos.y = transform.position.y;
					transform.position = newPos;
				}

				//transform.rotation = Quaternion.LookRotation(-hits[0].normal, Vector3.up);
				//transform.forward = -hits[0].normal;
				transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(-hits[0].normal), Time.fixedDeltaTime);

				transform.parent = hits[0].bone;
				isStuck = true;
			}
		}	
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.DrawLine(transform.position, transform.position + (transform.forward) * 100);

		Gizmos.color = Color.red;

		if (sm != null)
		{
			if (sm.RaycastAll(new Ray(transform.position, transform.forward), ref hits))
			{
				for (int i = 0; i < hits.Count; i++)
				{
					Gizmos.DrawSphere(hits[i].point, 0.2f);	
				}
			}
		}

		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(transform.position, Radius);

		Debug.DrawRay(transform.position, transform.up, Color.blue);
		Debug.DrawRay(transform.position, transform.right, Color.blue);
	}
}
