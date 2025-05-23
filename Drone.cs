using UnityEngine;
using UnityEngine.UI;

public class Drone : MonoBehaviour
{
    private RectTransform rectTransform;
    private Vector2 targetPosition;
    private float speed = 500f; // Збільшена швидкість руху
    private float rotationSpeed = 720f; // Збільшена швидкість обертання
    private bool isMoving = true;
    private float arrivalThreshold = 5f; // Поріг прибуття до цілі

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