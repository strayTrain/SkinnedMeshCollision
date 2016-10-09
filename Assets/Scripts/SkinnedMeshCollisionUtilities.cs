using UnityEngine;
using System.Collections;

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

	public static bool TriangleRayIntersection(Vector3 a, Vector3 b, Vector3 c, Ray ray, out RaycastHit hit, float rayCastDistance = Mathf.Infinity)
     {
     	 hit = new RaycastHit();

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
             //ray does intersect
             hit.distance = length;
			 hit.point = ray.origin + (ray.direction * length);
			 hit.normal = Vector3.Cross(edge1, edge2).normalized;

			 hit.barycentricCoordinate = GetBarycentricCoordinate(hit.point, a, b, c);

             return true;
         }
 
         // No hit at all
         return false;
     }

	public static bool TriangleSphereIntersection(Vector3 a, Vector3 b, Vector3 c, Vector3 sphereOrigin, float sphereRadius, out RaycastHit hit) 
	{
		hit = new RaycastHit();

		Vector3 triangleNormal = Vector3.Cross(b - a, c - a).normalized;

		// Step 1: Test against the triangle surface
		if (TriangleRayIntersection(a, b, c, new Ray(sphereOrigin, -triangleNormal), out hit, sphereRadius))
		{
			return true;
		}

		// Step 2: Test against the triangle edges
		/*Vector3 ba = b-a;
		Vector3 bc = b-c;
		Vector3 ca = c-a;

		Vector3 t1 = Vector3.Project(sphereOrigin - b, ba);
		Vector3 t2 = Vector3.Project(sphereOrigin - b, bc);
		Vector3 t3 = Vector3.Project(sphereOrigin - c, ca);

		Vector3 lowestT = Vector3.one * Mathf.Infinity;
		bool wasLowestTFound = false;

		if (t1.magnitude < sphereRadius && t1.magnitude < lowestT.magnitude)
		{
			lowestT = t1;
			wasLowestTFound = true;
		}

		if (t2.magnitude < sphereRadius && t2.magnitude < lowestT.magnitude)
		{
			lowestT = t2;
			wasLowestTFound = true;
		}

		if (t3.magnitude < sphereRadius && t3.magnitude < lowestT.magnitude)
		{
			lowestT = t3;
			wasLowestTFound = true;
		}

		if (wasLowestTFound)
		{
			hit.distance = lowestT.magnitude;
			hit.point = sphereOrigin + (lowestT * hit.distance);
			hit.normal = triangleNormal;

			hit.barycentricCoordinate = GetBarycentricCoordinate(hit.point, a, b, c);

			Debug.Log("FOUND");

			return true;
		}*/

		return false;
	}
}