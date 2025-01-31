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
    private bool isReloading = false; // Track if the player is reloading



    [SerializeField] private float attackCooldown;
    private PlayerMovement playerMovement;
    private float cooldownTimer = Mathf.Infinity;
    private float lastValidAngle; // Store the last valid angle
    private bool isAiming = false; // Track if aiming

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

    private bool hasStartedAiming = false;
    private float aimStartTime = 0f;
    [SerializeField] private float aimDelay = 0.5f; // Delay before aiming starts


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
        if (!playerMovement.IsMoving())
        {
            // Enable/disable aiming when pressing the right mouse button
            if (Input.GetMouseButton(1)) // Right Mouse Button
            {
                isAiming = true;
                playerMovement.DisableMovement(); // Prevent movement while aiming
            }
            else
            {
                isAiming = false;
                playerMovement.EnableMovement(); // Re-enable movement
            }

            if (isAiming && !playerMovement.IsMoving())
            {
                if (!aimTransform.gameObject.activeSelf)
                {
                    aimTransform.gameObject.SetActive(true); // Ensure the aiming object is active before using it
                }
                HandleAiming();
                HandleShooting();
            }
            else
            {
                aimTransform.gameObject.SetActive(false); // Hide gun when not aiming
            }

            ammoText.SetText(ammo + "/" + bulletsInReserve);

        }
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
        if (!isAiming)
        {
            hasStartedAiming = false;
            aimStartTime = Time.time; // Reset the timer when not aiming
            return;
        }

        if (!hasStartedAiming)
        {
            if (Time.time - aimStartTime < aimDelay)
            {
                return; // Wait for the delay before allowing aiming
            }
            hasStartedAiming = true;
        }

        aimTransform.gameObject.SetActive(true); // Show gun only when aiming

        Vector3 mousePosition = GetMouseWorldPosition();
        Vector3 aimDirection = (mousePosition - transform.position).normalized;

        float targetAngle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;

        // Normalize angles within -180° to 180° range
        if (targetAngle > 180f) targetAngle -= 360f;
        if (targetAngle < -180f) targetAngle += 360f;

        Debug.Log("Normalized Target Angle: " + targetAngle);

        // Reset angle when changing directions
        if (playerMovement.IsFacingRight && lastValidAngle < 0)
        {
            lastValidAngle = 0; // Reset to right-facing angle
        }
        else if (!playerMovement.IsFacingRight && lastValidAngle >= 0)
        {
            lastValidAngle = 180; // Reset to left-facing angle
        }

        float clampedAngle;
        bool isValidAngle = true;

        if (playerMovement.IsFacingRight)
        {
            // Limit aiming upwards to avoid weird flips
            clampedAngle = Mathf.Clamp(targetAngle, -60f, 60f);
            aimTransform.localScale = new Vector3(1, 1, 1); // Normal gun scale

            if (targetAngle < -60f || targetAngle > 120f)
            {
                isValidAngle = false; // Out of range
            }
        }
        else
        {
            // Limit aiming downwards when facing left to avoid sudden flips
            if (targetAngle < -60f)
            {
                clampedAngle = Mathf.Clamp(targetAngle, -180f, -120f);
            }
            else
            {
                clampedAngle = Mathf.Clamp(targetAngle, 120f, 180f);
            }

            aimTransform.localScale = new Vector3(-1, -1, 1); // Flip gun

            if (targetAngle > -60f && targetAngle < 120f)
            {
                isValidAngle = false; // Out of range
            }
        }

        // ?? Only update lastValidAngle if within valid range
        if (isValidAngle)
        {
            lastValidAngle = clampedAngle;
        }
        else
        {
            Debug.Log("Angle out of range, hiding gun.");
        }

        // Hide gun if aiming outside the allowed angles
        aimTransform.gameObject.SetActive(isValidAngle);

        aimTransform.eulerAngles = new Vector3(0, 0, lastValidAngle);
        aimTransform.position = transform.position + (Quaternion.Euler(0, 0, lastValidAngle) * Vector3.right * armLength);
    }



    private void HandleShooting()
    {
        cooldownTimer += Time.deltaTime;

        // ?? Prevent shooting if reloading OR gun is hidden
        if (isReloading || !aimTransform.gameObject.activeSelf)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0) && cooldownTimer >= attackCooldown && playerMovement.canShoot())
        {
            if (ammo > 0)
            {
                Vector3 shootDirection = FirePoint.right * (playerMovement.IsFacingRight ? 1 : -1);
                Attack(shootDirection);
                aimAnimator.SetTrigger("Shoot");
                OnShoot?.Invoke(this, new OnShootEventArgs
                {
                    gunEndPointPosition = aimGunEndPointTransform.position,
                    shootPosition = aimGunEndPointTransform.position + shootDirection * 10f * (playerMovement.IsFacingRight ? 1 : -1),
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
        if (ammo == magazineSize || isReloading)
        {
            Debug.Log("Already full or currently reloading!");
            return;
        }

        int bulletsNeeded = magazineSize - ammo;
        int bulletsToReload = Mathf.Min(bulletsNeeded, bulletsInReserve);

        if (bulletsToReload > 0)
        {
            isReloading = true; // ?? Prevent shooting while reloading
            reloadText.SetText("Reloading...");
            Debug.Log("Reloading...");

            Invoke(nameof(FinishReload), 1.5f); // Simulate reload delay
        }
        else
        {
            Debug.Log("No bullets left in reserve!");
        }
    }

    // ?? Called after reload delay to restore ammo and allow shooting again
    private void FinishReload()
    {
        int bulletsNeeded = magazineSize - ammo;
        int bulletsToReload = Mathf.Min(bulletsNeeded, bulletsInReserve);

        if (bulletsToReload > 0)
        {
            ammo += bulletsToReload;
            bulletsInReserve -= bulletsToReload;
            Debug.Log($"Reloaded {bulletsToReload} bullets. Current Ammo: {ammo}/{bulletsInReserve}");
        }

        isReloading = false; // ? Allow shooting again
        reloadText.SetText(""); // Clear reload text
    }


    

    private void Attack(Vector3 direction)
    {
        cooldownTimer = 0;

        GameObject bullet = bulletPoolManager.GetBullet();
        if (bullet != null)
        {
            bullet.transform.position = FirePoint.position;
            bullet.SetActive(true);
            bullet.GetComponent<Projectile>().SetDirection(direction.normalized * (playerMovement.IsFacingRight ? 1 : -1)); // Adjusted for left-facing

            ammo--;
            Camera.main.GetComponent<CameraShake>().Shake();
        }
        else
        {
            Debug.Log("No available bullets in pool!");
        }

    }

}