using UnityEngine;

public class WaypointHeightAligner : MonoBehaviour
{
    public GameObject waypoints;
    public float heightAboveGround;

    void Awake()
    {
        foreach (Transform child in waypoints.transform)
        {
            AlignToGround(child);
        }
    }

    private void AlignToGround(Transform sphere)
    {
        Vector3 pos = sphere.position;
        Ray ray = new Ray(pos, Vector3.down);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
                pos.y = hit.point.y + heightAboveGround;
                sphere.position = pos;
        }
    }
}
