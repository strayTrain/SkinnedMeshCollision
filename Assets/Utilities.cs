using UnityEngine;
using System.Collections;

public static class Utilities
{
	public static Vector3 GetBarycentricCoordinate(Vector3 pointInTriangle, Vector3 vertexA, Vector3 vertexB, Vector3 vertexC)
	{
		Vector3 v0 = vertexB - vertexA; 
		Vector3 v1 = vertexC - vertexA; 
		Vector3 v2 = pointInTriangle - vertexA;

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

	public static bool TriangleRayIntersection(Vector3 p1, Vector3 p2, Vector3 p3, Ray ray, out RaycastHit hit, float rayCastDistance = Mathf.Infinity)
     {
     	 hit = new RaycastHit();

         // Vectors from p1 to p2/p3 (edges)
         Vector3 edge1, edge2;  
 
         Vector3 p, q, t;
         float determinant, inverseDeterminant, u, v;
 
         //Find vectors for two edges sharing vertex/point p1
         edge1 = p2 - p1;
         edge2 = p3 - p1;
 
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
         t = ray.origin - p1;

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

			 hit.barycentricCoordinate = GetBarycentricCoordinate(hit.point, p1, p2, p3);

             return true;
         }
 
         // No hit at all
         return false;
     }

	public static bool TriangleSphereIntersection(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 sphereOrigin, float sphereRadius, out RaycastHit hit, float rayCastDistance = Mathf.Infinity) 
	{
		hit = new RaycastHit();

		// Step 1: Test against the triangle surface

		return true;
	}
}
