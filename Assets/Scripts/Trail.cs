using System;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class Trail : MonoBehaviour
{
    const int MaxSegments = 300;

    [SerializeField] bool updateInEditor = true;
    [SerializeField] float maxSegmentLength = 1f;
    [SerializeField] float width = 1f;
    [SerializeField][Range(.05f, 1.5f)] float spacing = 1f;
    [SerializeField] float tiling = 1f;
    [SerializeField][Range(5, 25)] int roundCornersCount = 8;
    [SerializeField] MeshRenderer meshR;
    [SerializeField] MeshFilter meshF;
    [SerializeField] ParticleSystem particles;

    List<Vector3> points = new List<Vector3>(MaxSegments);

    Car car;
    Transform plane;
    GameObject linkedParking;
    Vector3 pressedPoint = Vector3.zero;
    bool parkingFound = false;
    bool splineEnded = false;
    public bool IsSplineEnded => splineEnded;

    public void Init(Car car, Transform plane, GameObject linkedParking)
    {
        this.car = car;
        this.plane = plane;
        this.linkedParking = linkedParking;
        particles.Stop();
    }

    public void Back()
    {
        pressedPoint = Vector3.zero;
        parkingFound = false;
        splineEnded = false;
        points.Clear();
        particles.Stop();
        meshF.sharedMesh = new Mesh();
    }

    public void Begin()
    {
        Back();

        pressedPoint = car.transform.position.Y(plane.position.y);
        points.Add(pressedPoint);
        particles.Play();
    }

    public void Continue()
    {
        pressedPoint = car.transform.position.Y(plane.position.y);
        points.Add(pressedPoint);
        particles.Play();
    }

    public void Modify(Vector3 hitPoint)
    {
        if (parkingFound || points.Count > MaxSegments)
            return;

        particles.transform.position = hitPoint;

        if (points.Count == 0)
        {
            if (Vector3.Distance(pressedPoint, hitPoint) >= maxSegmentLength)
            {
                points.Add(hitPoint);
                UpdateTrail();
            }
        }
        else
        {
            if (Vector3.Distance(points[points.Count - 1], hitPoint) >= maxSegmentLength)
            {
                points.Add(hitPoint);
                UpdateTrail();
            }
        }
    }

    public void ParkingReached(Vector3 hitPoint, GameObject parking)
    {
        if (parkingFound || linkedParking.GetInstanceID() != parking.GetInstanceID())
            return;

        particles.Stop();
        parkingFound = true;
        points.Add(hitPoint);
        points.Add(parking.transform.position.Y(plane.position.y));
        UpdateTrail();
    }

    public void End(Vector3 hitPoint)
    {
        splineEnded = true;

        particles.Stop();
        if (!parkingFound)
        {
            points.Add(hitPoint);
        }
        car.MoveAlongPoints(points, parkingFound);
    }

    public void End()
    {
        splineEnded = true;

        Move();
    }

    public void Move()
    {
        particles.Stop();
        car.MoveAlongPoints(points, parkingFound);
    }

    void UpdateTrail()
    {
        var array = new Vector3[points.Count];
        for (int i = 0; i < array.Length; i++)
        {
            array[i] = points[i].Y(plane.position.y);
        }

        meshF.sharedMesh = UpdateMesh(array);

        int textureRepeat = Mathf.RoundToInt(tiling * array.Length * spacing * .05f);
        meshR.sharedMaterial.mainTextureScale = new Vector2(1, textureRepeat);
    }

    Mesh UpdateMesh(Vector3[] points)
    {
        Vector3[] verts = new Vector3[(points.Length * 2) + ((roundCornersCount + 1) * 2)];
        Vector2[] uvs = new Vector2[verts.Length];
        int numTris = 2 * ((points.Length - 1) + (roundCornersCount + 1));
        int[] tris = new int[numTris * 3];
        int vertIndex = 0;
        int triIndex = 0;

        verts[vertIndex] = points[0];
        int vertIndexPrimary = vertIndex++;

        for (int i = 0; i < roundCornersCount; i++)
        {
            float t = i / (float) (roundCornersCount - 1);
            Vector3 direction = Vector3.zero;
            if (points.Length > 1)
                direction = (points[1] - points[0]).normalized;
            float angRad = t * Mathf.PI + Mathf.Atan2(-direction.z, direction.x) + (90 * Mathf.Deg2Rad);

            Vector3 point = new Vector3(Mathf.Cos(angRad), 0, -Mathf.Sin(angRad));
            verts[vertIndex] = points[0] + point * width * .5f;

                // float completionPercent = i / (float)(points.Length - 1);
            uvs[vertIndex] = new Vector2(0, i);
            // uvs[vertIndex + 1] = new Vector2(1, completionPercent);

            if (i < roundCornersCount - 1)
            {
                tris[triIndex] = vertIndexPrimary;
                tris[triIndex + 1] = vertIndex;
                tris[triIndex + 2] = (vertIndex + 1) % verts.Length;
            }

            vertIndex += 1;
            triIndex += 3;
        }

        for (int i = 0; i < points.Length; i++)
        {
            Vector3 direction = Vector3.zero;
            if (i < points.Length - 1)
            {
                direction += points[(i + 1) % points.Length] - points[i];
            }

            if (i > 0)
            {
                direction += points[i] - points[(i - 1 + points.Length) % points.Length];
            }

            direction.Normalize();

            Vector3 side = Vector3.Cross(direction, Vector3.up).normalized;

            verts[vertIndex] = points[i] + side * width * .5f;
            verts[vertIndex + 1] = points[i] - side * width * .5f;
            

            float completionPercent = i / (float)(points.Length - 1);
            //                float v = 1 - Mathf.Abs(2 * completionPercent - 1);
            uvs[vertIndex] = new Vector2(0, completionPercent);
            uvs[vertIndex + 1] = new Vector2(1, completionPercent);

            tris[triIndex] = vertIndex;
            tris[triIndex + 1] = (vertIndex + 2) % verts.Length;
            tris[triIndex + 2] = vertIndex + 1;

            tris[triIndex + 3] = vertIndex + 1;
            tris[triIndex + 4] = (vertIndex + 2) % verts.Length;
            tris[triIndex + 5] = (vertIndex + 3) % verts.Length;

            vertIndex += 2;
            triIndex += 6;
        }

        verts[vertIndex] = points[points.Length - 1];
        vertIndexPrimary = vertIndex++;

        for (int i = 0; i < roundCornersCount; i++)
        {
            float t = i / (float)(roundCornersCount-1);
            Vector3 direction = Vector3.zero;
            if (points.Length > 1)
                direction = (points[points.Length-2] - points[points.Length-1]).normalized;
            float angRad = t * Mathf.PI + Mathf.Atan2(-direction.z, direction.x) + (90 * Mathf.Deg2Rad);

            Vector3 point = new Vector3(Mathf.Cos(angRad), 0, -Mathf.Sin(angRad));
            verts[vertIndex] = points[points.Length-1] + point * width * .5f;

            // float completionPercent = i / (float)(points.Length - 1);
            uvs[vertIndex] = new Vector2(0, i);
            // uvs[vertIndex + 1] = new Vector2(1, completionPercent);

            if (i < roundCornersCount-1)
            {
                tris[triIndex] = vertIndexPrimary;
                tris[triIndex + 1] = vertIndex;
                tris[triIndex + 2] = (vertIndex + 1) % verts.Length;
            }

            vertIndex += 1;
            triIndex += 3;
        }

        Mesh mesh = new Mesh();
        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.uv = uvs;

        return mesh;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        for (int i = 0; i < points.Count; i++)
        {
            Gizmos.DrawSphere(points[i], 0.1f);
        }

        if (updateInEditor)
            UpdateTrail();
    }
}
