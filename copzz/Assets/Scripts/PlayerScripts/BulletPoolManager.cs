using UnityEngine;
using System.Collections.Generic;

public class BulletPoolManager : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private int poolSize = 7;

    private List<GameObject> bulletPool;

    private void Awake()
    {
        bulletPool = new List<GameObject>();
        for (int i = 0; i < poolSize; i++)
        {
            GameObject bullet = Instantiate(bulletPrefab);
            bullet.SetActive(false);
            bulletPool.Add(bullet);
        }
    }

    public GameObject GetBullet()
    {
        foreach (GameObject bullet in bulletPool)
        {
            if (!bullet.activeInHierarchy)
            {
                // Reset bullet state before returning it
                ResetBullet(bullet);
                return bullet;
            }
        }

        return null; // No available bullets
    }

    private void ResetBullet(GameObject bullet)
    {
        bullet.transform.position = Vector3.zero; // Reset position
        bullet.transform.rotation = Quaternion.identity; // Reset rotation
        bullet.SetActive(false); // Ensure it's inactive
    }

    public void ReturnBulletToPool(GameObject bullet)
    {
        // This is called by the bullet when it "expires" (hits something or goes offscreen)
        bullet.SetActive(false);
    }
}
