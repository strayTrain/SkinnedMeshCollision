using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SkinnedMeshCollider : MonoBehaviour 
{
	private class VertexWeight
	{
	    public int index;
	    public Vector3 localPosition;
	    public float weight;
	 
	    public VertexWeight(int i, Vector3 p, float w)
	    {
	        index = i;
	        localPosition = p;
	        weight = w;
	    }
	}
	 
	private class WeightList
	{
	    public Transform transform;
	    public List<VertexWeight> weights;

	    public WeightList()
	    {
	        weights = new List<VertexWeight>();
	    }
	}

	private Vector3[] vertices;
	private int[] triangles;
	private WeightList[] nodeWeights;

	private SkinnedMeshRenderer skinnedMesh;

	public void ExtractMeshData()
	{
		if (skinnedMesh == null)
		{
			skinnedMesh = GetComponent<SkinnedMeshRenderer>();
		}

		vertices = skinnedMesh.sharedMesh.vertices;
		triangles = skinnedMesh.sharedMesh.triangles;

		Vector3[] cachedVertices = skinnedMesh.sharedMesh.vertices;
		Matrix4x4[] cachedBindposes = skinnedMesh.sharedMesh.bindposes;
		BoneWeight[] cachedBoneWeights = skinnedMesh.sharedMesh.boneWeights;
       
        // Make a CWeightList for each bone in the skinned mesh
		nodeWeights = new WeightList[skinnedMesh.bones.Length];
		for ( int i = 0 ; i < skinnedMesh.bones.Length ; i++ )
        {
            nodeWeights[i] = new WeightList();
			nodeWeights[i].transform = skinnedMesh.bones[i];
        }

        // Create a bone weight list for each bone, ready for quick calculation during an update...
        for ( int i = 0 ; i < cachedVertices.Length ; i++ )
        {
            BoneWeight bw = cachedBoneWeights[i];
            if (bw.weight0 != 0.0f)
            {
                Vector3 localPt = cachedBindposes[bw.boneIndex0].MultiplyPoint3x4( cachedVertices[i] );
                nodeWeights[bw.boneIndex0].weights.Add( new VertexWeight( i, localPt, bw.weight0 ) );
            }
            if (bw.weight1 != 0.0f)
            {
                Vector3 localPt = cachedBindposes[bw.boneIndex1].MultiplyPoint3x4( cachedVertices[i] );
                nodeWeights[bw.boneIndex1].weights.Add( new VertexWeight( i, localPt, bw.weight1 ) );
            }
            if (bw.weight2 != 0.0f)
            {
                Vector3 localPt = cachedBindposes[bw.boneIndex2].MultiplyPoint3x4( cachedVertices[i] );
                nodeWeights[bw.boneIndex2].weights.Add( new VertexWeight( i, localPt, bw.weight2 ) );
            }
            if (bw.weight3 != 0.0f)
            {
                Vector3 localPt = cachedBindposes[bw.boneIndex3].MultiplyPoint3x4( cachedVertices[i] );
                nodeWeights[bw.boneIndex3].weights.Add( new VertexWeight( i, localPt, bw.weight3 ) );
            }
        }

		//WeldVertices(ref vertices, ref triangles, Mathf.Epsilon, 2);
	}

	public void UpdateCollisionMesh()
	{
		Vector3[] newVerts = new Vector3[vertices.Length];
       
        // Now get the local positions of all weighted indices
        int nodeWeightsLength = nodeWeights.Length;
        int nodeWeightCount = 0;
        VertexWeight currentVertexWeight;

		for (int i = 0; i < nodeWeightsLength; i++)
        {
        	nodeWeightCount = nodeWeights[i].weights.Count;
			for (int j = 0; j < nodeWeightCount; j++)
        	{
				currentVertexWeight = nodeWeights[i].weights[j];
				newVerts[currentVertexWeight.index] += nodeWeights[i].transform.localToWorldMatrix.MultiplyPoint3x4(currentVertexWeight.localPosition) * currentVertexWeight.weight;
        	}
        }

        // Now convert each point into local coordinates of this object.
        for ( int i = 0 ; i < newVerts.Length ; i++ )
        {
            newVerts[i] = transform.InverseTransformPoint(newVerts[i]);
        }
 
        vertices = newVerts;
	}

	private void Start()
	{
		ExtractMeshData();
	}

	private void LateUpdate()
	{
		UpdateCollisionMesh();
	}

	private void OnDrawGizmos()
	{
		if (skinnedMesh == null)
		{
			ExtractMeshData();
		}

		for (int i = 0; i < triangles.Length; i += 3)
		{
			// The 3 verts that make up this triangle
			Vector3 a, b, c;

			a = transform.TransformPoint(vertices[triangles[i]]);
			b = transform.TransformPoint(vertices[triangles[i+1]]);
			c = transform.TransformPoint(vertices[triangles[i+2]]);

			Ray testRay = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f));

			Gizmos.DrawRay(Camera.main.transform.position, (Camera.main.transform.position + Camera.main.transform.forward * 10) - Camera.main.transform.position);
			RaycastHit hit;

			if (TriangleRayIntersection(a, b, c, testRay, out hit))
			{
				Gizmos.color = Color.red;

				Gizmos.DrawLine(a, b);
				Gizmos.DrawLine(b, c);
				Gizmos.DrawLine(c, a);

				Gizmos.color = Color.green;

				Gizmos.DrawWireSphere(hit.point, 0.1f);
			}
		}
	}

	private bool TriangleRayIntersection(Vector3 p1, Vector3 p2, Vector3 p3, Ray ray, out RaycastHit hit)
     {
     	 hit = new RaycastHit();

         // Vectors from p1 to p2/p3 (edges)
         Vector3 e1, e2;  
 
         Vector3 p, q, t;
         float det, invDet, u, v;
 
         //Find vectors for two edges sharing vertex/point p1
         e1 = p2 - p1;
         e2 = p3 - p1;
 
         // calculating determinant 
         p = Vector3.Cross(ray.direction, e2);
 
         //Calculate determinat
         det = Vector3.Dot(e1, p);
 
         //if determinant is near zero, ray lies in plane of triangle otherwise not
         if (det > - Mathf.Epsilon && det < Mathf.Epsilon) 
         { 
         	return false; 
         }

         invDet = 1.0f / det;
 
         //calculate distance from p1 to ray origin
         t = ray.origin - p1;

         //Calculate u parameter
         u = Vector3.Dot(t, p) * invDet;
 
         //Check for ray hit
         if (u < 0 || u > 1) { return false; }
 
         //Prepare to test v parameter
         q = Vector3.Cross(t, e1);
 
         //Calculate v parameter
         v = Vector3.Dot(ray.direction, q) * invDet;
 
         //Check for ray hit
         if (v < 0 || u + v > 1) { return false; }

		 float length = Vector3.Dot(e2, q) * invDet;

		 if (length > Mathf.Epsilon)
         { 
             //ray does intersect
             hit.distance = length;
			 hit.point = ray.origin + (ray.direction * length);

             return true;
         }
 
         // No hit at all
         return false;
     }
}
