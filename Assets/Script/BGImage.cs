using UnityEngine;

public class BGImage : MonoBehaviour
{
    [SerializeField] private float scrollSpeed = 2f;
    private Vector3 startPosition;
    private float repeatWidth;

    void Start()
    {
        startPosition = transform.position;

        // Get the width of the sprite in world units
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        repeatWidth = spriteRenderer.bounds.size.x;
    }

    void Update()
    {
        transform.Translate(Vector3.left * scrollSpeed * Time.deltaTime);

        if (transform.position.x < startPosition.x - repeatWidth)
        {
            // Reset position to loop
            transform.position = startPosition;
        }
    }
}
