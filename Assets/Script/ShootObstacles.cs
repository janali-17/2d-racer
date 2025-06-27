using UnityEngine;
using System.Collections;

public class ShootObstacles : MonoBehaviour
{
    [SerializeField] private GameObject obstaclePrefab;
    [SerializeField] private float initialMinDelay = 3f;
    [SerializeField] private float initialMaxDelay = 5f;
    [SerializeField] private float minPossibleDelay = 0.5f;
    [SerializeField] private float speedUpRate = 0.1f; // Decrease delay by this amount every spawn

    private float currentMinDelay;
    private float currentMaxDelay;

    private void Start()
    {
        currentMinDelay = initialMinDelay;
        currentMaxDelay = initialMaxDelay;
        StartCoroutine(Shoot());
    }

    IEnumerator Shoot()
    {
        while (true)
        {
            float delay = Random.Range(currentMinDelay, currentMaxDelay);
            yield return new WaitForSeconds(delay);

            Instantiate(obstaclePrefab, transform.position, Quaternion.identity);

            // Speed up shooting by reducing delays
            currentMinDelay = Mathf.Max(minPossibleDelay, currentMinDelay - speedUpRate);
            currentMaxDelay = Mathf.Max(minPossibleDelay + 0.2f, currentMaxDelay - speedUpRate); // ensure maxDelay > minDelay
        }
    }
}
