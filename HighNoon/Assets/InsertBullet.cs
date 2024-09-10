using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BNG
{
    public class InsertBullet : MonoBehaviour
    {
        public Revolver Weapon;
        public bool canInsert;
        public string AcceptBulletName = "LongBulletPivot";
        public AudioClip InsertSound;


        void OnTriggerEnter(Collider other)
        {
            if (canInsert == false)
            {
                return;
            }
            Grabbable grab = other.GetComponent<Grabbable>();
            if (grab != null)
            {
                if (grab.transform.name.Contains(AcceptBulletName))
                {

                    // Weapon is full
                    if (Weapon.GetBulletCount() >= Weapon.MaxInternalAmmo)
                    {
                        return;
                    }

                    // Drop the bullet and add ammo to gun
                    grab.DropItem(false, true);
                    grab.transform.parent = null;
                    GameObject.Destroy(grab.gameObject);

                    // Find the closest chamber
                    GameObject closestChamber = GetClosestChamber(grab.transform.position);

                    if (closestChamber != null)
                    {
                        // Insert bullet into the closest chamber
                        GameObject b = Instantiate(Weapon.bulletPrefab, closestChamber.transform.position, closestChamber.transform.rotation);
                        b.transform.parent = closestChamber.transform; // Parent to the chamber
                        b.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f); // Adjust scale if needed
                    }

                    // Play Sound
                    if (InsertSound)
                    {
                        VRUtils.Instance.PlaySpatialClipAt(InsertSound, transform.position, 1f, 0.5f);
                    }
                }
            }
        }

        // Method to find the closest chamber
        GameObject GetClosestChamber(Vector3 position)
        {
            GameObject closestChamber = null;
            float closestDistance = Mathf.Infinity;

            foreach (GameObject chamber in Weapon.chambers)
            {
                float distance = Vector3.Distance(position, chamber.transform.position);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestChamber = chamber;
                    print(chamber);
                }
            }

            return closestChamber;
        }
    }
}
