using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingBlock : MonoBehaviour
{
    public float speed;
    public Transform targetPlayer;
    private void Update()
    {
        FindNearestPlayer();

        if (targetPlayer != null)
        {
            GoToPlayer();
        }
    }

    private void FindNearestPlayer()
    {
        if (GameObject.FindGameObjectWithTag("Player") == null) return;
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        float nearestDistance = Mathf.Infinity;

        foreach (GameObject player in players)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                targetPlayer = player.transform;
            }
        }
    }

    private void GoToPlayer()
    {
        Vector3 direction = (targetPlayer.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;
    }
}
