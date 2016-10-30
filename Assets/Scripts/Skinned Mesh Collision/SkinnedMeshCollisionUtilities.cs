using UnityEngine;
using System;
using System.Collections;

[System.Serializable]
public struct SkinnedMeshHit
{
	public float distance;
	public Vector3 point;
	public Vector3 normal;
	public Vector3 barycentricCoordinate;
	public int triangleIndex;
	public Transform bone;
}

public static class SkinnedMeshCollisionUtilities
{
	public static Vector3 GetBarycentricCoordinate(Vector3 pointInTriangle, Vector3 vertexA, Vector3 vertexB, Vector3 vertexC)
	{
		Vector3 v0 = vertexB - vertexA; 
		Vector3 v1 = vertexC - vertexA; 
		Vector3 v2 = pointInTriangle - vertexA;

		// Basically applying cramers rule
	    float d00 = Vector3.Dot(v0, v0);
		float d01 = Vector3.Dot(v0, v1);
		float d11 = Vector3.Dot(v1, v1);
		float d20 = Vector3.Dot(v2, v0);
		float d21 = Vector3.Dot(v2, v1);
	    float invDenom = 1.0f / (d00 * d11 - d01 * d01);

		Vector3 barycentricCoordinate = Vector3.zero;

		barycentricCoordinate.x = (d11 * d20 - d01 * d21) * invDenom;
		barycentricCoordinate.y = (d00 * d21 - d01 * d20) * invDenom;
		barycentricCoordinate.z = 1.0f - barycentricCoordinate.y - barycentricCoordinate.z;

		return barycentricCoordinate;
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

		Vector3 triangleNormal = Vector3.Cross(b - a, c - a).normalized;

		// Step 1: Test against the triangle surface
		if (TriangleRayIntersection(triangleIndex, a, b, c, new Ray(sphereOrigin, -triangleNormal), out hit, sphereRadius))
		{
			return true;
		}

		// Step 2: Test against the triangle edges
		Vector3 hitPoint = Vector3.zero;
		float hitDistance = Mathf.Infinity;
		/*Vector3 ab = b-a;
		Vector3 bc = c-b;
		Vector3 ca = a-c;

		Vector3 abProjection = Vector3.Project(sphereOrigin - ab, ab);
		Vector3 bcProjection = Vector3.Project(sphereOrigin - bc, bc);
		Vector3 caProjection = Vector3.Project(sphereOrigin - ca, ca);

		// The closest point of the projection from the sphere
		Vector3 abPoint = a + abProjection;
		Vector3 bcPoint = b + bcProjection;
		Vector3 caPoint = c + caProjection;

		// The vector from the sphere to the closest point of projection
		Vector3 S2ABP = sphereOrigin - abPoint;
		Vector3 S2BCP = sphereOrigin - bcPoint;
		Vector3 S2CAP = sphereOrigin - caPoint;



		// Now we find the closest projection point
		if (S2ABP.sqrMagnitude < S2BCP.sqrMagnitude && S2ABP.sqrMagnitude < S2CAP.sqrMagnitude)
		{
			hitPoint = abPoint;
			hitDistance = S2ABP.magnitude;		
		}
		else if (S2BCP.sqrMagnitude < S2ABP.sqrMagnitude && S2BCP.sqrMagnitude < S2CAP.sqrMagnitude)
		{
			hitPoint = bcPoint;
		 	hitDistance = S2BCP.magnitude;
		}
		else
		{
			hitPoint = caPoint;
			hitDistance = S2CAP.magnitude;
		}

		if (hitDistance <= sphereRadius)
		{	
			hit = new SkinnedMeshHit();

			hit.distance = hitDistance;
			hit.point = hitPoint;
			hit.normal = triangleNormal;
			hit.barycentricCoordinate = GetBarycentricCoordinate(hit.point, a, b, c);
			hit.triangleIndex = triangleIndex;

			Debug.Log("Triangle Edge Case");

			return true;
		}*/

		// Step 3: Check verts against sphere center
		float radiusSquared = sphereRadius * sphereRadius;
		bool wasVertexHit = false;

		if ((sphereOrigin - a).sqrMagnitude <= radiusSquared)
		{
			hitDistance = (sphereOrigin - a).magnitude;
			hitPoint = a;
			wasVertexHit = true;
		}

		if ((sphereOrigin - b).sqrMagnitude <= radiusSquared)
		{
			hitDistance = (sphereOrigin - b).magnitude;
			hitPoint = b;
			wasVertexHit = true;
		}

		if ((sphereOrigin - c).sqrMagnitude <= radiusSquared)
		{
			hitDistance = (sphereOrigin - c).magnitude;
			hitPoint = c;
			wasVertexHit = true;
		}

		if (wasVertexHit)
		{
			hit = new SkinnedMeshHit();

			hit.distance = hitDistance;
			hit.point = hitPoint;
			hit.normal = triangleNormal;
			hit.barycentricCoordinate = GetBarycentricCoordinate(hit.point, a, b, c);
			hit.triangleIndex = triangleIndex;

			return true;
		}

		return false;
	}

	public static bool SphereSphereIntersection(Vector3 center1, float radius1, Vector3 center2, float radius2)
	{
		float distanceSqaured = (center2 - center1).sqrMagnitude;
		float radiusSquared = (radius1 + radius2) * (radius1 * radius2);

		return distanceSqaured < radiusSquared;
	}

	public static bool RaySphereIntersection(Ray ray, Vector3 sphereCenter, float sphereRadius, out Vector3 intersectionPoint, out Vector3 normal)
	{
		intersectionPoint = Vector3.zero;
		normal = Vector3.zero;

		// Calculate ray start's offset from the sphere center
		Vector3 ray2Sphere = ray.origin - sphereCenter;

		float radiusSquared = sphereRadius * sphereRadius;
		float ray2SphereDOTdirection = Vector3.Dot(ray2Sphere, ray.direction);

		// The sphere is behind or surrounding the start point.
		if(ray2SphereDOTdirection < 0 || Vector3.Dot(ray2Sphere, ray2Sphere) < radiusSquared)
		{
			return false;
		}

		// Flatten ray2Sphere into the plane passing through c perpendicular to the ray.
		// This gives the closest approach of the ray to the center.
		Vector3 a = ray2Sphere - ray2SphereDOTdirection * ray.direction;

		float aSquared = Vector3.Dot(a, a);

		// Closest approach is outside the sphere.
		if(aSquared > radiusSquared)
		{
		  return false;
		}

		// Calculate distance from plane where ray enters/exits the sphere.    
		float h = Mathf.Sqrt(radiusSquared - aSquared);

		// Calculate intersection point relative to sphere center.
		Vector3 i = a - h * ray.direction;

		intersectionPoint = sphereCenter + i;
		normal = i.normalized;

		return true;
	}
}