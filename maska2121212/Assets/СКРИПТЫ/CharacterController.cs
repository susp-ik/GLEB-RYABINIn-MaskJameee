using UnityEngine;

[RequireComponent(typeof(UnityEngine.CharacterController))]
[RequireComponent(typeof(Animator))]
public class PlayerCharacterController : MonoBehaviour
{
    [Header("Движение")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private bool invertMovementAxis = false; // Если W идет не туда, попробуйте изменить это
    
    [Header("Прыжок")]
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private float gravity = -9.81f;
    
    [Header("Проверка земли")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.4f;
    [SerializeField] private LayerMask groundMask;
    
    [Header("Анимации")]
    [SerializeField] private string idleAnimationName = "Idle";
    [SerializeField] private string walkAnimationName = "Walk";
    [SerializeField] private string jumpAnimationName = "Jump";
    [SerializeField] private string fallAnimationName = "Fall";
    [SerializeField] private bool debugAnimations = false; // Включить отладку анимаций
    
    private UnityEngine.CharacterController controller;
    private Animator animator;
    private Vector3 velocity;
    private bool isGrounded;
    
    // Параметры аниматора
    private int isMovingHash;
    private int isJumpingHash;
    private int isFallingHash;
    private int speedHash;
    
    private void Awake()
    {
        controller = GetComponent<UnityEngine.CharacterController>();
        animator = GetComponent<Animator>();
        
        // Инициализация хешей параметров аниматора
        isMovingHash = Animator.StringToHash("IsMoving");
        isJumpingHash = Animator.StringToHash("IsJumping");
        isFallingHash = Animator.StringToHash("IsFalling");
        speedHash = Animator.StringToHash("Speed");
        
        // Проверка параметров аниматора при старте
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            CheckAnimatorParameters();
        }
        else if (debugAnimations)
        {
            Debug.LogWarning("Animator Controller не назначен на объекте!");
        }
        
        // Создаем точку проверки земли, если она не назначена
        if (groundCheck == null)
        {
            GameObject groundCheckObj = new GameObject("GroundCheck");
            groundCheckObj.transform.SetParent(transform);
            groundCheckObj.transform.localPosition = new Vector3(0, -controller.height / 2, 0);
            groundCheck = groundCheckObj.transform;
        }
    }
    
    private void CheckAnimatorParameters()
    {
        if (animator == null || animator.runtimeAnimatorController == null) return;
        
        bool hasIsMoving = false;
        bool hasIsJumping = false;
        bool hasIsFalling = false;
        bool hasSpeed = false;
        
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.nameHash == isMovingHash) hasIsMoving = true;
            if (param.nameHash == isJumpingHash) hasIsJumping = true;
            if (param.nameHash == isFallingHash) hasIsFalling = true;
            if (param.nameHash == speedHash) hasSpeed = true;
        }
        
        if (debugAnimations)
        {
            Debug.Log($"Параметры аниматора: IsMoving={hasIsMoving}, IsJumping={hasIsJumping}, IsFalling={hasIsFalling}, Speed={hasSpeed}");
            
            if (!hasIsMoving) Debug.LogError("Параметр 'IsMoving' (Bool) не найден в Animator Controller!");
            if (!hasIsJumping) Debug.LogError("Параметр 'IsJumping' (Bool) не найден в Animator Controller!");
            if (!hasIsFalling) Debug.LogError("Параметр 'IsFalling' (Bool) не найден в Animator Controller!");
            if (!hasSpeed) Debug.LogError("Параметр 'Speed' (Float) не найден в Animator Controller!");
        }
    }
    
    private void Update()
    {
        // Проверка земли
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        
        // Сброс скорости падения при касании земли
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        
        // Получение ввода
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        
        // Определение направления движения в мировых координатах
        // W (vertical = 1) = вперед, S (vertical = -1) = назад
        // A (horizontal = -1) = влево, D (horizontal = 1) = вправо
        Vector3 inputDirection = new Vector3(horizontal, 0f, vertical);
        
        if (invertMovementAxis)
        {
            // Альтернативная ориентация: меняем местами X и Z
            inputDirection = new Vector3(vertical, 0f, -horizontal);
        }
        
        inputDirection.Normalize();
        float moveMagnitude = inputDirection.magnitude;
        
        // Поворот и движение персонажа
        if (moveMagnitude > 0.1f)
        {
            // Вычисляем целевой поворот в направлении ввода
            Quaternion targetRotation = Quaternion.LookRotation(inputDirection, Vector3.up);
            // Плавный поворот к целевому углу
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            
            // Движение персонажа в направлении, куда он смотрит (после поворота)
            Vector3 moveDirection = transform.forward * moveMagnitude;
            controller.Move(moveDirection * moveSpeed * Time.deltaTime);
        }
        
        // Прыжок
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
        }
        
        // Применение гравитации
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
        
        // Обновление анимаций
        UpdateAnimations(moveMagnitude);
    }
    
    private void UpdateAnimations(float moveMagnitude)
    {
        // Проверка наличия аниматора
        if (animator == null)
        {
            if (debugAnimations) Debug.LogWarning("Animator не найден!");
            return;
        }
        
        if (!animator.enabled)
        {
            if (debugAnimations) Debug.LogWarning("Animator отключен!");
            return;
        }
        
        // Обновление параметров аниматора
        bool isMoving = moveMagnitude > 0.1f;
        bool isJumping = !isGrounded && velocity.y > 0;
        bool isFalling = !isGrounded && velocity.y < 0;
        
        // Отладка
        if (debugAnimations)
        {
            Debug.Log($"Move Magnitude: {moveMagnitude}, IsMoving: {isMoving}, IsGrounded: {isGrounded}, IsJumping: {isJumping}, IsFalling: {isFalling}");
        }
        
        // Устанавливаем параметры аниматора
        // Убедитесь, что эти параметры существуют в Animator Controller:
        // IsMoving (Bool), IsJumping (Bool), IsFalling (Bool), Speed (Float)
        try
        {
            animator.SetBool(isMovingHash, isMoving);
            animator.SetBool(isJumpingHash, isJumping);
            animator.SetBool(isFallingHash, isFalling);
            animator.SetFloat(speedHash, moveMagnitude);
        }
        catch (System.Exception e)
        {
            if (debugAnimations)
            {
                Debug.LogError($"Ошибка при установке параметров аниматора: {e.Message}");
                Debug.LogError("Убедитесь, что в Animator Controller созданы параметры: IsMoving (Bool), IsJumping (Bool), IsFalling (Bool), Speed (Float)");
            }
        }
    }
    
    // Публичные методы для изменения параметров
    public void SetMoveSpeed(float speed)
    {
        moveSpeed = speed;
    }
    
    public void SetJumpForce(float force)
    {
        jumpForce = force;
    }
    
    // Геттеры для получения текущих значений
    public float GetMoveSpeed() => moveSpeed;
    public float GetJumpForce() => jumpForce;
    
    private void OnDrawGizmosSelected()
    {
        // Визуализация области проверки земли в редакторе
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        }
    }
}

