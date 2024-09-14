using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Duck : MonoBehaviour
{
    private Mesh mesh;
    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();
    public float lineThickness = 0.05f;  // Thickness of the "line"
    public float drawSpeed = 0.5f;  // Speed of drawing
    public float size = 1.0f;  // Size of the duck

    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        // Define the vertices for the duck's body, head, wings, and feet
        Vector3[] duckVertices = GenerateDuckVertices();

        // Start drawing the duck edges sequentially
        StartCoroutine(DrawDuckEdges(duckVertices));
    }

    Vector3[] GenerateDuckVertices()
    {
        List<Vector3> verticesList = new List<Vector3>();

        // Head (simple polygon)
        verticesList.Add(new Vector3(-size * 0.1f, size * 0.9f, 0));  // Left of the head
        verticesList.Add(new Vector3(size * 0.1f, size * 0.9f, 0));   // Right of the head
        verticesList.Add(new Vector3(0, size, 0));                    // Top of the head

        // Neck (connecting head to body)
        verticesList.Add(new Vector3(-size * 0.05f, size * 0.6f, 0)); // Left of the neck
        verticesList.Add(new Vector3(size * 0.05f, size * 0.6f, 0));  // Right of the neck

        // Body (large polygon)
        verticesList.Add(new Vector3(-size * 0.4f, 0, 0));  // Left bottom of the body
        verticesList.Add(new Vector3(size * 0.4f, 0, 0));   // Right bottom of the body
        verticesList.Add(new Vector3(-size * 0.3f, size * 0.4f, 0));  // Left top of the body
        verticesList.Add(new Vector3(size * 0.3f, size * 0.4f, 0));   // Right top of the body

        // Wings (attached to the body)
        verticesList.Add(new Vector3(-size * 0.2f, size * 0.3f, 0));  // Left wing top
        verticesList.Add(new Vector3(0, size * 0.2f, 0));             // Wing center
        verticesList.Add(new Vector3(size * 0.2f, size * 0.3f, 0));   // Right wing top

        // Feet (simple triangles at the bottom)
        verticesList.Add(new Vector3(-size * 0.2f, -size * 0.1f, 0));  // Left foot left
        verticesList.Add(new Vector3(-size * 0.1f, -size * 0.2f, 0));  // Left foot right
        verticesList.Add(new Vector3(size * 0.2f, -size * 0.1f, 0));   // Right foot left
        verticesList.Add(new Vector3(size * 0.1f, -size * 0.2f, 0));   // Right foot right

        return verticesList.ToArray();
    }

    IEnumerator DrawDuckEdges(Vector3[] duckVertices)
    {
        // Draw head (3 sides)
        yield return StartCoroutine(DrawShape(duckVertices, 0, 3));

        // Draw neck (2 sides)
        yield return StartCoroutine(DrawShape(duckVertices, 3, 2));

        // Draw body (4 sides)
        yield return StartCoroutine(DrawShape(duckVertices, 5, 4));

        // Draw wings (3 sides)
        yield return StartCoroutine(DrawShape(duckVertices, 9, 3));

        // Draw feet (2 sides each)
        yield return StartCoroutine(DrawShape(duckVertices, 12, 2));  // Left foot
        yield return StartCoroutine(DrawShape(duckVertices, 14, 2));  // Right foot
    }

    IEnumerator DrawShape(Vector3[] verticesArray, int startIndex, int numSides)
    {
        for (int i = 0; i < numSides; i++)
        {
            Vector3 start = verticesArray[startIndex + i];
            Vector3 end = verticesArray[startIndex + (i + 1) % numSides + startIndex];
            yield return StartCoroutine(DrawDuckEdge(start, end));
        }
    }

    IEnumerator DrawDuckEdge(Vector3 start, Vector3 end)
    {
        float distance = Vector3.Distance(start, end);
        Vector3 direction = (end - start).normalized;
        Vector3 perpendicular = Vector3.Cross(direction, Vector3.forward).normalized * lineThickness;

        // Add vertices for this line
        Vector3 v0 = start - perpendicular;
        Vector3 v1 = start + perpendicular;
        Vector3 v2 = end - perpendicular;
        Vector3 v3 = end + perpendicular;

        int startIndex = vertices.Count; // Starting index for this set of vertices

        vertices.Add(v0);
        vertices.Add(v1);
        vertices.Add(v2);
        vertices.Add(v3);

        // Define two triangles for this edge
        triangles.Add(startIndex);
        triangles.Add(startIndex + 1);
        triangles.Add(startIndex + 2);
        triangles.Add(startIndex + 1);
        triangles.Add(startIndex + 3);
        triangles.Add(startIndex + 2);

        // Update the mesh
        UpdateMesh();

        // Gradually move towards the endpoint
        float step = 0;
        while (step < 1.0f)
        {
            step += Time.deltaTime / drawSpeed;
            yield return null;  // Wait until the next frame
        }
    }

    void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
    }
}
