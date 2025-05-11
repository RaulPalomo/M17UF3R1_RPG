using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damager : MonoBehaviour
{
    public float damageAmount = 10f; // cantidad de daño que inflige el objeto
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerMovement>().TakeDamage(damageAmount);
        }
        else if (other.CompareTag("Enemy"))
        {

            Debug.Log("Enemy hit");
            other.GetComponent<EnemyAI>().TakeDamage(damageAmount);
        }
    }
}
