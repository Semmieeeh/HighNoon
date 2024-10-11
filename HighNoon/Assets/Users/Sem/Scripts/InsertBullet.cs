using System.Collections;
using System.Collections.Generic;
using TMPro;
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

                    if (grab.GetComponent<Bullet>().fired == true || grab.GetComponent<Bullet>().canBeLoaded == false)
                    {
                        return;
                    }
                    grab.DropItem(false, true);
                    grab.transform.parent = null;
                    GameObject.Destroy(grab.gameObject);

                    // Find the closest available chamber
                    GameObject closestChamber = GetClosestAvailableChamber(grab.transform.position);

                    if (closestChamber != null)
                    {
                        // Insert bullet into the closest available chamber
                        GameObject b = Instantiate(Weapon.bulletPrefab, closestChamber.transform.position, closestChamber.transform.rotation);
                        b.transform.parent = closestChamber.transform; // Parent to the chamber
                        b.transform.localScale = new Vector3(0.1f * 12, 0.1f * 12, 0.13f * 12); // Adjust scale if needed
                        b.GetComponent<Bullet>().canBeLoaded = false;

                        Weapon.bullets[int.Parse(closestChamber.name) - 1] = b.gameObject.GetComponent<Bullet>();
                    }

                    // Play Sound
                    if (InsertSound)
                    {
                        VRUtils.Instance.PlaySpatialClipAt(InsertSound, transform.position, 1f, 0.5f);
                    }

                }
            }
        }

        // Method to find the closest available chamber
        GameObject GetClosestAvailableChamber(Vector3 position)
        {
            List<GameObject> chambers = new List<GameObject>(Weapon.chambers);
            GameObject closestChamber = null;
            float closestDistance = Mathf.Infinity;

            // Check if all chambers are full
            bool allChambersFull = true;
            foreach (GameObject chamber in chambers)
            {
                if (chamber.transform.childCount == 0) // Check for empty chambers
                {
                    allChambersFull = false;
                    break;
                }
            }

            // If all chambers are full, return null early
            if (allChambersFull)
            {
                Debug.Log("All chambers are full. Cannot insert any more bullets.");
                return null;
            }

            // Continue to find the closest available chamber
            while (chambers.Count > 0)
            {
                foreach (GameObject chamber in chambers)
                {
                    // Check if chamber already has a bullet (child object)
                    if (chamber.transform.childCount == 0)
                    {
                        float distance = Vector3.Distance(position, chamber.transform.position);

                        if (distance < closestDistance)
                        {
                            closestDistance = distance;
                            closestChamber = chamber;
                        }
                    }
                }

                // If we found a chamber with no child, return it
                if (closestChamber != null)
                {
                    return closestChamber;
                }
                else
                {
                    // Remove the closestChamber from the list to avoid looping forever
                    chambers.Remove(closestChamber);
                }
            }

            return null; // No available chamber found
        }
    }
}
