using UnityEngine;

public class DetailedTriggerChecker
{
    public bool IsObjectCompletelyInsideTrigger(Collider objectCollider, Collider triggerCollider)
    {
        Bounds objectBounds = objectCollider.bounds;

        Vector3[] cornerPoints = GetBoundsCorners(objectBounds);

        foreach (Vector3 corner in cornerPoints)
        {
            // если хот€ бы один угол —Ќј–”∆» - объект не полностью внутри
            if (!IsPointInsideCollider(corner, triggerCollider))
            {
                return false;
            }
        }

        return true;
    }

    public bool IsObjectsCompletelyInsideTrigger(Collider[] objectColliders, Collider triggerCollider)
    {
        foreach (Collider objectCollider in objectColliders)
        {
            if (!IsObjectCompletelyInsideTrigger(objectCollider, triggerCollider))
            {
                return false;
            }
        }
        return true;
    }

    private Vector3[] GetBoundsCorners(Bounds bounds)
    {
        Vector3[] corners = new Vector3[8];

        Vector3 center = bounds.center;
        Vector3 extents = bounds.extents;

        corners[0] = center + new Vector3(-extents.x, -extents.y, -extents.z);
        corners[1] = center + new Vector3(-extents.x, -extents.y, extents.z);
        corners[2] = center + new Vector3(-extents.x, extents.y, -extents.z);
        corners[3] = center + new Vector3(-extents.x, extents.y, extents.z);
        corners[4] = center + new Vector3(extents.x, -extents.y, -extents.z);
        corners[5] = center + new Vector3(extents.x, -extents.y, extents.z);
        corners[6] = center + new Vector3(extents.x, extents.y, -extents.z);
        corners[7] = center + new Vector3(extents.x, extents.y, extents.z);

        return corners;
    }

    private bool IsPointInsideCollider(Vector3 point, Collider collider)
    {
        switch (collider)
        {
            case BoxCollider boxCollider:
                return IsPointInsideBox(point, boxCollider);
            default:
                return false;
        }
    }

    private bool IsPointInsideBox(Vector3 worldPoint, BoxCollider boxCollider)
    {
        Vector3 localPoint = boxCollider.transform.InverseTransformPoint(worldPoint);

        Bounds localBounds = new Bounds(boxCollider.center, boxCollider.size);

        return localBounds.Contains(localPoint);
    }   
}
