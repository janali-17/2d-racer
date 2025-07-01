using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;
using Playroom;

public class Player : MonoBehaviour
{
    //[SerializeField] private TextMeshProUGUI text;

    private int jump = 7;
    private Rigidbody2D rb;
    private bool _resetJumpNeeded;
    private bool isGrounded;
    public bool gameOver = false;

    public LayerMask _layerMask;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    
    public void PlayerJumpInput()
    {
        isGrounded = _IsGrounded();
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            PlayerJump();
        }
    }

    private void Restart()
    {
        if (gameOver == true)
        {
            if (Input.GetKey(KeyCode.R))
            {
                gameOver = false;
                Time.timeScale = 1.0f;
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);

            }
        }
    }
    private void PlayerJump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jump);
    }

    private bool _IsGrounded()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 0.6f, _layerMask);
        Debug.DrawRay(transform.position, Vector2.down, Color.green);
        if (hit.collider != null)
        {
          // Debug.Log("Grounded");
            if (_resetJumpNeeded == false)
            {
                return true;
            }
        }
        return false;
    }
    IEnumerator resetJumpNeeded()
    {
        _resetJumpNeeded = true;
        yield return new WaitForSeconds(1.0f);
        _resetJumpNeeded = false;
    }

    public void GameOver()
    {
        if (!gameOver) 
        {
            gameOver = true;

            var prk = GameManager.Instance.GetPlayerRoomKit();

            if (prk == null)
            {
                Debug.LogWarning("prk is null!");
                return;
            }

            prk.RpcCall("GameOver", gameOver);
            this.gameObject.SetActive(false);
        }
    }
    private void OnDestroy()
    {
       
    }
}
