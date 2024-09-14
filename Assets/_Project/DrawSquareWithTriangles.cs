using System.Collections;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class DrawSquareWithTriangles : MonoBehaviour
{
    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;
    public float lineThickness = 0.05f;  // Thickness of the "line"
    public float drawSpeed = 0.5f;  // Speed of drawing
    public float sideLength = 2f;   // Side length of the square

    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        // Define the vertices of the square
        Vector3[] squareVertices = new Vector3[4];
        squareVertices[0] = new Vector3(0, 0, 0);                   // Bottom-left corner
        squareVertices[1] = new Vector3(sideLength, 0, 0);           // Bottom-right corner
        squareVertices[2] = new Vector3(sideLength, sideLength, 0);  // Top-right corner
        squareVertices[3] = new Vector3(0, sideLength, 0);           // Top-left corner

        // Start drawing the square edges sequentially
        StartCoroutine(DrawSquareEdges(squareVertices));
    }

    IEnumerator DrawSquareEdges(Vector3[] squareVertices)
    {
        // Draw the four edges one by one
        yield return StartCoroutine(DrawSquareEdge(squareVertices[0], squareVertices[1]));  // Bottom edge
        yield return StartCoroutine(DrawSquareEdge(squareVertices[1], squareVertices[2]));  // Right edge
        yield return StartCoroutine(DrawSquareEdge(squareVertices[2], squareVertices[3]));  // Top edge
        yield return StartCoroutine(DrawSquareEdge(squareVertices[3], squareVertices[0]));  // Left edge
    }

    IEnumerator DrawSquareEdge(Vector3 start, Vector3 end)
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
