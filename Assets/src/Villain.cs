using UnityEngine;

public class Villain : MonoBehaviour
{
    [SerializeField] private string villainName;
    public string VillainName => villainName;

    [SerializeField] private Agent enemy;
    public Agent Enemy => enemy;
}
