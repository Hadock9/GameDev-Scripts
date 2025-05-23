using UnityEngine;
using UnityEngine.UI;

public class Drone : MonoBehaviour
{
    private RectTransform rectTransform;
    private Vector2 targetPosition;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void Initialize(Vector2 startPosition, Vector2 target)
    {
        rectTransform.anchoredPosition = startPosition;
        targetPosition = target;
    }

    private void Update()
    {
    }
} 