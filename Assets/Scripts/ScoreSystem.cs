using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ScoreSystem : MonoBehaviour
{
    public static ScoreSystem instance { get; private set; }
    public Slider scoreSlider;
    [SerializeField] private int score = 10;
    public void AddScore()
    {
        scoreSlider.value += score;
    }
    private void Awake()
    {
        instance = this;
    }

    private void OnDestroy()
    {
        instance = null;
    }

    private void Start()
    {
        scoreSlider.maxValue = 100;

    }

}
