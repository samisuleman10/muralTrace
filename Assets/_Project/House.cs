using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class House : MonoBehaviour
{
    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;
    public float lineThickness = 0.05f;  // Thickness of the "line"
    public float drawSpeed = 0.5f;  // Speed of drawing
    public float size = 1.0f;  // Size of the house

    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        // Define the vertices for the house (square body and triangle roof)
        Vector3[] houseVertices = GenerateHouseVertices();

        // Start drawing the house edges sequentially
        StartCoroutine(DrawHouseEdges(houseVertices));
    }

    Vector3[] GenerateHouseVertices()
    {
        List<Vector3> verticesList = new List<Vector3>();

        // Square for the body of the house (bottom-left, bottom-right, top-right, top-left)
        verticesList.Add(new Vector3(-size, -size, 0));  // Bottom-left corner
        verticesList.Add(new Vector3(size, -size, 0));   // Bottom-right corner
        verticesList.Add(new Vector3(size, size, 0));    // Top-right corner
        verticesList.Add(new Vector3(-size, size, 0));   // Top-left corner

        // Triangle for the roof (top-left, top-right, peak)
        verticesList.Add(new Vector3(-size, size, 0));   // Left corner of the roof
        verticesList.Add(new Vector3(size, size, 0));    // Right corner of the roof
        verticesList.Add(new Vector3(0, size * 1.5f, 0)); // Peak of the roof (middle top)

        return verticesList.ToArray();
    }

    IEnumerator DrawHouseEdges(Vector3[] houseVertices)
    {
        // First, draw the square body (4 sides)
        for (int i = 0; i < 4; i++)
        {
            Vector3 start = houseVertices[i];
            Vector3 end = houseVertices[(i + 1) % 4];  // Connect to the next vertex and loop at the 4th side
            yield return StartCoroutine(DrawHouseEdge(start, end));  // Draw each edge progressively
        }

        // Then, draw the roof (3 sides)
        yield return StartCoroutine(DrawHouseEdge(houseVertices[4], houseVertices[6]));  // Left roof side
        yield return StartCoroutine(DrawHouseEdge(houseVertices[6], houseVertices[5]));  // Right roof side
    }

    IEnumerator DrawHouseEdge(Vector3 start, Vector3 end)
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
