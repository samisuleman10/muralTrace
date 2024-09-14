using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Fish : MonoBehaviour
{
    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;
    public float lineThickness = 0.05f;  // Thickness of the "line"
    public float drawSpeed = 0.5f;  // Speed of drawing
    public float size = 1.0f;  // Size of the fish

    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        // Define the vertices for the fish body and tail
        Vector3[] fishVertices = GenerateFishVertices();

        // Start drawing the fish edges sequentially
        StartCoroutine(DrawFishEdges(fishVertices));
    }

    Vector3[] GenerateFishVertices()
    {
        List<Vector3> verticesList = new List<Vector3>();

        // Body of the fish (simple diamond shape)
        verticesList.Add(new Vector3(-size, 0, 0));   // Left point of the body
        verticesList.Add(new Vector3(0, size / 2, 0)); // Top point of the body
        verticesList.Add(new Vector3(size, 0, 0));    // Right point of the body
        verticesList.Add(new Vector3(0, -size / 2, 0)); // Bottom point of the body

        // Tail of the fish (simple triangle)
        verticesList.Add(new Vector3(size, 0, 0));    // Connects to body
        verticesList.Add(new Vector3(size + size * 0.5f, size * 0.5f, 0));  // Top of tail
        verticesList.Add(new Vector3(size + size * 0.5f, -size * 0.5f, 0));  // Bottom of tail

        // Close the tail by connecting back to the right point of the body
        verticesList.Add(new Vector3(size, 0, 0));  // Connect back to the body

        return verticesList.ToArray();
    }

    IEnumerator DrawFishEdges(Vector3[] fishVertices)
    {
        // First, draw the fish body (4 sides)
        for (int i = 0; i < 4; i++)
        {
            Vector3 start = fishVertices[i];
            Vector3 end = fishVertices[(i + 1) % 4];  // Connect to next vertex, and loop at the 4th side
            yield return StartCoroutine(DrawFishEdge(start, end));  // Draw each edge progressively
        }

        // Then, draw the tail (3 sides)
        for (int i = 4; i < 7; i++)
        {
            Vector3 start = fishVertices[i];
            Vector3 end = fishVertices[i + 1];
            yield return StartCoroutine(DrawFishEdge(start, end));  // Draw each edge of the tail
        }

        // Finally, close the tail by connecting it back to the body
        //yield return StartCoroutine(DrawFishEdge(fishVertices[6], fishVertices[7]));
    }

    IEnumerator DrawFishEdge(Vector3 start, Vector3 end)
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
        vertices[2] = end - perpendicular;    // Bottom-right (now moving to the end point)
        vertices[3] = end + perpendicular;    // Top-right

        // Define the triangles for the current edge
        triangles[0] = 0; triangles[1] = 1; triangles[2] = 2;  // First triangle
        triangles[3] = 1; triangles[4] = 3; triangles[5] = 2;  // Second triangle

        // Update the mesh
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        // Gradually move towards the endpoint
        Vector3 currentEnd = start;
        while (currentEnd != end)
        {
            currentEnd = Vector3.MoveTowards(currentEnd, end, Time.deltaTime / drawSpeed);

            // Update the vertices dynamically
            vertices[2] = currentEnd - perpendicular;  // Bottom-right
            vertices[3] = currentEnd + perpendicular;  // Top-right

            // Update the mesh
            mesh.vertices = vertices;
            yield return null;  // Wait for the next frame
        }
    }
}
