using UnityEngine;

namespace Buildwise.Core
{
    public class HelperFunctions
    {
        public Vector3[] GetBoundingBoxCorners(Transform selection)
        {
            Bounds bounds = selection.GetComponent<Renderer>().bounds;
            Vector3 center = bounds.center;
            Vector3 extents = bounds.extents;

            Vector3 topRight = center + new Vector3(extents.x, extents.y, extents.z);
            Vector3 topLeft = center + new Vector3(-extents.x, extents.y, extents.z);
            Vector3 bottomRight = center + new Vector3(extents.x, -extents.y, extents.z);
            Vector3 bottomLeft = center + new Vector3(-extents.x, -extents.y, extents.z);
            Vector3 topBack = center + new Vector3(extents.x, extents.y, -extents.z);
            Vector3 topFront = center + new Vector3(-extents.x, extents.y, -extents.z);
            Vector3 bottomBack = center + new Vector3(extents.x, -extents.y, -extents.z);
            Vector3 bottomFront = center + new Vector3(-extents.x, -extents.y, -extents.z);

            var corners = new Vector3[] { topRight, topLeft, bottomRight, bottomLeft, topBack, topFront, bottomBack, bottomFront };

            return corners;
        }

        public Vector3 GetMostRightPointAsSeenFromCamera(Camera cam, Vector3[] points)
        {
            Vector3 screenUpperRightCorner = cam.WorldToScreenPoint(points[0]);
            Vector3 worldUpperRightCorner = points[0];

            foreach (var b in points)
            {
                var screenCoordinate = cam.WorldToScreenPoint(b);
                if (screenCoordinate.x > screenUpperRightCorner.x)
                {
                    screenUpperRightCorner = screenCoordinate;
                    worldUpperRightCorner = b;
                }
            }
            return worldUpperRightCorner;
        }

        public Vector3 GetHighestPointAsSeenFromCamera(Camera cam, Vector3[] points)
        {
            Vector3 screenHighestCorner = cam.WorldToScreenPoint(points[0]);
            Vector3 worldHighestCorner = points[0];

            foreach (var b in points)
            {
                var screenCoordinate = cam.WorldToScreenPoint(b);
                if (screenCoordinate.y > screenHighestCorner.y)
                {
                    screenHighestCorner = screenCoordinate;
                    worldHighestCorner = b;
                }
            }
            return worldHighestCorner;
        }

        public Vector3 GetClosestPointAsSeenFromCamera(Camera cam, Vector3[] points)
        {
            Vector3 screenClosestCorner = cam.WorldToScreenPoint(points[0]);
            Vector3 worldClosestCorner = screenClosestCorner;

            foreach (var b in points)
            {
                var screenCoordinate = cam.WorldToScreenPoint(b);
                if (screenCoordinate.z < screenClosestCorner.z)
                {
                    screenClosestCorner = screenCoordinate;
                    worldClosestCorner = b;
                }
            }
            return worldClosestCorner;
        }

        /// <summary>
        /// Given a selection and a hit, returns the index of the material that was hit.
        /// The index refers to the submesh hit. If an object has several submeshes, 
        /// it can have several materials. This allows thus to identify which part of the object was hit.
        /// </summary>
        /// <param name="selection">The object hit</param>
        /// <param name="hit">The hit of the Ray</param>
        /// <returns>The index of the submesh hit</returns>
        public int GetMaterialIndex(Transform selection, RaycastHit hit)
        {
            int triangleIndex = hit.triangleIndex;
            Mesh m = selection.GetComponent<MeshCollider>().sharedMesh;
            int materialIndex = -1;
            if (m != null)
            {
                int nbSubMeshes = m.subMeshCount;
                for (int i = 0; i < nbSubMeshes; i++)
                {
                    if (m.GetTopology(i) == MeshTopology.Triangles) // 3 indices per triangle
                    {
                        var subMesh = m.GetSubMesh(i);
                        if (triangleIndex >= subMesh.indexStart / 3 && triangleIndex < (subMesh.indexStart + subMesh.indexCount) / 3)
                        {
                            materialIndex = i;
                            break;
                        }
                    }
                    else
                    {
                        Debug.LogError("Only triangle meshes are accounted for!");
                        return -1;
                    }
                }
            }
            return materialIndex;
        }

        /// <summary>
        /// Add a Component of type T to each child (including all depths) of the parent GameObject provided.
        /// The component is only added if not already present on the child.
        /// </summary>
        /// <typeparam name="T">The type of Component to be added</typeparam>
        /// <param name="parent">The parent GameObject</param>
        /// <param name="includeParent">True if the component is to be added on the parent as well</param>
        public static void AddComponentToAllChildren<T>(Transform parent, bool includeParent) where T : MonoBehaviour
        {
            if (includeParent && parent.TryGetComponent<T>(out T t) == false)
            {
                parent.gameObject.AddComponent<T>();
            }
            foreach (Transform child in parent)
            {
                if (child.TryGetComponent(out T tt) == false)
                {
                    child.gameObject.AddComponent<T>();
                }
                AddComponentToAllChildren<T>(child, true);
            }
        }

        public static void AddCollidersToAllChildren(GameObject go)
        {
            foreach (Transform child in go.transform)
            {
                if (child.TryGetComponent(out Collider c) == false)
                {
                    child.gameObject.AddComponent<MeshCollider>();
                }
            }
        }

        /// <summary>
        /// Assigns a tag to all children GameObjects of the GameObject passed as an argument
        /// </summary>
        /// <param name="go">The parent GameObject</param>
        /// <param name="tag">The tag to apply to all the children</param>
        /// <param name="includeParent">True if the tag is to be applied to the parent GameObject itself too</param>
        public static void AddTagToAllChildren(GameObject go, string tag, bool includeParent = false)
        {
            foreach (Transform child in go.transform)
            {
                child.gameObject.tag = tag;
            }
            if (includeParent) go.tag = tag;
        }
    }
}
