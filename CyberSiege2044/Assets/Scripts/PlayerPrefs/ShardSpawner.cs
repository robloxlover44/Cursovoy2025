using UnityEngine;

public class ShardOnAnimationEnd : MonoBehaviour
{
    [SerializeField] private int shardsToAdd = 5; // Количество осколков для добавления
    [SerializeField] private string animationName = "YourAnimationName"; // Имя анимации (замените на нужное)

    private Animator animator; // Ссылка на компонент Animator
    private bool hasAddedShards = false; // Флаг, чтобы не добавлять осколки повторно

    private void Awake()
    {
        // Получаем компонент Animator
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator не найден на объекте " + gameObject.name + "! Добавьте компонент Animator.");
        }
    }

    private void Update()
    {
        // Проверяем, закончилась ли анимация
        if (!hasAddedShards && animator != null)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0); // 0 - слой анимации (обычно базовый)

            // Проверяем, соответствует ли текущая анимация нашей целевой анимации
            if (stateInfo.IsName(animationName))
            {
                // Проверяем, достигла ли анимация конца (normalizedTime >= 1)
                if (stateInfo.normalizedTime >= 1f)
                {
                    AddShards();
                    hasAddedShards = true; // Устанавливаем флаг, чтобы не повторять
                    gameObject.SetActive(false); // Отключаем объект
                    Debug.Log("Анимация " + animationName + " закончилась. Объект отключен.");
                }
            }
        }
    }

    private void AddShards()
    {
        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.AddShards(shardsToAdd);
            Debug.Log($"Добавлено {shardsToAdd} осколков. Общее количество: {PlayerDataManager.Instance.GetShards()}");
        }
        else
        {
            Debug.LogError("PlayerDataManager не найден! Убедитесь, что он инициализирован.");
        }
    }

    // Метод для отладки или ручного срабатывания (опционально)
    [ContextMenu("Add Shards Manually")]
    private void AddShardsManually()
    {
        AddShards();
    }
}