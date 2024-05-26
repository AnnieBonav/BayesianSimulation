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

    private void MoveToRandom()
    {
        placeholderPosition.x = UnityEngine.Random.Range(cam.LeftCamLimit, cam.RightCamLimit);
        placeholderPosition.z = UnityEngine.Random.Range(cam.BottomCamLimit, cam.TopCamLimit);

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
