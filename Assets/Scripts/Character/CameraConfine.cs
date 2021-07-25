using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraConfine : MonoBehaviour
{
    [System.Flags]
    public enum EConfinementFlags : int
    {
        Left = (1 << 1),
        Right = (1 << 2),
        Top = (1 << 3),
        Bottom = (1 << 4)
    }

    public EConfinementFlags confinementMask = EConfinementFlags.Left | EConfinementFlags.Right | EConfinementFlags.Top | EConfinementFlags.Bottom;

    private void LateUpdate()
    {
        // Ordering: [0] = Left, [1] = Right, [2] = Down, [3] = Up, [4] = Near, [5] = Far
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(Camera.main);

        // Left side of the screen
        planes[0].SameSide(GetComponent<Collider>().transform.position, Camera.main.transform.position);
        Vector3[] extremities = GetCapsuleExtremities();

        float intersection;

        if ((int)(confinementMask & EConfinementFlags.Left) != 0 && !planes[0].GetSide(extremities[0]))
        {
            Ray ray = new Ray(extremities[0], (transform.position - extremities[0]).normalized);
            planes[0].Raycast(ray, out intersection);
            transform.position += (intersection * ray.direction);
        }

        if ((int)(confinementMask & EConfinementFlags.Right) != 0 && !planes[1].GetSide(extremities[1]))
        {
            Ray ray = new Ray(extremities[1], (transform.position - extremities[1]).normalized);
            planes[1].Raycast(ray, out intersection);
            transform.position += (intersection * ray.direction);
        }

        Vector3 projectedForward = Fighter.GameplayStatics.GetProjectedForwardVector(Camera.main, transform);

        if ((int)(confinementMask & EConfinementFlags.Bottom) != 0 && !planes[2].GetSide(extremities[2]))
        {
            Ray ray = new Ray(extremities[2], projectedForward);
            if (planes[2].Raycast(ray, out intersection))
            {
                transform.position += (intersection * ray.direction);
            }
        }

        if ((int)(confinementMask & EConfinementFlags.Top) != 0 && !planes[3].GetSide(extremities[3]))
        {
            Ray ray = new Ray(extremities[3], projectedForward);
            if (planes[3].Raycast(ray, out intersection))
            {
                transform.position += (intersection * ray.direction);
            }
        }
    }

    Vector3[] GetCapsuleExtremities()
    {
        CharacterController collider = GetComponent<CharacterController>();

        // Ordering: [0] = Left, [1] = Right, [2] = Down, [3] = Up
        Vector3[] extremities = new Vector3[4];

        extremities[0] = collider.transform.position - (Camera.main.transform.right * collider.radius);
        extremities[1] = collider.transform.position + (Camera.main.transform.right * collider.radius);
        extremities[2] = collider.transform.position - (Vector3.up * (Mathf.Max(collider.radius, collider.height) / 2f));
        extremities[3] = collider.transform.position + (Vector3.up * (Mathf.Max(collider.radius, collider.height) / 2f));

        /* foreach (Vector3 point in extremities)
        {
            DebugExtension.DebugPoint(point, Color.red);
        } */

        return extremities;
    }
}
