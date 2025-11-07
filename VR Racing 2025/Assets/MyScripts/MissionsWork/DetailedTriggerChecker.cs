using UnityEngine;

public class DetailedTriggerChecker
{
    public bool IsObjectCompletelyInsideTrigger(Collider objectCollider, Collider triggerCollider)
    {
        Bounds objectBounds = objectCollider.bounds;

        Vector3[] cornerPoints = GetBoundsCorners(objectBounds);

        foreach (Vector3 corner in cornerPoints)
        {
            // Если хотя бы один угол СНАРУЖИ - объект не полностью внутри
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
            case SphereCollider sphereCollider:
                return IsPointInsideSphere(point, sphereCollider);
            default:
                // Для MeshCollider
                return IsPointInsideGeneric(point, collider);
        }
    }

    private bool IsPointInsideBox(Vector3 worldPoint, BoxCollider boxCollider)
    {
        Vector3 localPoint = boxCollider.transform.InverseTransformPoint(worldPoint);

        Bounds localBounds = new Bounds(boxCollider.center, boxCollider.size);

        return localBounds.Contains(localPoint);
    }

    private bool IsPointInsideSphere(Vector3 worldPoint, SphereCollider sphereCollider)
    {        
        Vector3 sphereCenter = sphereCollider.transform.TransformPoint(sphereCollider.center);

        float maxScale = GetMaxScale(sphereCollider.transform);
        float worldRadius = sphereCollider.radius * maxScale;

        float distance = Vector3.Distance(worldPoint, sphereCenter);
        return distance <= worldRadius;
    }

    private float GetMaxScale(Transform transform)
    {
        Vector3 lossyScale = transform.lossyScale; // Глобальный масштаб
        return Mathf.Max(lossyScale.x, Mathf.Max(lossyScale.y, lossyScale.z));
    }

    bool IsPointInsideGeneric(Vector3 point, Collider collider)
    {
        // Создаем очень маленькую сферу в точке проверк и смотрим, пересекается ли она с нашим коллайдером
        Collider[] results = new Collider[5];
        float tinyRadius = 0.001f; // Очень маленький радиус
        
        int numColliders = Physics.OverlapSphereNonAlloc(point, tinyRadius, results);

        // Проверяем все коллайдеры, которые пересеклись с нашей сферой
        for (int i = 0; i < numColliders; i++)
        {
            if (results[i] == collider)
            {
                return true; // Нашли наш коллайдер - точка внутри
            }
        }

        return false; // Не нашли - точка снаружи
    }
}
