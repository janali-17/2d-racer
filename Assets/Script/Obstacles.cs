using Unity.Mathematics;
using UnityEngine;

public class Obstacles : MonoBehaviour
{
    private float speed = 2f;
    private float maxSpeed = 10.0f;
    private float acceleration = .5f;

    // Update is called once per frame
    void Update()
    {
        speed += acceleration;

        speed = Mathf.Min(speed, maxSpeed);

        transform.Translate(Vector2.left * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<Player>() != null)
        {
            Player player = other.GetComponent<Player>();
            player.GameOver();

        }
        else if (other.CompareTag("Destroyer"))
        {
            Destroy(this.gameObject);
        }
    }
}
