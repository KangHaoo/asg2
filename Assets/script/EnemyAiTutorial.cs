/// <author>Kang Hao</author>
/// <date>2024-06-26</date>
/// <summary>
/// Controls the behavior of an enemy AI, including patrolling, chasing, and attacking the player.
/// </summary>


/// <remarks>
/// Attach this script to an enemy GameObject with a NavMeshAgent component.
/// Requires a healthBar component to display and manage AI health.
/// </remarks>
/// <example>
/// Attach this script to an enemy GameObject. Set the player tag in the Unity editor.
/// Adjust sight and attack ranges, patrol behavior, and attack settings as needed.
/// </example>



using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;




public class EnemyAiTutorial : MonoBehaviour
{
    /// <summary>
    /// The NavMeshAgent component for navigation.
    /// </summary>
    public NavMeshAgent agent;

    /// <summary>
    /// Reference to the player's transform.
    /// </summary>
    private Transform player;

    /// <summary>
    /// Layers to consider for ground and player detection.
    /// </summary>
    public LayerMask whatIsGround, whatIsPlayer;

    /// <summary>
    /// Maximum health of the AI.
    /// </summary>
    public float aiMaxHealth = 100f;

    /// <summary>
    /// Current health of the AI.
    /// </summary>
    private float aiHealth;

    /// <summary>
    /// Reference to the healthBar UI component for displaying AI health.
    /// </summary>
    public healthBar healthBar;

    // Patrolling
    public Vector3 walkPoint;
    bool walkPointSet;
    public float walkPointRange;

    // Attacking
    public float timeBetweenAttacks;
    bool alreadyAttacked;
    public GameObject projectile;

    // States
    public float sightRange, attackRange;
    public bool playerInSightRange, playerInAttackRange;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        aiHealth = aiMaxHealth;
        if (healthBar != null)
        {
            healthBar.SetMaxHealth(aiMaxHealth);
        }
    }

    private void Update()
    {
        // Re-assign player reference in case of scene change
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }

        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

        if (!playerInSightRange && !playerInAttackRange) Patroling();
        if (playerInSightRange && !playerInAttackRange) ChasePlayer();
        if (playerInAttackRange && playerInSightRange) AttackPlayer();
    }

    private void Patroling()
    {
        if (!walkPointSet) SearchWalkPoint();

        if (walkPointSet)
            agent.SetDestination(walkPoint);

        Vector3 distanceToWalkPoint = transform.position - walkPoint;

        if (distanceToWalkPoint.magnitude < 1f)
            walkPointSet = false;
    }

    private void SearchWalkPoint()
    {
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        if (Physics.Raycast(walkPoint, -transform.up, 2f, whatIsGround))
            walkPointSet = true;
    }

    private void ChasePlayer()
    {
        agent.SetDestination(player.position);
    }

    private void AttackPlayer()
    {
        agent.SetDestination(transform.position);

        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);

        if (!alreadyAttacked)
        {
            Vector3 projectileSpawnPosition = transform.position + directionToPlayer;

            GameObject spawnedProjectile = Instantiate(projectile, projectileSpawnPosition, Quaternion.identity);
            Rigidbody rb = spawnedProjectile.GetComponent<Rigidbody>();

            Vector3 forceDirection = (player.position - spawnedProjectile.transform.position).normalized;
            rb.AddForce(forceDirection * 32f, ForceMode.Impulse);
            rb.AddForce(Vector3.up * 8f, ForceMode.Impulse);

            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
    }

    /// <summary>
    /// Reduces AI health when taking damage and updates the healthBar if available.
    /// Destroys the enemy when health reaches zero.
    /// </summary>
    /// <param name="damage">Amount of damage to apply.</param>
    public void TakeDamage(int damage)
    {
        aiHealth -= damage;
        if (healthBar != null)
        {
            healthBar.SetHealth(aiHealth);
        }

        if (aiHealth <= 0) Invoke(nameof(DestroyEnemy), 0.5f);
    }

    /// <summary>
    /// Destroys the enemy GameObject.
    /// </summary>
    private void DestroyEnemy()
    {
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }
}
