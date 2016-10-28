using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SkinnedMeshCollider : MonoBehaviour
{
	[System.Serializable]
	private class VertexWeight
	{
		public int Index;

		// The triangles that this vertex is in
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

		private List<int> uniqueVertexIndices;
		public List<int> UniqueVertexIndices { get {return uniqueVertexIndices;} }

		private List<int> triangleIndices;
		public List<int> TriangleIndices {get {return triangleIndices;} }

		public void CalculateBoundingSphere(Matrix4x4 localToWorldMatrix, Vector3[] vertexArray, ref Vector3 sphereCentre, ref float radius)
		{
			//sphereCentre = Vector3.zero;
			radius = 0;

			float x = 0, y = 0, z = 0;

			int uniqueVertexIndicesCount = uniqueVertexIndices.Count;

			// Start by finding the center of the sphere in local space
			for (int i = 0; i < uniqueVertexIndicesCount; i++)
			{
				x += vertexArray[uniqueVertexIndices[i]].x;
				y += vertexArray[uniqueVertexIndices[i]].y;
				z += vertexArray[uniqueVertexIndices[i]].z;
			}

			sphereCentre = new Vector3(x/uniqueVertexIndicesCount, y/uniqueVertexIndicesCount, z/uniqueVertexIndicesCount);

			Vector3 longestDistance = Vector3.zero;
			for (int i = 0; i < uniqueVertexIndices.Count; i++)
			{
				Vector3 currentDistance = (vertexArray[uniqueVertexIndices[i]] - sphereCentre);

				if (currentDistance.sqrMagnitude > longestDistance.sqrMagnitude)
				{
					longestDistance = currentDistance;
				}
			}

			sphereCentre = localToWorldMatrix.MultiplyPoint3x4(sphereCentre);

			longestDistance = new Vector3(longestDistance.x * BoneTransform.lossyScale.x, longestDistance.y * BoneTransform.lossyScale.y, longestDistance.z * BoneTransform.lossyScale.z);
			radius = longestDistance.magnitude;
		}

		public void CalculateUniqueVertexIndices()
		{
			for (int i = 0; i < Weights.Count; i++)
			{
				if (!uniqueVertexIndices.Contains(Weights[i].Index))
				{
					uniqueVertexIndices.Add(Weights[i].Index);
				}
			}
		}

		public void CalculateTriangles(int[] triangleArray)
		{
			triangleIndices.Clear();

			for (int i = 0; i < triangleArray.Length; i +=3)
			{
				int a = triangleArray[i];
				int b = triangleArray[i+1];
				int c = triangleArray[i+2];

				// Check if the vertices that make up the current triangle are present in the vertices this bone moves
				if (uniqueVertexIndices.Contains(a) && uniqueVertexIndices.Contains(b) && uniqueVertexIndices.Contains(c))
				{
					triangleIndices.Add(i);	
				}
			}
		}

		public Bone()
		{
			Weights = new List<VertexWeight>();
			uniqueVertexIndices = new List<int>();
			triangleIndices = new List<int>();
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

	public bool VisualiseBoundingSpheres = false;
	public int BoneIndexToVisualise = 0;

	public void ExtractMeshData()
	{
		if (skinnedMesh == null)
		{
			skinnedMesh = GetComponent<SkinnedMeshRenderer>();
		}

		vertices = skinnedMesh.sharedMesh.vertices;
		triangles = skinnedMesh.sharedMesh.triangles;

		tempVertices = new Vector3[vertices.Length];

		Matrix4x4[] bindPoses = skinnedMesh.sharedMesh.bindposes;
		BoneWeight[] boneWeights = skinnedMesh.sharedMesh.boneWeights;
       
		// Make a bone for each bone in the skinned mesh
		bones = new Bone[skinnedMesh.bones.Length];
		for (int i = 0; i < skinnedMesh.bones.Length; i++)
		{
			bones[i] = new Bone();
			bones[i].BoneTransform = skinnedMesh.bones[i];
		}

		// Create a vertex weight list for each bone, ready for quick calculation during an update...
		for (int i = 0; i < vertices.Length; i++)
		{
			BoneWeight currentBoneWeight = boneWeights[i];

			// You can have up to 4 bones affecting a vertex
			if (currentBoneWeight.weight0 != 0.0f)
			{
				Vector3 localPt = bindPoses[currentBoneWeight.boneIndex0].MultiplyPoint3x4(vertices[i]);
				bones[currentBoneWeight.boneIndex0].Weights.Add(new VertexWeight(i, localPt, currentBoneWeight.weight0));
			}
			if (currentBoneWeight.weight1 != 0.0f)
			{
				Vector3 localPt = bindPoses[currentBoneWeight.boneIndex1].MultiplyPoint3x4(vertices[i]);
				bones[currentBoneWeight.boneIndex1].Weights.Add(new VertexWeight(i, localPt, currentBoneWeight.weight1));
			}
			if (currentBoneWeight.weight2 != 0.0f)
			{
				Vector3 localPt = bindPoses[currentBoneWeight.boneIndex2].MultiplyPoint3x4(vertices[i]);
				bones[currentBoneWeight.boneIndex2].Weights.Add(new VertexWeight(i, localPt, currentBoneWeight.weight2));
			}
			if (currentBoneWeight.weight3 != 0.0f)
			{
				Vector3 localPt = bindPoses[currentBoneWeight.boneIndex3].MultiplyPoint3x4(vertices[i]);
				bones[currentBoneWeight.boneIndex3].Weights.Add(new VertexWeight(i, localPt, currentBoneWeight.weight3));
			}
		}

		for (int i = 0; i < bones.Length; i++)
		{
			bones[i].CalculateUniqueVertexIndices();
			bones[i].CalculateTriangles(triangles);
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

	private void OnValidate()
	{
		if (bones == null)
		{
			ExtractMeshData();
		}

		BoneIndexToVisualise = Mathf.Clamp(BoneIndexToVisualise, 0, bones.Length - 1);
	}

	private void OnDrawGizmosSelected()
	{
		if (VisualiseBoundingSpheres)
		{
			if (bones == null || skinnedMesh == null)
			{
				ExtractMeshData();
			}

			Vector3 sphereCenter = Vector3.zero;
			float radius = 0;

			bones[BoneIndexToVisualise].CalculateBoundingSphere(transform.localToWorldMatrix, vertices, ref sphereCenter, ref radius);

			Gizmos.DrawWireSphere(sphereCenter, radius);

			int uniqueVertexIndicesCount = bones[BoneIndexToVisualise].UniqueVertexIndices.Count;

			Gizmos.color = Color.white;
			// Draw the verts
			for (int i = 0; i < uniqueVertexIndicesCount; i++)
			{
				int currentIndex = bones[BoneIndexToVisualise].UniqueVertexIndices[i];
				Gizmos.DrawWireSphere(transform.localToWorldMatrix.MultiplyPoint3x4(vertices[currentIndex]), 0.1f);
			}

			// Draw the triangles
			Gizmos.color = Color.red;
			for (int i = 0; i < bones[BoneIndexToVisualise].TriangleIndices.Count; i++)
			{
				int currentTriangleIndex = bones[BoneIndexToVisualise].TriangleIndices[i];

				Vector3 a = transform.localToWorldMatrix.MultiplyPoint3x4(vertices[ triangles[currentTriangleIndex] ]);
				Vector3 b = transform.localToWorldMatrix.MultiplyPoint3x4(vertices[ triangles[currentTriangleIndex + 1] ]);
				Vector3 c = transform.localToWorldMatrix.MultiplyPoint3x4(vertices[ triangles[currentTriangleIndex + 2] ]);

				Gizmos.DrawLine(a, b);
				Gizmos.DrawLine(b, c);
				Gizmos.DrawLine(c, a);
			}
		}
	}
}