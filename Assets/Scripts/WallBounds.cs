using UnityEngine;

public class WallBounds : MonoBehaviour
{
    public SurfaceMode surfaceMode = SurfaceMode.LeftWall;

    private BoxCollider boxCollider;

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider>();
    }

    public bool Contains(Vector3 worldPosition)
    {
        if (boxCollider == null)
            boxCollider = GetComponent<BoxCollider>();

        if (boxCollider == null)
            return false;

        Vector3 localPoint = transform.InverseTransformPoint(worldPosition);
        Vector3 center = boxCollider.center;
        Vector3 size = boxCollider.size * 0.5f;

        return
            localPoint.x >= center.x - size.x && localPoint.x <= center.x + size.x &&
            localPoint.y >= center.y - size.y && localPoint.y <= center.y + size.y &&
            localPoint.z >= center.z - size.z && localPoint.z <= center.z + size.z;
    }
}