using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class Unit_Control : MonoBehaviour
{
    public NavMeshAgent agent;

    [Header ("Attack Details")]
    public float attackCooldown;
    public float attackCooldownTimer;

    [Header ("Dynamics")]
    public Transform attackTarget;
    public float targetDistance;

    void Awake () {
        agent = GetComponent<NavMeshAgent>();
    }

    public void Move_Order (Vector3 movePos) {
        if (agent != null) {
            if (agent.isStopped == true) {
                agent.isStopped = false;
            }

            agent.SetDestination (movePos);
        }
    }

    void Update () {
        if (attackTarget != null) {
            targetDistance = Vector3.Distance (transform.position, attackTarget.position);
        } else {
            targetDistance = -99999;
        }
    }

}
