using UnityEngine;

public class AmmoPickUp : MonoBehaviour
{
    [SerializeField] private int ammo; // Amount of ammo to give

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerAimWeapon playerAimWeapon = other.GetComponent<PlayerAimWeapon>();
            if (playerAimWeapon != null)
            {
                playerAimWeapon.IncreaseAmmo(ammo);
                Destroy(gameObject); // Destroy the ammo pickup object
            }
        }
    }
}
