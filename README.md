Some utilities that allow for real time collision detection on skinned meshes.

Currently supported collisions:
* Sphere vs Skinned Mesh
* Raycast vs Skinned Mesh

Both return information on points hit, normals of hit, barycentric coordinates, bones and other useful information.

Usage:
* Attach SkinnedMeshCollider.cs to a skinned mesh.
* Call SkinnedMeshCollisionController.RaycastAll/SphereCastAll from SkinnedMeshCollisionController depending on your usage
* Profit

![screenshot](http://i.imgur.com/ZdgT0Av.png)
