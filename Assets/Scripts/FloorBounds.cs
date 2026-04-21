using UnityEngine;

public class FloorBounds : MonoBehaviour
{
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

        return boxCollider.bounds.Contains(worldPosition);
    }
}