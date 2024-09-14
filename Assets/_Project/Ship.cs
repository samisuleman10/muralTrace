using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Ship : MonoBehaviour
{
    private Mesh mesh;
    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();
    
    public float lineThickness = 0.05f;  // Thickness of the "line"
    public float drawSpeed = 1.0f;  // Speed of line drawing
    public float shipSize = 1.0f;  // Scale of the ship

    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        // Define the vertices for the ship, adjusted by the shipSize
        Vector3[] shipVertices = GenerateShipVertices();

        // Start drawing the ship one line at a time with old lines disappearing
        StartCoroutine(DrawShipEdges(shipVertices));
    }

    Vector3[] GenerateShipVertices()
    {
        List<Vector3> verticesList = new List<Vector3>();

        // Hull (trapezoid)
        verticesList.Add(new Vector3(-0.6f, -0.5f, 0) * shipSize);  // Bottom-left of hull
        verticesList.Add(new Vector3(0.6f, -0.5f, 0) * shipSize);   // Bottom-right of hull
        verticesList.Add(new Vector3(1.0f, 0.0f, 0) * shipSize);    // Top-right of hull (extended outward)
        verticesList.Add(new Vector3(-1.0f, 0.0f, 0) * shipSize);   // Top-left of hull (extended outward)

        // Mast (vertical line)
        verticesList.Add(new Vector3(0, 0, 0) * shipSize);          // Bottom of mast (center)
        verticesList.Add(new Vector3(0, 1.2f, 0) * shipSize);       // Top of mast

        // Left sail (triangle)
        verticesList.Add(new Vector3(0, 1.0f, 0) * shipSize);       // Top of left sail (at mast)
        verticesList.Add(new Vector3(-0.5f, 0.5f, 0) * shipSize);   // Bottom-left of left sail
        verticesList.Add(new Vector3(0, 0.5f, 0) * shipSize);       // Bottom-right of left sail

        // Right sail (triangle)
        //verticesList.Add(new Vector3(0, 1.0f, 0) * shipSize);       // Top of right sail (at mast)
        //verticesList.Add(new Vector3(0.5f, 0.5f, 0) * shipSize);    // Bottom-right of right sail
        //verticesList.Add(new Vector3(0, 0.5f, 0) * shipSize);       // Bottom-left of right sail

        // Flag (single triangle on top of mast)
        //verticesList.Add(new Vector3(0, 1.2f, 0) * shipSize);       // Top of mast (flag base)
        //verticesList.Add(new Vector3(0.3f, 1.5f, 0) * shipSize);    // Tip of flag (right corner)
        //verticesList.Add(new Vector3(0, 1.5f, 0) * shipSize);       // Left corner of flag

        return verticesList.ToArray();
    }

    IEnumerator DrawShipEdges(Vector3[] shipVertices)
    {
        // Draw each edge of the ship one line at a time with old lines disappearing
        // Hull (4 sides of trapezoid)
        yield return StartCoroutine(DrawTravelingLine(shipVertices[0], shipVertices[1]));  // Bottom edge
        yield return StartCoroutine(DrawTravelingLine(shipVertices[1], shipVertices[2]));  // Right side
        yield return StartCoroutine(DrawTravelingLine(shipVertices[2], shipVertices[3]));  // Top edge
        yield return StartCoroutine(DrawTravelingLine(shipVertices[3], shipVertices[0]));  // Left side

        // Mast (vertical line)
        yield return StartCoroutine(DrawTravelingLine(shipVertices[4], shipVertices[5]));

        // Left sail (triangle)
        yield return StartCoroutine(DrawTravelingLine(shipVertices[6], shipVertices[7]));
        yield return StartCoroutine(DrawTravelingLine(shipVertices[7], shipVertices[8]));
        yield return StartCoroutine(DrawTravelingLine(shipVertices[8], shipVertices[6]));

        // ** Clear everything after the last line is drawn **
        yield return new WaitForSeconds(1f);  // Optional: Add a delay before clearing
        ClearEverything();  // Clear the entire mesh and vertices
        
        // Right sail (triangle)
        //yield return StartCoroutine(DrawTravelingLine(shipVertices[9], shipVertices[10]));
        //yield return StartCoroutine(DrawTravelingLine(shipVertices[10], shipVertices[11]));
        //yield return StartCoroutine(DrawTravelingLine(shipVertices[11], shipVertices[9]));

        // Flag (triangle)
        //yield return StartCoroutine(DrawTravelingLine(shipVertices[12], shipVertices[13])); // Flag base to tip
        //yield return StartCoroutine(DrawTravelingLine(shipVertices[13], shipVertices[14])); // Tip to bottom corner
        //yield return StartCoroutine(DrawTravelingLine(shipVertices[14], shipVertices[12])); // Back to base
    }

    IEnumerator DrawTravelingLine(Vector3 start, Vector3 end)
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

            // Clear previous lines
            mesh.Clear();
            vertices.Clear();
            triangles.Clear();

            // Add the new vertices for the traveling line
            vertices.Add(v0);
            vertices.Add(v1);
            vertices.Add(v2);
            vertices.Add(v3);

            // Define two triangles for the line segment
            triangles.Add(0);
            triangles.Add(1);
            triangles.Add(2);
            triangles.Add(1);
            triangles.Add(3);
            triangles.Add(2);

            // Update the mesh
            UpdateMesh();

            yield return null;  // Wait for the next frame to continue drawing
        }
    }

    void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
    }
    
    void ClearEverything()
    {
        mesh.Clear();
        vertices.Clear();
        triangles.Clear();
        UpdateMesh();  // Ensure the mesh is updated and cleared
    }
}
