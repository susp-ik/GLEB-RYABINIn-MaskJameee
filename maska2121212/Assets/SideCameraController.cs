using UnityEngine;

[RequireComponent(typeof(Camera))]
public class SideCameraController : MonoBehaviour
{
    [Header("Целевой объект")]
    [SerializeField] private Transform target;
    
    [Header("Смещение камеры")]
    [SerializeField] private Vector3 offset = new Vector3(5f, 3f, 0f);
    
    [Header("Скорость следования")]
    [SerializeField] private float followSpeed = 5f;
    
    [Header("Ограничения движения")]
    [SerializeField] private bool limitX = false;
    [SerializeField] private float minX = -10f;
    [SerializeField] private float maxX = 10f;
    
    [SerializeField] private bool limitY = false;
    [SerializeField] private float minY = 1f;
    [SerializeField] private float maxY = 10f;
    
    [SerializeField] private bool limitZ = true;
    [SerializeField] private float minZ = -10f;
    [SerializeField] private float maxZ = 10f;
    
    private Camera cam;
    private Vector3 targetPosition;
    
    private void Awake()
    {
        cam = GetComponent<Camera>();
        
        // Если цель не назначена, пытаемся найти объект с тегом Player
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
        }
    }
    
    private void LateUpdate()
    {
        if (target == null) return;
        
        // Вычисляем желаемую позицию камеры
        targetPosition = target.position + offset;
        
        // Применяем ограничения по осям
        if (limitX)
        {
            targetPosition.x = Mathf.Clamp(targetPosition.x, minX, maxX);
        }
        
        if (limitY)
        {
            targetPosition.y = Mathf.Clamp(targetPosition.y, minY, maxY);
        }
        
        if (limitZ)
        {
            targetPosition.z = Mathf.Clamp(targetPosition.z, minZ, maxZ);
        }
        
        // Плавное перемещение камеры
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
        
        // Камера НЕ вращается - она всегда смотрит в одном направлении
        // Направление взгляда можно настроить в инспекторе через Rotation
    }
    
    // Метод для установки цели в рантайме
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
    
    // Метод для изменения смещения в рантайме
    public void SetOffset(Vector3 newOffset)
    {
        offset = newOffset;
    }
}

