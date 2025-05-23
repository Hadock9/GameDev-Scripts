using UnityEngine;
using UnityEngine.UI;

public class Drone : MonoBehaviour
{
    private RectTransform rectTransform;
    private Vector2 targetPosition;
    private float speed = 300f; // Збільшена швидкість
    private float rotationSpeed = 360f;
    private bool isMoving = true;

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
        if (!isMoving) return;

        // Рухаємо дрон до цілі
        Vector2 currentPosition = rectTransform.anchoredPosition;
        Vector2 direction = (targetPosition - currentPosition).normalized;
        float distance = Vector2.Distance(currentPosition, targetPosition);

        if (distance > 1f)
        {
            // Рух
            rectTransform.anchoredPosition = Vector2.MoveTowards(
                currentPosition,
                targetPosition,
                speed * Time.deltaTime
            );

            // Обертання
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.Euler(0, 0, angle);
            rectTransform.rotation = Quaternion.RotateTowards(
                rectTransform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
        else
        {
            // Дрон досяг цілі
            isMoving = false;
            rectTransform.anchoredPosition = targetPosition;
            rectTransform.rotation = Quaternion.identity;
        }
    }
} 