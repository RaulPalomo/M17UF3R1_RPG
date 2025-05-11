using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public enum State { Patrolling, Chasing, Searching, Fleeing, Attacking }
    private State currentState;

    public Transform[] patrolPoints;
    public Transform player;
    public float chaseRange = 10f;
    public float searchTime = 3f;
    public float fleeHealthThreshold = 0.5f;
    public float patrolWaitTime = 2f;
    public float fieldOfView = 90f;
    public float viewDistance = 10f;

    private NavMeshAgent agent;
    private int currentPatrolIndex;
    private float searchTimer;
    private float patrolTimer;
    private float maxHealth = 100f;
    public float currentHealth;
    private Vector3 lastSeenPosition;
    private bool waiting;

    private Animator animator;
    private Coroutine attackCoroutine;

    public BoxCollider attackCollider;
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        currentHealth = maxHealth;
        currentState = State.Patrolling;
        GoToNextPatrolPoint();
        animator = GetComponent<Animator>();
        
    }

    void Update()
    {
        switch (currentState)
        {
            case State.Patrolling:
                animator.SetBool("IsRunning", false);
                animator.SetBool("IsWalking", true);
                Patrol();
                break;
            case State.Chasing:
                animator.SetBool("IsRunning", true);
                animator.SetBool("IsWalking", false);
                Chase();
                break;
            case State.Searching:
                Search();
                break;
            case State.Fleeing:
                Flee();
                break;
            case State.Attacking:
                Attack();
                break;
        }

    }

    bool CanSeePlayer()
    {
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, directionToPlayer);

        if (angle < fieldOfView / 2 && Vector3.Distance(transform.position, player.position) < viewDistance)
        {
            RaycastHit hit;
            if (Physics.Linecast(transform.position, player.position, out hit))
            {
                if (hit.transform == player)
                {
                    return true;
                }
            }
        }
        return false;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            currentState = State.Attacking;
            attackCoroutine = StartCoroutine(Attack());
        }
    
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StopCoroutine(attackCoroutine);
        }
        
        currentState = State.Chasing;
        

    }
    public IEnumerator Attack()
    {
        animator.SetTrigger("Attack");
        attackCollider.enabled = true;
        yield return new WaitForSeconds(0.5f);
        attackCollider.enabled = false;
        attackCoroutine = StartCoroutine(Attack());
    }
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            // Handle enemy death
            Destroy(gameObject);
        }
    }
    void Patrol()
    {
        if (currentHealth < maxHealth * fleeHealthThreshold)
        {
            currentState = State.Fleeing;
            return;
        }

        if (CanSeePlayer())
        {
            currentState = State.Chasing;
            return;
        }

        if (!agent.pathPending && agent.remainingDistance < 0.5f && !waiting)
        {
            StartCoroutine(WaitAtPatrolPoint());
        }

    }

    IEnumerator WaitAtPatrolPoint()
    {
        waiting = true;
        yield return new WaitForSeconds(patrolWaitTime);
        GoToNextPatrolPoint();
        waiting = false;
    }

    void Chase()
    {
        
        if (currentHealth < maxHealth * fleeHealthThreshold)
        {
            currentState = State.Fleeing;
            return;
        }

        agent.SetDestination(player.position);
        lastSeenPosition = player.position;

        if (!CanSeePlayer())
        {

            currentState = State.Searching;
            searchTimer = searchTime;
        }
    }

    void Search()
    {
        searchTimer -= Time.deltaTime;

        if (searchTimer > 0)
        {
            agent.SetDestination(lastSeenPosition);
        }
        else
        {
            currentState = State.Patrolling;
            GoToNextPatrolPoint();
        }
    }
    void OnDrawGizmosSelected()
    {
        if (player == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewDistance);

        Vector3 leftBoundary = Quaternion.Euler(0, -fieldOfView / 2, 0) * transform.forward;
        Vector3 rightBoundary = Quaternion.Euler(0, fieldOfView / 2, 0) * transform.forward;

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary * viewDistance);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary * viewDistance);
    }
    void Flee()
    {
        Vector3 fleeDirection = (transform.position - player.position).normalized;
        Vector3 fleePosition = transform.position + fleeDirection * chaseRange;
        agent.SetDestination(fleePosition);
    }

    void GoToNextPatrolPoint()
    {
        if (patrolPoints.Length == 0) return;
        agent.destination = patrolPoints[currentPatrolIndex].position;
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
    }
}
