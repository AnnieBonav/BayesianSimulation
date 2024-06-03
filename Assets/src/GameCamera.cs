using UnityEngine;

public class GameCamera : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private Camera cam;
    [SerializeField] float xMargin = 2f; // Distance in the x axis the villain can move beyond the screen edge.
    [SerializeField] float yMargin = 1f; // Distance in the y axis the villain can move beyond the screen edge.
    private float topCamLimit;
    public float TopCamLimit => topCamLimit;
    private float bottomCamLimit;
    public float BottomCamLimit => bottomCamLimit;
    private float leftCamLimit;
    public float LeftCamLimit => leftCamLimit;
    private float rightCamLimit;
    public float RightCamLimit => rightCamLimit;
    private float maxDistance;
    public float MaxDistance => maxDistance;

    private void Awake() {
        Vector3 camLimit = cam.ViewportToWorldPoint(new Vector3(0, 0, 0));
        bottomCamLimit = camLimit.z - yMargin / 2;
        topCamLimit = Mathf.Abs(bottomCamLimit) + yMargin / 2;
        leftCamLimit = camLimit.x - xMargin / 2;
        rightCamLimit = Mathf.Abs(leftCamLimit) + xMargin / 2;

        maxDistance = Vector3.Distance(new Vector3(leftCamLimit, 0, topCamLimit), new Vector3(rightCamLimit, 1, bottomCamLimit));
    }
}
