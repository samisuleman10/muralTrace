using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Heart : MonoBehaviour
{
    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;
    public float lineThickness = 0.05f;  // Thickness of the "line"
    public float drawSpeed = 0.05f;  // Speed of drawing
    public float size = 1.0f;  // Size of the heart
    public int curveResolution = 20;  // How many small lines to use for curves

    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        // Define the vertices for the heart shape
        Vector3[] heartVertices = GenerateHeartVertices();

        // Start drawing the heart edges sequentially
        StartCoroutine(DrawHeartEdges(heartVertices));
    }

    Vector3[] GenerateHeartVertices()
    {
        List<Vector3> verticesList = new List<Vector3>();

        // Bezier curve for the top-left part of the heart
        Vector3 p0 = new Vector3(-size * 0.5f, 0, 0);  // Start point (left)
        Vector3 p1 = new Vector3(-size, size * 1.2f, 0);  // Control point (up)
        Vector3 p2 = new Vector3(0, size * 0.75f, 0);  // End point (top center)

        // Generate points along the left Bezier curve
        for (int i = 0; i <= curveResolution; i++)
        {
            float t = i / (float)curveResolution;
            Vector3 point = CalculateBezierPoint(t, p0, p1, p2);
            verticesList.Add(point);
        }

        // Bezier curve for the top-right part of the heart
        p0 = new Vector3(0, size * 0.75f, 0);  // Start point (top center)
        p1 = new Vector3(size, size * 1.2f, 0);  // Control point (up)
        p2 = new Vector3(size * 0.5f, 0, 0);  // End point (right)

        // Generate points along the right Bezier curve
        for (int i = 0; i <= curveResolution; i++)
        {
            float t = i / (float)curveResolution;
            Vector3 point = CalculateBezierPoint(t, p0, p1, p2);
            verticesList.Add(point);
        }

        // Add the sharp point at the bottom of the heart
        verticesList.Add(new Vector3(0, -size, 0));  // Bottom point

        return verticesList.ToArray();
    }

    // Function to calculate Bezier curve points
    Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;

        Vector3 point = (uu * p0) + (2 * u * t * p1) + (tt * p2);
        return point;
    }

    IEnumerator DrawHeartEdges(Vector3[] heartVertices)
    {
        // Connect the vertices to form the heart shape
        for (int i = 0; i < heartVertices.Length; i++)
        {
            Vector3 start = heartVertices[i];
            Vector3 end = heartVertices[(i + 1) % heartVertices.Length];  // Loop back to the start at the end
            yield return StartCoroutine(DrawHeartEdge(start, end));  // Draw each edge progressively
        }
    }

    IEnumerator DrawHeartEdge(Vector3 start, Vector3 end)
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
