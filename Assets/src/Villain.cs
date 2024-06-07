using System.Collections;
using UnityEngine;

public class Villain : MonoBehaviour
{
    [SerializeField] private string villainName;
    public string VillainName => villainName;
    [SerializeField] private float yPos = 0.7f;
    [SerializeField] private Agent enemy;
    public Agent Enemy => enemy;
    [SerializeField] private bool move = true;
    
    [SerializeField] private GameCamera cam;
    private Vector3 placeholderPosition;

    private void Awake()
    {
        placeholderPosition = new Vector3(0, yPos, 0);
    }

    private void Start()
    {
        if(move) StartCoroutine(MoveLoop());
    }

    private void OnDestroy()
    {
        StopCoroutine(MoveLoop());
    }

    // Instead of move to random should probably make it go closer to perry
    private void MoveToRandom()
    {
        placeholderPosition.x = UnityEngine.Random.Range(cam.LeftCamLimit, cam.RightCamLimit);
        placeholderPosition.z = UnityEngine.Random.Range(cam.BottomCamLimit, cam.TopCamLimit);

        iTween.MoveTo(this.gameObject, iTween.Hash("position", placeholderPosition));
    }

    public void ScareAway(float minDistance, float maxDistance)
    {
        print("Scaring away " + villainName);
        // Calculate a random direction
        Vector2 randomDirection = UnityEngine.Random.insideUnitCircle.normalized;

        // Calculate a random distance between minDistance and maxDistance
        float randomDistance = UnityEngine.Random.Range(minDistance, maxDistance);

        // Calculate the new position
        Vector2 newPosition = (Vector2)this.transform.position + randomDirection * randomDistance;

        // Ensure the new position is within the camera limits
        // newPosition.x = Mathf.Clamp(newPosition.x, cam.LeftCamLimit, cam.RightCamLimit);
        // newPosition.y = Mathf.Clamp(newPosition.y, cam.BottomCamLimit, cam.TopCamLimit);

        // Move the villain to the new position
        iTween.MoveTo(this.gameObject, iTween.Hash("position", newPosition));
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
