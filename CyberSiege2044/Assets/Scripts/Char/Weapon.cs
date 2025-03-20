using UnityEngine;
using System.Collections;
using TMPro; // Подключаем TextMeshPro

public class Weapon : MonoBehaviour
{
    [Header("Ammo Settings")]
    public int magazineCapacity = 10;  // Патронов в магазине (x)
    public int currentAmmo;            // Текущее количество патронов в магазине
    public int totalAmmo = 50;         // Общий запас патронов (y)
    public float reloadTime = 2f;      // Время перезарядки

    [Header("Firing Settings")]
    public GameObject projectilePrefab; // Префаб пули
    public Transform firePoint;         // Точка вылета пули
    public float fireRate = 0.5f;         // Задержка между выстрелами
    private float nextFireTime = 0f;

    private bool isReloading = false;

    [Header("UI Settings")]
    public TMP_Text ammoText; // Ссылка на TextMeshPro компонент для отображения патронов

    void Start()
    {
        currentAmmo = magazineCapacity;
        UpdateAmmoUI();
    }

    // Метод для выстрела. Принимает направление выстрела.
    public void Fire(Vector2 direction)
    {
        // Если перезаряжаемся или ещё не прошло время задержки, ничего не делаем.
        if (isReloading || Time.time < nextFireTime)
            return;

        // Если патронов нет, запускаем эффект тряски текста и выходим.
        if (currentAmmo <= 0)
        {
            Debug.Log("Пусто, бро! Нужно перезарядить");
            StartCoroutine(ShakeAmmoText());
            return;
        }

        // Вычисляем угол поворота пули
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0, 0, angle);

        // Создаем пулю с нужным поворотом в точке firePoint
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, rotation);
        Projectile projScript = projectile.GetComponent<Projectile>();
        if (projScript != null)
        {
            projScript.SetDirection(direction);
        }

        currentAmmo--;
        nextFireTime = Time.time + fireRate;
        UpdateAmmoUI();
    }

    // Метод для запуска перезарядки
    public void Reload()
    {
        if (!isReloading)
            StartCoroutine(ReloadCoroutine());
    }

    IEnumerator ReloadCoroutine()
    {
        isReloading = true;
        // Выводим сообщение о перезарядке
        if (ammoText != null)
            ammoText.text = "//reloading.exe";
        Debug.Log("Перезаряжаю, бро...");
        yield return new WaitForSeconds(reloadTime);

        int ammoNeeded = magazineCapacity - currentAmmo;
        if (totalAmmo >= ammoNeeded)
        {
            currentAmmo = magazineCapacity;
            totalAmmo -= ammoNeeded;
        }
        else
        {
            currentAmmo += totalAmmo;
            totalAmmo = 0;
        }
        isReloading = false;
        UpdateAmmoUI();
        Debug.Log($"Перезарядка завершена: {currentAmmo} в магазине, {totalAmmo} в запасе");
    }

    // Обновляет текст с патронами на UI
    void UpdateAmmoUI()
    {
        if (ammoText != null)
            ammoText.text = $"{currentAmmo} / {totalAmmo}";
    }

    // Корутина для тряски текста и изменения его цвета на красный при попытке выстрела без патронов
    IEnumerator ShakeAmmoText()
    {
        if (ammoText == null)
            yield break;

        Vector3 originalPos = ammoText.transform.localPosition;
        Color originalColor = ammoText.color;
        ammoText.color = Color.red;

        float duration = 0.5f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            // Генерируем небольшое случайное смещение
            float offsetX = Random.Range(-5f, 5f);
            float offsetY = Random.Range(-5f, 5f);
            ammoText.transform.localPosition = originalPos + new Vector3(offsetX, offsetY, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }
        ammoText.transform.localPosition = originalPos;
        ammoText.color = originalColor;
    }
}
