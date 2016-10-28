using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SkinnedMeshCollider : MonoBehaviour
{
	[System.Serializable]
	private class VertexWeight
	{
		public int Index;
		public Vector3 LocalPosition;
		public float Weight;

		public VertexWeight(int indexInVertexArray, Vector3 localPosition, float weight)
		{
			Index = indexInVertexArray;
			LocalPosition = localPosition;
			Weight = weight;
		}
	}

	[System.Serializable]
	private class Bone
	{
		public Transform BoneTransform;
		public List<VertexWeight> Weights;

		public void CalculateBoundingSphere(Vector3[] vertexArray, ref Vector3 sphereCentre, ref float radius)
		{
			sphereCentre = Vector3.zero;
			radius = 0;

			// Find the center of the sphere
			for (int i = 0; i < Weights.Count; i++)
			{
				//sphereCentre += vertexArray[Weights[i].Index];

				Gizmos.DrawWireSphere(BoneTransform.localToWorldMatrix.MultiplyPoint3x4(vertexArray[Weights[i].Index]), 0.01f);
			}	

			// Calculate the radius
			/*float currentDistance = 0;
			for (int i = 0; i < Weights.Count; i++)
			{
				currentDistance = Vector3.SqrMagnitude(vertexArray[Weights[i].Index] - sphereCentre);

				if (currentDistance > radius)
				{
					radius = currentDistance;
				}
			}*/
				
		}

		public Bone()
		{
			Weights = new List<VertexWeight>();
		}
	}

	// Used to update the collision mesh
	private Vector3[] tempVertices;
	private Vector3[] vertices;
	private int[] triangles;
	private Bone[] bones;

	private SkinnedMeshRenderer skinnedMesh;

	public Vector3[] Vertices {get {return vertices;}}
	public int[] Triangles {get {return triangles;}}

	public void ExtractMeshData()
	{
		if (skinnedMesh == null)
		{
			skinnedMesh = GetComponent<SkinnedMeshRenderer>();
		}

		vertices = skinnedMesh.sharedMesh.vertices;
		triangles = skinnedMesh.sharedMesh.triangles;

		tempVertices = new Vector3[vertices.Length];

		Vector3[] cachedVertices = skinnedMesh.sharedMesh.vertices;
		Matrix4x4[] cachedBindposes = skinnedMesh.sharedMesh.bindposes;
		BoneWeight[] cachedBoneWeights = skinnedMesh.sharedMesh.boneWeights;
       
		// Make a bone for each bone in the skinned mesh
		bones = new Bone[skinnedMesh.bones.Length];
		for (int i = 0; i < skinnedMesh.bones.Length; i++)
		{
			bones[i] = new Bone();
			bones[i].BoneTransform = skinnedMesh.bones[i];
		}

		// Create a vertex weight list for each bone, ready for quick calculation during an update...
		for (int i = 0; i < cachedVertices.Length; i++)
		{
			BoneWeight bw = cachedBoneWeights[i];

			// You can have up to 4 bones affecting a vertex
			if (bw.weight0 != 0.0f)
			{
				Vector3 localPt = cachedBindposes[bw.boneIndex0].MultiplyPoint3x4(cachedVertices[i]);
				bones[bw.boneIndex0].Weights.Add(new VertexWeight(i, localPt, bw.weight0));
			}
			if (bw.weight1 != 0.0f)
			{
				Vector3 localPt = cachedBindposes[bw.boneIndex1].MultiplyPoint3x4(cachedVertices[i]);
				bones[bw.boneIndex1].Weights.Add(new VertexWeight(i, localPt, bw.weight1));
			}
			if (bw.weight2 != 0.0f)
			{
				Vector3 localPt = cachedBindposes[bw.boneIndex2].MultiplyPoint3x4(cachedVertices[i]);
				bones[bw.boneIndex2].Weights.Add(new VertexWeight(i, localPt, bw.weight2));
			}
			if (bw.weight3 != 0.0f)
			{
				Vector3 localPt = cachedBindposes[bw.boneIndex3].MultiplyPoint3x4(cachedVertices[i]);
				bones[bw.boneIndex3].Weights.Add(new VertexWeight(i, localPt, bw.weight3));
			}
		}
	}

	public void UpdateCollisionMesh()
	{       
		// Now get the local positions of all weighted indices
		int bonesLength = bones.Length;
		int boneWeightCount = 0;

		VertexWeight currentVertexWeight;

		// Clear out the temp vertices array
		for (int i = 0; i < tempVertices.Length; ++i)
		{
			tempVertices[i] = Vector3.zero;
		}

		for (int i = 0; i < bonesLength; ++i)
		{
			boneWeightCount = bones[i].Weights.Count;
			Matrix4x4 currentLocalToWorldMatrix = bones[i].BoneTransform.localToWorldMatrix;

			for (int j = 0; j < boneWeightCount; ++j)
			{
				currentVertexWeight = bones[i].Weights[j];
				tempVertices[currentVertexWeight.Index] += currentLocalToWorldMatrix.MultiplyPoint3x4(currentVertexWeight.LocalPosition) * currentVertexWeight.Weight;
			}
		}

		// Now convert each point into local coordinates of this object.
		Matrix4x4 worldToLocalMatrix = transform.worldToLocalMatrix;
		for (int i = 0; i < tempVertices.Length; ++i)
		{
			tempVertices[i] = worldToLocalMatrix.MultiplyPoint3x4(tempVertices[i]);
		}
 
		vertices = tempVertices;
	}

	public bool RaycastAll(Ray ray, ref List<SkinnedMeshHit> hits)
	{
		SkinnedMeshHit hit;
		bool wasRaycastSuccessful = false;
		hits.Clear();

		for (int i = 0; i < triangles.Length; i += 3)
		{
			// The 3 verts that make up this triangle
			Vector3 a, b, c;

			// Get them in world space
			a = transform.TransformPoint(vertices[triangles[i]]);
			b = transform.TransformPoint(vertices[triangles[i + 1]]);
			c = transform.TransformPoint(vertices[triangles[i + 2]]);

			if (SkinnedMeshCollisionUtilities.TriangleRayIntersection(i, a, b, c, ray, out hit))
			{
				hits.Add(hit);
				wasRaycastSuccessful = true;
			}
		}

		return wasRaycastSuccessful;
	}

	public bool SphereCastAll(Vector3 origin, float radius, ref List<SkinnedMeshHit> hits)
	{
		SkinnedMeshHit hit;
		bool sphereCastSuccessfull = false;
		hits.Clear();

		for (int i = 0; i < triangles.Length; i += 3)
		{
			// The 3 verts that make up this triangle
			Vector3 a, b, c;

			a = transform.TransformPoint(vertices[triangles[i]]);
			b = transform.TransformPoint(vertices[triangles[i + 1]]);
			c = transform.TransformPoint(vertices[triangles[i + 2]]);

			if (SkinnedMeshCollisionUtilities.TriangleSphereIntersection(i, a, b, c, origin, radius, out hit))
			{
				hits.Add(hit);
				sphereCastSuccessfull = true;
			}
		}	

		return sphereCastSuccessfull;
	}

	private void Start()
	{
		ExtractMeshData();
	}

	private void LateUpdate()
	{
		UpdateCollisionMesh();
	}

	private void OnDrawGizmosSelected()
	{
		if (bones == null || skinnedMesh == null)
		{
			ExtractMeshData();
		}

		/*for (int i = 0; i < bones.Length; i++)
		{
			float radius = 0;
			Vector3 sphereCenter = Vector3.zero;

			bones[i].CalculateBoundingSphere(vertices, ref sphereCenter, ref radius);

			//Gizmos.DrawWireSphere(sphereCenter, radius);
		}*/

		for (int i = 0; i < vertices.Length; i++)
		{
			Gizmos.DrawWireSphere(transform.localToWorldMatrix.MultiplyPoint3x4(vertices[i]), 0.1f);
		}
	}
}