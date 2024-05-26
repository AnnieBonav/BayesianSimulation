using System;
using System.Collections;
using UnityEngine;

public class Villain : MonoBehaviour
{
    [SerializeField] private string villainName;
    public string VillainName => villainName;
    [SerializeField] private float yPos = 0.7f;
    [SerializeField] private Agent enemy;
    public Agent Enemy => enemy;
    [Header("Camera Settings")]
    [SerializeField] private Camera cam;
    [SerializeField] float xMargin = 2f; // Distance in the x axis the villain can move beyond the screen edge.
    [SerializeField] float yMargin = 1f; // Distance in the y axis the villain can move beyond the screen edge.
    private float topCamLimit;
    private float bottomCamLimit;
    private float leftCamLimit;
    private float rightCamLimit;
    private Vector3 placeholderPosition;

    private void Awake()
    {
        placeholderPosition = new Vector3(0, yPos, 0);
    }

    private void Start()
    {
        Vector3 camLimit = cam.ViewportToWorldPoint(new Vector3(0, 0, 0));
        bottomCamLimit = camLimit.z - yMargin / 2;
        topCamLimit = Mathf.Abs(bottomCamLimit) + yMargin / 2;
        leftCamLimit = camLimit.x - xMargin / 2;
        rightCamLimit = Mathf.Abs(leftCamLimit) + xMargin / 2;

        print($"Top: {topCamLimit}, Bottom: {bottomCamLimit}, Left: {leftCamLimit}, Right: {rightCamLimit}");

        StartCoroutine(MoveLoop());
    }

    private void OnDestroy()
    {
        StopCoroutine(MoveLoop());
    }

    private void MoveToRandom()
    {
        placeholderPosition.x = UnityEngine.Random.Range(leftCamLimit, rightCamLimit);
        placeholderPosition.z = UnityEngine.Random.Range(bottomCamLimit, topCamLimit);

        iTween.MoveTo(this.gameObject, iTween.Hash("position", placeholderPosition));
    }

    IEnumerator MoveLoop()
    {
        while (true)
        {
            // Wait between 2 and 5 seconds to move again
            float randomWait = UnityEngine.Random.Range(2, 5);
            MoveToRandom();
            yield return new WaitForSeconds(randomWait);
        }
    }
}
