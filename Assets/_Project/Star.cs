using System.Collections;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class DrawStarWithTriangles : MonoBehaviour
{
    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;
    public float lineThickness = 0.05f;  // Thickness of the "line"
    public float drawSpeed = 0.5f;  // Speed of drawing
    public float radiusOuter = 2f;  // Outer radius of the star
    public float radiusInner = 1f;  // Inner radius of the star
    private int totalPoints = 10;   // 5 outer and 5 inner points

    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        // Define the vertices of the star
        Vector3[] starVertices = new Vector3[totalPoints];
        for (int i = 0; i < totalPoints; i++)
        {
            float angle = i * Mathf.PI * 2 / totalPoints;
            float radius = (i % 2 == 0) ? radiusOuter : radiusInner;  // Outer or inner radius
            starVertices[i] = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0);
        }

        // Start drawing the star edges sequentially
        StartCoroutine(DrawStarEdges(starVertices));
    }

    IEnumerator DrawStarEdges(Vector3[] starVertices)
    {
        // Loop through the vertices and connect them to form the star
        for (int i = 0; i < starVertices.Length; i++)
        {
            Vector3 start = starVertices[i];
            Vector3 end = starVertices[(i + 2) % starVertices.Length];  // Skip one vertex to create the star shape
            yield return StartCoroutine(DrawStarEdge(start, end));  // Draw each edge one by one
        }
    }

    IEnumerator DrawStarEdge(Vector3 start, Vector3 end)
    {
        float distance = Vector3.Distance(start, end);
        Vector3 direction = (end - start).normalized;
        Vector3 perpendicular = Vector3.Cross(direction, Vector3.forward).normalized * lineThickness;

        // Define the vertices for the current line segment (rectangle)
        vertices = new Vector3[4]; // 4 vertices for two triangles per edge
        triangles = new int[6];    // 6 indices for two triangles per edge

        // Set up the initial rectangle with two triangles
        vertices[0] = start - perpendicular;  // Bottom-left of the line
        vertices[1] = start + perpendicular;  // Top-left of the line
        vertices[2] = start - perpendicular;  // Bottom-right (start as same as left)
        vertices[3] = start + perpendicular;  // Top-right (start as same as left)

        // Initial empty mesh
        mesh.vertices = vertices;
        mesh.triangles = new int[] { };

        // Gradually move towards the endpoint
        Vector3 currentEnd = start;
        while (currentEnd != end)
        {
            currentEnd = Vector3.MoveTowards(currentEnd, end, Time.deltaTime / drawSpeed);

            // Adjust the right vertices (for the moving endpoint)
            vertices[2] = currentEnd - perpendicular;  // Bottom-right
            vertices[3] = currentEnd + perpendicular;  // Top-right

            // Define the two triangles that form the "line" for this edge
            triangles[0] = 0; triangles[1] = 1; triangles[2] = 2;  // First triangle
            triangles[3] = 1; triangles[4] = 3; triangles[5] = 2;  // Second triangle

            // Update the mesh
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();

            yield return null;  // Wait for the next frame
        }

        // After drawing is complete, finalize the mesh for this edge
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }
}
