using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Circle : MonoBehaviour
{
    private Mesh mesh;
    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();

    public float lineThickness = 0.05f;  // Thickness of the line
    public float drawSpeed = 1.0f;  // Speed of line drawing
    public int segments = 100;  // Number of segments to approximate the circle
    public float radius = 1.0f;  // Radius of the circle

    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        // Start drawing the circle
        StartCoroutine(DrawCircle());
    }

    IEnumerator DrawCircle()
    {
        // Generate points for the circle
        Vector3[] circleVertices = GenerateCircleVertices();

        // Draw each segment of the circle progressively
        for (int i = 0; i < circleVertices.Length - 1; i++)
        {
            yield return StartCoroutine(DrawLineAndKeepPrevious(circleVertices[i], circleVertices[i + 1]));
        }

        // Close the circle by connecting the last point to the first
        yield return StartCoroutine(DrawLineAndKeepPrevious(circleVertices[circleVertices.Length - 1], circleVertices[0]));
    }

    Vector3[] GenerateCircleVertices()
    {
        Vector3[] points = new Vector3[segments + 1];  // We need one extra point to complete the circle

        for (int i = 0; i <= segments; i++)
        {
            float angle = 2 * Mathf.PI * i / segments;  // Angle for each point
            float x = Mathf.Cos(angle) * radius;        // X coordinate
            float y = Mathf.Sin(angle) * radius;        // Y coordinate
            points[i] = new Vector3(x, y, 0);           // Create the point on the circle
        }

        return points;
    }

    IEnumerator DrawLineAndKeepPrevious(Vector3 start, Vector3 end)
    {
        float progress = 0f;
        while (progress < 1.0f)
        {
            progress += Time.deltaTime * drawSpeed;
            Vector3 currentPosition = Vector3.Lerp(start, end, progress);  // Interpolate from start to end

            // Create the line segment that travels from point A to B
            Vector3 direction = (end - start).normalized;
            Vector3 perpendicular = Vector3.Cross(direction, Vector3.forward).normalized * lineThickness;

            // Create 4 vertices for this line segment (two triangles to form a rectangle)
            Vector3 v0 = start - perpendicular;  // Bottom-left of the line
            Vector3 v1 = start + perpendicular;  // Top-left of the line
            Vector3 v2 = currentPosition - perpendicular;  // Bottom-right (moving point)
            Vector3 v3 = currentPosition + perpendicular;  // Top-right (moving point)

            // Add the new vertices for the traveling line to the existing vertices list
            int startIndex = vertices.Count;
            vertices.Add(v0);
            vertices.Add(v1);
            vertices.Add(v2);
            vertices.Add(v3);

            // Define two triangles for the line segment
            triangles.Add(startIndex);
            triangles.Add(startIndex + 1);
            triangles.Add(startIndex + 2);
            triangles.Add(startIndex + 1);
            triangles.Add(startIndex + 3);
            triangles.Add(startIndex + 2);

            // Update the mesh to include the new line while keeping old lines
            UpdateMesh();

            yield return null;  // Wait for the next frame to continue drawing
        }
    }

    void UpdateMesh()
    {
        // Don't clear the mesh, keep adding to it to preserve previous lines
        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
    }
}
