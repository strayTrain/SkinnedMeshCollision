using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public struct SkinnedMeshHit
{
	public float distance;
	public Vector3 point;
	public Vector3 normal;
	public Vector3 barycentricCoordinate;
	public int triangleIndex;
	public Transform bone;
	public SkinnedMeshCollider skinnedMeshCollider;
}

public static class SkinnedMeshCollisionUtilities
{
	public static Vector3 GetBarycentricCoordinate(Vector3 p, Vector3 a, Vector3 b, Vector3 c)
	{
		Vector3 v0 = b - a; 
		Vector3 v1 = c - a; 
		Vector3 v2 = p - a;

	    float d00 = Vector3.Dot(v0, v0);
		float d01 = Vector3.Dot(v0, v1);
		float d11 = Vector3.Dot(v1, v1);
		float d20 = Vector3.Dot(v2, v0);
		float d21 = Vector3.Dot(v2, v1);
	    float denom = d00 * d11 - d01 * d01;

	    float v = (d11 * d20 - d01 * d21) / denom;
	    float w = (d00 * d21 - d01 * d20) / denom;
	    float u = 1.0f - v - w;

	    return new Vector3(u, v, w);
	}

	public static Vector3 BarycentricToWorldPosition(Vector3 barycentricCoordinate, Vector3 vertexA, Vector3 vertexB, Vector3 vertexC)
	{
		float u = barycentricCoordinate.x;
		float v = barycentricCoordinate.y;
		float w = 1 - (u + v);

		return new Vector3(u * vertexA.x + v * vertexB.x + w * vertexC.x,
						   u * vertexA.y + v * vertexB.y + w * vertexC.y,
						   u * vertexA.z + v * vertexB.z + w * vertexC.z);
	}

	public static bool TriangleRayIntersection(int triangleIndex, Vector3 a, Vector3 b, Vector3 c, Ray ray, out SkinnedMeshHit hit, float rayCastDistance = Mathf.Infinity)
     {
		 hit = new SkinnedMeshHit();

         // Vectors from p1 to p2/p3 (edges)
         Vector3 edge1, edge2;  
 
         Vector3 p, q, t;
         float determinant, inverseDeterminant, u, v;
 
         //Find vectors for two edges sharing vertex/point p1
         edge1 = b - a;
         edge2 = c - a;
 
         // calculating determinant 
         p = Vector3.Cross(ray.direction, edge2);
 
         //Calculate determinat
         determinant = Vector3.Dot(edge1, p);
 
         //if determinant is near zero, ray lies in plane of triangle otherwise not
         if (determinant > - Mathf.Epsilon && determinant < Mathf.Epsilon) 
         { 
         	return false; 
         }

         inverseDeterminant = 1.0f / determinant;
 
         //calculate distance from p1 to ray origin
         t = ray.origin - a;

         //Calculate u parameter
         u = Vector3.Dot(t, p) * inverseDeterminant;
 
         //Check for ray hit
         if (u < 0 || u > 1) { return false; }
 
         //Prepare to test v parameter
         q = Vector3.Cross(t, edge1);
 
         //Calculate v parameter
         v = Vector3.Dot(ray.direction, q) * inverseDeterminant;
 
         //Check for ray hit
         if (v < 0 || u + v > 1) { return false; }

		 float length = Vector3.Dot(edge2, q) * inverseDeterminant;

		 if (length > Mathf.Epsilon && length <= rayCastDistance)
         { 
             hit.distance = length;
			 hit.point = ray.origin + (ray.direction * length);
			 hit.normal = Vector3.Cross(edge1, edge2).normalized;
			 hit.barycentricCoordinate = GetBarycentricCoordinate(hit.point, a, b, c);
			 hit.triangleIndex = triangleIndex;

             return true;
         }
 
         // No hit at all
         return false;
     }

	public static bool TriangleSphereIntersection(int triangleIndex, Vector3 a, Vector3 b, Vector3 c, Vector3 sphereOrigin, float sphereRadius, out SkinnedMeshHit hit) 
	{
		hit = new SkinnedMeshHit();
		bool wasCollisionDetected = false;

		Vector3 triangleNormal = Vector3.Cross(b - a, c - a).normalized;

		Vector3 closestPoint = GetClosestPointOnTriangle(sphereOrigin, a, b, c);

		Vector3 v = closestPoint - sphereOrigin;
		wasCollisionDetected = Vector3.Dot(v, v) <= sphereRadius*sphereRadius;

		if (wasCollisionDetected)
		{
			hit.distance = Vector3.Distance(sphereOrigin, closestPoint);
			hit.point = closestPoint;
			hit.normal = triangleNormal;
			hit.barycentricCoordinate = GetBarycentricCoordinate(closestPoint, a, b, c);
			hit.triangleIndex = triangleIndex;
		}

		return wasCollisionDetected;
	}

	public static bool SphereSphereIntersection(Vector3 center1, float radius1, Vector3 center2, float radius2)
	{
		float distanceSqaured = (center2 - center1).sqrMagnitude;
		float radiusSquared = (radius1 + radius2) * (radius1 * radius2);

		return distanceSqaured < radiusSquared;
	}

	public static bool RaySphereIntersection(Ray ray, Vector3 sphereCenter, float sphereRadius, float distance = Mathf.Infinity)
	{
		if ((ray.origin - sphereCenter).sqrMagnitude <= (sphereRadius * sphereRadius))
		{
			return true;
		}

		Vector3 L = sphereCenter - ray.origin;
		float tc = Vector3.Dot(L, ray.direction);
		
		if ( tc < 0 ) 
		{
			return false;
		}

		float d2 = (tc*tc) - (Vector3.Dot(L, L));
		
		float radius2 = sphereRadius * sphereRadius;

		if ( d2 > radius2) 
		{
			return false;
		}

		//solve for t1c
		float t1c = Mathf.Sqrt(radius2 - d2);

		//solve for intersection points
		Vector3 intersectionPoint1 = ray.origin + ray.direction * (tc - t1c);
		Vector3 intersectionPoint2 = ray.origin + ray.direction * (tc + t1c);

		// Lastly check if the intersection point are within the distance
		float distanceSquared = distance * distance;
		if ((ray.origin - intersectionPoint1).sqrMagnitude >= distanceSquared || (ray.origin - intersectionPoint2).sqrMagnitude >= distanceSquared)
		{
			return false;
		}

		return true;
	}

	// Sort a list of hits by distance from an origin point
	public static void SortHitsByDistance(ref List<SkinnedMeshHit> hits, Vector3 point)
	{
		SkinnedMeshHit tmp;

		for (int i = hits.Count - 1; i > 0; i--) 
		{
			for (int j = 0; j < i; j++) 
			{
				if ((point - hits[j].point).sqrMagnitude > (point - hits[j + 1].point).sqrMagnitude)
				{
					tmp = hits[j];
					hits [j] = hits[j + 1];
					hits [j + 1] = tmp;
				}
			}
		}	
	}

	// Finds the closest point on a triangle with vertices a, b and c to a point p
	public static Vector3 GetClosestPointOnTriangle(Vector3 p, Vector3 a, Vector3 b, Vector3 c)
	{
		// Check if P in vertex region outside A
		Vector3 ab = b-a;
		Vector3 ac = c-a;
		Vector3 ap = p-a;

		float d1 = Vector3.Dot(ab, ap);
		float d2 = Vector3.Dot(ac, ap);

		if (d1 <= 0 && d2 <= 0)
		{
			return a;
		} 	

		// Check if P in vertex region outside B
		Vector3 bp = p-b;
		float d3 = Vector3.Dot(ab, bp);
		float d4 = Vector3.Dot(ac, bp);

		if (d3 >= 0 && d4 <= d3)
		{
			return b;
		}

		// Check if P in edge region of AB, return projection of P onto AB if that's the case
		float vc = d1*d4 - d3*d2;

		if (vc <= 0 && d1 >= 0 && d3 <= 0)
		{
			float v = d1/(d1-d3);
			return a + v*ab;
		}

		// Check if P in vertex region outside C
		Vector3 cp = p-c;
		float d5 = Vector3.Dot(ab, cp);
		float d6 = Vector3.Dot(ac, cp);

		if (d6 >= 0 && d5 <= d6)
		{
			return c;
		}

		// Check if P in edge region of AC, return projection of P onto AC if that's the case
		float vb = d5*d2 - d1*d6;

		if (vb <= 0 && d2 >= 0 && d6 <= 0)
		{
			float w = d2/(d2-d6);
			return a + w*ac;
		}

		// Check if P in edge region of BC, return projection of P onto BC if that's the case
		float va = d3*d6 - d5*d4;

		if (va <= 0 && (d4-d3) >= 0 && (d5-d6) >= 0)
		{
			float w = (d4-d3)/((d4-d3) + (d5-d6));
			return b + w*(c-b);
		}

		// P inside the face region, Compute Q through its barycentric coordinates (u, v, w)
		float denom = 1.0f/(va+vb+vc);
		float j = vb*denom;
		float k = vc*denom;

		return a + ab*j + ac*k;
	}
}