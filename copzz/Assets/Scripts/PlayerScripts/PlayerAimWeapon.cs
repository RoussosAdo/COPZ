using System;
using UnityEngine;

public class PlayerAimWeapon : MonoBehaviour
{
    public event EventHandler<OnShootEventArgs> OnShoot;

    public class OnShootEventArgs : EventArgs
    {
        public Vector3 gunEndPointPosition;
        public Vector3 shootPosition;
    }

    private BulletPoolManager bulletPoolManager;

    [SerializeField] private float attackCooldown;
    private PlayerMovement playerMovement;
    private float cooldownTimer = Mathf.Infinity;

    [SerializeField] private Transform FirePoint;
    [SerializeField] private GameObject[] bullets;
    [SerializeField] private int defaultAmmo;
    [SerializeField] private int maxAmmo;
    [SerializeField] private int magazineSize = 10; // Bullets per reload
    [SerializeField] private TMPro.TextMeshProUGUI ammoText;
    [SerializeField] private TMPro.TextMeshProUGUI reloadText; // Text for reload status

    private int ammo;
    private int bulletsInReserve;

    private Transform aimTransform;
    private Transform aimGunEndPointTransform;
    private Animator aimAnimator;

    [SerializeField] private float armLength = 0.8f; // Adjustable distance from the body
    [SerializeField] private float smoothFlipSpeed = 10f; // Smooth flipping speed

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();

        aimTransform = transform.Find("Aim");
        aimAnimator = aimTransform?.GetComponent<Animator>();
        aimGunEndPointTransform = aimTransform?.Find("GunEndPointPosition");

        bulletPoolManager = FindObjectOfType<BulletPoolManager>();
        if (bulletPoolManager == null)
        {
            Debug.LogError("BulletPoolManager not found in the scene!");
        }

        // Initialize ammo and reserve bullets
        bulletsInReserve = maxAmmo - defaultAmmo;
        ammo = Mathf.Clamp(defaultAmmo, 0, magazineSize);

        if (aimTransform == null)
        {
            Debug.LogError("Aim Transform not found! Ensure there's a child named 'Aim' under the player.");
        }

        if (playerMovement == null)
        {
            Debug.LogError("PlayerMovement script not found! Ensure this is on the same GameObject.");
        }

        reloadText.SetText(""); // Clear reload text on start
    }

    private void Update()
    {
        HandleAiming();
        HandleShooting();
        ammoText.SetText(ammo + "/" + bulletsInReserve);

        // Check for reload input
        if (Input.GetKeyDown(KeyCode.R))
        {
            Reload();
        }
    }

    public void IncreaseAmmo(int increaseAmount)
    {
        bulletsInReserve += increaseAmount;
        bulletsInReserve = Mathf.Clamp(bulletsInReserve, 0, maxAmmo);
    }

    public static Vector3 GetMouseWorldPosition()
    {
        return GetMouseWorldPositionWithZ(Input.mousePosition, Camera.main);
    }

    public static Vector3 GetMouseWorldPositionWithZ(Vector3 screenPosition, Camera worldCamera)
    {
        if (worldCamera == null)
        {
            Debug.LogError("World Camera is null! Make sure a Camera is tagged as 'MainCamera'.");
            return Vector3.zero;
        }

        Vector3 worldPosition = worldCamera.ScreenToWorldPoint(screenPosition);
        worldPosition.z = 0f;
        return worldPosition;
    }

    private void HandleAiming()
    {
        if (aimTransform == null) return;

        Vector3 mousePosition = GetMouseWorldPosition();
        Vector3 aimDirection = (mousePosition - transform.position).normalized;

        float targetAngle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;

        if (playerMovement.IsFacingRight)
        {
            targetAngle = Mathf.Clamp(targetAngle, -63.64f, 39.26f);
            aimTransform.localScale = Vector3.Lerp(aimTransform.localScale, new Vector3(1, 1, 1), Time.deltaTime * smoothFlipSpeed);
        }
        else
        {
            targetAngle = Mathf.Clamp(targetAngle, 190.03f, -190.63f);
            aimTransform.localScale = Vector3.Lerp(aimTransform.localScale, new Vector3(-1, -1, -1), Time.deltaTime * smoothFlipSpeed);
        }

        aimTransform.eulerAngles = new Vector3(0, 0, targetAngle);
        aimTransform.position = transform.position + aimDirection * armLength;
    }

    private void HandleShooting()
    {
        cooldownTimer += Time.deltaTime;

        if (Input.GetMouseButtonDown(0) && cooldownTimer >= attackCooldown && playerMovement.canShoot())
        {
            if (ammo > 0)
            {
                Vector3 mousePosition = GetMouseWorldPosition();
                Attack();

                aimAnimator.SetTrigger("Shoot");

                OnShoot?.Invoke(this, new OnShootEventArgs
                {
                    gunEndPointPosition = aimGunEndPointTransform.position,
                    shootPosition = mousePosition,
                });
            }
            else
            {
                Debug.Log("Out of ammo! Reload.");
            }
        }
    }

    private void Reload()
    {
        if (ammo == magazineSize)
        {
            Debug.Log("Magazine already full!");
            return;
        }

        int bulletsNeeded = magazineSize - ammo;
        int bulletsToReload = Mathf.Min(bulletsNeeded, bulletsInReserve);

        if (bulletsToReload > 0)
        {
            ammo += bulletsToReload;
            bulletsInReserve -= bulletsToReload;

            Debug.Log($"Reloaded {bulletsToReload} bullets. Current Ammo: {ammo}/{bulletsInReserve}");
            reloadText.SetText("Reloading...");
            Invoke("ClearReloadText", 1.5f); // Simulate reload delay
        }
        else
        {
            Debug.Log("No bullets left in reserve!");
        }
    }

    private void ClearReloadText()
    {
        reloadText.SetText("");
    }

    private void Attack()
    {
        cooldownTimer = 0;

        GameObject bullet = bulletPoolManager.GetBullet();
        if (bullet != null)
        {
            bullet.transform.position = FirePoint.position;
            bullet.SetActive(true);
            bullet.GetComponent<Projectile>().SetDirection((GetMouseWorldPosition() - FirePoint.position).normalized);

            ammo--;
            Camera.main.GetComponent<CameraShake>().Shake();
        }
        else
        {
            Debug.Log("No available bullets in pool!");
        }
    }
}