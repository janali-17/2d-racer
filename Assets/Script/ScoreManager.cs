using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    private TextMeshProUGUI scoreText;
    private float score = 0f;
    private float scoreRate = 0.5f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
     scoreText = GetComponent<TextMeshProUGUI>();   
    }

    // Update is called once per frame
    void Update()
    {
        score += scoreRate * Time.deltaTime;
        scoreText.text = "Score: " + score.ToString("F2");
    }
}
