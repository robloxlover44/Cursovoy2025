using UnityEngine;
using System.Collections;

public class Weapon : MonoBehaviour
{
    public string weaponID;

    [Header("Ammo Settings")]
    public int magazineCapacity = 10;
    public int currentAmmo;
    public int totalAmmo = 50;
    public float reloadTime = 2f;

    [Header("Firing Settings")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float fireRate = 0.5f;
    private float nextFireTime = 0f;

    [Header("Fan Fire Settings")]
    [Tooltip("Сколько пуль вылетает веером за раз (1 = обычный выстрел)")]
    public int bulletsPerShot = 1;
    [Tooltip("Общий угол веера (например, 45 = пули разлетятся на 45° друг от друга)")]
    public float fanAngle = 30f;

    [Header("Auto Fire")]
    [Tooltip("Включить автоматический огонь при зажатой кнопке")]
    public bool holdToFire = false;

    [Header("Laser Charge Settings")]
    public bool isLaserGun = false;              // Оружие-луч?
    public float maxCharge = 100f;               // Максимальный заряд (проценты)
    public float chargeDrainPerSecond = 30f;     // Скорость расхода заряда (за 1 сек стрельбы)
    [HideInInspector] public float currentCharge = 100f;  // Текущий заряд

    private bool isReloading = false;
    private bool isFiringLaser = false;

    private Coroutine reloadCoroutine; // для отмены перезарядки

    void Start()
    {
        if (isLaserGun)
            currentCharge = maxCharge;
        else
            currentAmmo = magazineCapacity;
    }

    void Update()
    {
        // Для лазера: если стреляем — тратим заряд
        if (isLaserGun && isFiringLaser)
        {
            currentCharge -= chargeDrainPerSecond * Time.deltaTime;
            currentCharge = Mathf.Max(currentCharge, 0f);
            isFiringLaser = false;
        }
    }

    public void Fire(Vector2 direction)
    {
        if (isLaserGun)
        {
            FireLaser(direction);
            return;
        }

        if (isReloading || Time.time < nextFireTime)
            return;

        if (currentAmmo <= 0)
        {
            Debug.Log("Пусто, бро! Нужно перезарядить");
            return;
        }

        int bulletsToShoot = Mathf.Min(bulletsPerShot, currentAmmo);

        float baseAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        float startAngle = baseAngle - fanAngle * 0.5f;
        float angleStep = (bulletsPerShot > 1) ? (fanAngle / (bulletsPerShot - 1)) : 0f;

        for (int i = 0; i < bulletsToShoot; i++)
        {
            float angle = (bulletsPerShot > 1) ? startAngle + angleStep * i : baseAngle;
            Quaternion rotation = Quaternion.Euler(0, 0, angle);

            GameObject projectile = Instantiate(projectilePrefab, firePoint.position, rotation);
            Projectile projScript = projectile.GetComponent<Projectile>();
            if (projScript != null)
            {
                Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
                projScript.SetDirection(dir);
            }
        }

        currentAmmo -= bulletsToShoot;
        nextFireTime = Time.time + fireRate;
    }

    // Новый метод для лазерного оружия
    public void FireLaser(Vector2 direction)
    {
        if (isReloading || currentCharge <= 0f || Time.time < nextFireTime)
            return;

        isFiringLaser = true;
        nextFireTime = Time.time + fireRate;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0, 0, angle);

        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, rotation);
        Projectile projScript = projectile.GetComponent<Projectile>();
        if (projScript != null)
            projScript.SetDirection(direction);
    }

    public void Reload()
    {
        if (!isLaserGun && !isReloading)
        {
            reloadCoroutine = StartCoroutine(ReloadCoroutine());
        }
    }

    IEnumerator ReloadCoroutine()
    {
        isReloading = true;
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
        Debug.Log($"Перезарядка завершена: {currentAmmo} в магазине, {totalAmmo} в запасе");
    }

    // --- Новый метод для сброса релоада ---
    public void CancelReload()
    {
        if (isReloading && reloadCoroutine != null)
        {
            StopCoroutine(reloadCoroutine);
            isReloading = false;
            reloadCoroutine = null;
        }
    }

    // --- Методы для доступа из PlayerController ---
    public int GetCurrentAmmo()
    {
        return currentAmmo;
    }

    public int GetTotalAmmo()
    {
        return totalAmmo;
    }

    public bool IsReloading()
    {
        return isReloading;
    }

    public bool IsAutoFireEnabled()
    {
        return holdToFire;
    }

    public bool IsLaserGun()
    {
        return isLaserGun;
    }

    public float GetCurrentCharge()
    {
        return currentCharge;
    }

    public float GetMaxCharge()
    {
        return maxCharge;
    }

    public bool HasCharge()
    {
        return currentCharge > 0f;
    }
}