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

    private bool isReloading = false;

    void Start()
    {
        currentAmmo = magazineCapacity;
    }

    public void Fire(Vector2 direction)
    {
        if (isReloading || Time.time < nextFireTime)
            return;

        if (currentAmmo <= 0)
        {
            Debug.Log("Пусто, бро! Нужно перезарядить");
            return;
        }

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0, 0, angle);

        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, rotation);
        Projectile projScript = projectile.GetComponent<Projectile>();
        if (projScript != null)
        {
            projScript.SetDirection(direction);
        }

        currentAmmo--;
        nextFireTime = Time.time + fireRate;
    }

    public void Reload()
    {
        if (!isReloading)
            StartCoroutine(ReloadCoroutine());
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

    // Публичные методы для получения информации о патронах
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
}