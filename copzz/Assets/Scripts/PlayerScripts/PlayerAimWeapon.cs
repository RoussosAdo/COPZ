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

    // For Shooting
    [SerializeField] private float attackCooldown;
    private PlayerMovement playerMovement;
    private float cooldownTimer = Mathf.Infinity;
    [SerializeField]
    private Transform FirePoint;
    [SerializeField]
    private GameObject[] bullets;

    private Transform aimTransform;
    private Transform aimGunEndPointTransform;
    private Animator aimAnimator;

    [SerializeField] private float armLength = 0.8f; // Adjustable distance from the body (closer now)
    [SerializeField] private float smoothFlipSpeed = 10f; // Smooth flipping speed

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();

        aimTransform = transform.Find("Aim");
        aimAnimator = aimTransform?.GetComponent<Animator>();
        aimGunEndPointTransform = aimTransform?.Find("GunEndPointPosition");

        if (aimTransform == null)
        {
            Debug.LogError("Aim Transform not found! Ensure there's a child named 'Aim' under the player.");
        }

        if (playerMovement == null)
        {
            Debug.LogError("PlayerMovement script not found! Ensure this is on the same GameObject.");
        }
    }

    private void Update()
    {
        HandleAiming();
        HandleShooting();
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

        // Get the mouse position and calculate the direction
        Vector3 mousePosition = GetMouseWorldPosition();
        Vector3 aimDirection = (mousePosition - transform.position).normalized;

        // Calculate the rotation angle
        float targetAngle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;

        if (playerMovement.IsFacingRight) // Facing right
        {
            // Apply angle limits for facing right
            targetAngle = Mathf.Clamp(targetAngle, -63.64f, 39.26f);
            aimTransform.localScale = Vector3.Lerp(aimTransform.localScale, new Vector3(1, 1, 1), Time.deltaTime * smoothFlipSpeed);
        }
        else // Facing left
        {
            // Apply angle limits for facing left
            targetAngle = Mathf.Clamp(targetAngle, 190.03f, -190.63f);
            aimTransform.localScale = Vector3.Lerp(aimTransform.localScale, new Vector3(-1, -1, -1), Time.deltaTime * smoothFlipSpeed); // Flip upside down
        }

        // Apply the rotation to the gun
        aimTransform.eulerAngles = new Vector3(0, 0, targetAngle);

        // Position the gun at the specified arm length from the player's body, based on the mouse direction
        aimTransform.position = transform.position + aimDirection * armLength;
    }

    private void HandleShooting()
    {
        // Increment the cooldown timer every frame
        cooldownTimer += Time.deltaTime;

        // Check if the player can shoot
        if (Input.GetMouseButtonDown(0) && cooldownTimer >= attackCooldown && playerMovement.canShoot())
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
    }


    private void Attack()
    {
        cooldownTimer = 0;

        // Loop through bullets to find an inactive one
        foreach (GameObject bullet in bullets)
        {
            if (!bullet.activeInHierarchy) // Use an inactive bullet
            {
                bullet.transform.position = FirePoint.position; // Spawn at FirePoint

                // Determine the direction: +1 for right, -1 for left
                float direction = playerMovement.IsFacingRight ? 1f : -1f;

                // Activate the bullet and apply force
                bullet.SetActive(true);
                bullet.GetComponent<Projectile>().SetDirection(direction);
                break; // Stop after firing one bullet
            }
        }
    }


}
