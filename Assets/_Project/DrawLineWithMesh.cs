using System.Collections;
using UnityEngine;

namespace MuralTrace
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class DrawLineWithMesh : MonoBehaviour
    {
        private Mesh mesh;
        private Vector3[] vertices;
        private int[] triangles;
        private int currentSegment = 0;
        public float lineThickness = 0.05f; // Thickness of the "line"
        public float drawSpeed = 0.5f; // Speed of drawing

        void Start()
        {
            mesh = new Mesh();
            GetComponent<MeshFilter>().mesh = mesh;

            // Define the points of the line
            Vector3 startPoint = new Vector3(0, 0, 0); // Starting point
            Vector3 endPoint = new Vector3(2, 1, 0); // End point

            // Start the coroutine to progressively draw the line
            StartCoroutine(DrawLineWithTrianglesOverTime(startPoint, endPoint));
        }

        IEnumerator DrawLineWithTrianglesOverTime(Vector3 start, Vector3 end)
        {
            float distance = Vector3.Distance(start, end);
            Vector3 direction = (end - start).normalized;
            Vector3 perpendicular = Vector3.Cross(direction, Vector3.forward).normalized * lineThickness;

            // Define the vertices for the initial thin rectangle (two triangles)
            vertices = new Vector3[4]; // 4 vertices for two triangles
            triangles = new int[6]; // 6 indices for two triangles

            // Set up the rectangle with two triangles
            vertices[0] = start - perpendicular; // Bottom-left
            vertices[1] = start + perpendicular; // Top-left
            vertices[2] = start - perpendicular; // Bottom-right
            vertices[3] = start + perpendicular; // Top-right

            // Initial empty mesh
            mesh.vertices = vertices;
            mesh.triangles = new int[] { };

            // Gradually move toward the endpoint
            Vector3 currentEnd = start;
            while (currentEnd != end)
            {
                currentEnd = Vector3.MoveTowards(currentEnd, end, Time.deltaTime / drawSpeed);

                // Adjust the right vertices (for the moving endpoint)
                vertices[2] = currentEnd - perpendicular; // Bottom-right
                vertices[3] = currentEnd + perpendicular; // Top-right

                // Define the two triangles that form the "line"
                triangles[0] = 0;
                triangles[1] = 1;
                triangles[2] = 2; // First triangle
                triangles[3] = 1;
                triangles[4] = 3;
                triangles[5] = 2; // Second triangle

                // Update the mesh
                mesh.vertices = vertices;
                mesh.triangles = triangles;
                mesh.RecalculateNormals();

                yield return null; // Wait for the next frame
            }

            // After drawing is complete, finalize the mesh
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
        }
    }
}