using UnityEngine;

public class GravitySwitch : MonoBehaviour
{
    public SurfaceMode activeOnMode = SurfaceMode.Floor;
    public SurfaceMode targetMode = SurfaceMode.RightWall;
    public Transform targetAnchor;
}