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
        if (!isMoving) return;

        // Отримуємо поточну позицію
        Vector2 currentPosition = rectTransform.anchoredPosition;
        
        // Обчислюємо напрямок до цілі
        Vector2 direction = (targetPosition - currentPosition).normalized;
        
        // Обчислюємо відстань до цілі
        float distance = Vector2.Distance(currentPosition, targetPosition);

        if (distance > arrivalThreshold)
        {
            // Рухаємо дрон до цілі
            rectTransform.anchoredPosition = Vector2.MoveTowards(
                currentPosition,
                targetPosition,
                speed * Time.deltaTime
            );

            // Обчислюємо кут для обертання
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.Euler(0, 0, angle);
            
            // Плавно обертаємо дрон
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