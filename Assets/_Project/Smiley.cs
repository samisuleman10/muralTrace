using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Smiley : MonoBehaviour
{
    private Mesh mesh;
    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();

    public float lineThickness = 0.05f;  // Thickness of the line
    public float drawSpeed = 1.0f;  // Speed of line drawing
    public int segments = 50;  // Number of segments for the eyes and mouth
    public float faceRadius = 1.5f;  // Radius of the face circle
    public float eyeRadius = 0.3f;  // Radius of the eyes
    public float mouthRadius = 0.8f;  // Reduced radius for the smaller mouth
    public Vector3 mouthPosition = new Vector3(0, -0.1f, 0);  // Move mouth slightly up

    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        // Start drawing the smiley face
        StartCoroutine(DrawSmiley());
    }

    IEnumerator DrawSmiley()
    {
        // Draw face circle
        Vector3[] faceVertices = GenerateCircleVertices(Vector3.zero, faceRadius, segments);
        for (int i = 0; i < faceVertices.Length - 1; i++)
        {
            yield return StartCoroutine(DrawLineAndKeepPrevious(faceVertices[i], faceVertices[i + 1]));
        }
        yield return StartCoroutine(DrawLineAndKeepPrevious(faceVertices[faceVertices.Length - 1], faceVertices[0]));

        // Draw left eye
        Vector3[] leftEyeVertices = GenerateCircleVertices(new Vector3(-0.6f, 0.5f, 0), eyeRadius, segments);
        for (int i = 0; i < leftEyeVertices.Length - 1; i++)
        {
            yield return StartCoroutine(DrawLineAndKeepPrevious(leftEyeVertices[i], leftEyeVertices[i + 1]));
        }
        yield return StartCoroutine(DrawLineAndKeepPrevious(leftEyeVertices[leftEyeVertices.Length - 1], leftEyeVertices[0]));

        // Draw right eye
        Vector3[] rightEyeVertices = GenerateCircleVertices(new Vector3(0.6f, 0.5f, 0), eyeRadius, segments);
        for (int i = 0; i < rightEyeVertices.Length - 1; i++)
        {
            yield return StartCoroutine(DrawLineAndKeepPrevious(rightEyeVertices[i], rightEyeVertices[i + 1]));
        }
        yield return StartCoroutine(DrawLineAndKeepPrevious(rightEyeVertices[rightEyeVertices.Length - 1], rightEyeVertices[0]));

        // Draw mouth (an arc facing downward, slightly smaller and moved up)
        Vector3[] mouthVertices = GenerateArcVertices(mouthPosition, mouthRadius, Mathf.PI, segments, Mathf.PI); // Start at π and end at 2π
        for (int i = 0; i < mouthVertices.Length - 1; i++)
        {
            yield return StartCoroutine(DrawLineAndKeepPrevious(mouthVertices[i], mouthVertices[i + 1]));
        }
        
        // clear the mesh after drawing the smiley face after 1s
        yield return new WaitForSeconds(2);
        mesh.Clear();
    }

    Vector3[] GenerateCircleVertices(Vector3 center, float radius, int numSegments)
    {
        Vector3[] points = new Vector3[numSegments + 1];  // One extra point to close the circle

        for (int i = 0; i <= numSegments; i++)
        {
            float angle = 2 * Mathf.PI * i / numSegments;  // Angle for each point
            float x = Mathf.Cos(angle) * radius + center.x; // X coordinate
            float y = Mathf.Sin(angle) * radius + center.y; // Y coordinate
            points[i] = new Vector3(x, y, 0);               // Create the point on the circle
        }

        return points;
    }

    Vector3[] GenerateArcVertices(Vector3 center, float radius, float arcAngle, int numSegments, float startAngle = 0)
    {
        Vector3[] points = new Vector3[numSegments + 1];  // One extra point for the arc

        for (int i = 0; i <= numSegments; i++)
        {
            float angle = startAngle + arcAngle * i / numSegments;  // Adjusted angle for the arc
            float x = Mathf.Cos(angle) * radius + center.x; // X coordinate
            float y = Mathf.Sin(angle) * radius + center.y; // Y coordinate
            points[i] = new Vector3(x, y, 0);               // Create the point on the arc
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
