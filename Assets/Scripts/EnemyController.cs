using UnityEngine;
using UnityEngine.AI;


public enum AIState
{
    Idle,
    Patrol,
    Wait,
    Alert,
    Chase,
    Attack,
}
public class EnemyController : MonoBehaviour
{
    public AIState actualState = AIState.Idle;

    [Header("Movement Settings")]
    public GameObject[] checkPoints;

    [SerializeField] public float walkSpeed = 2f;
    [SerializeField] public float runSpeed = 4f;
    [SerializeField] public float waitTime = 5f;

    [Header("Sensory Settings")]
    [SerializeField] public float hearingRange = 5f;


    Animator anim;
    private float stateTimer = 0f;
    private int actualCheckPoint = 0;
    private NavMeshAgent agent;
    PlayerController player;
    Vector3 soundSource;


    void Start()
    {
        player = FindFirstObjectByType <PlayerController>();
        agent = GetComponent<NavMeshAgent>();
        anim = transform.GetComponentInChildren<Animator>();

    }

    void Update()
    {
        stateTimer += Time.deltaTime;
        switch (actualState)
        {
            case AIState.Idle:
                ChangeState(AIState.Patrol);
                break;
            case AIState.Patrol:
                MoveToCheckpoint();
                if (DestinationReached())
                {
                    NextCheckpoint();
                    ChangeState(AIState.Wait);
                }

                if (PlayerIsHeard())
                {
                    ChangeState(AIState.Alert);
                }
                break;
            case AIState.Wait:
                if (TimeOut(waitTime))
                {
                    ChangeState(AIState.Patrol);
                }
                if (PlayerIsHeard())
                {
                    ChangeState(AIState.Alert);
                }
                break;
            case AIState.Alert:
                MoveToSound();
                if (DestinationReached())
                {
                    ChangeState(AIState.Wait);
                }
                if(PlayerIsHeard())
                {
                    ChangeState(AIState.Alert);
                }
                break;
            case AIState.Chase:
                // Handle chase behavior
                break;
            case AIState.Attack:
                // Handle attack behavior
                break;
        }
    }

    void ChangeState(AIState newState)
    {
        actualState = newState;
        stateTimer = 0f;

        UpdateAnimation();

    }

    void UpdateAnimation()
    {
        switch (actualState)
        {
            case AIState.Idle:
                agent.speed = 0f;
                agent.isStopped = true;
                anim.SetBool("IsMoving", false);
                break;
            case AIState.Patrol:
                agent.speed = walkSpeed;
                anim.SetBool("IsMoving", true);
                break;
            case AIState.Wait:
                agent.speed = 0f;
                agent.isStopped = true;
                anim.SetBool("IsMoving", false);
                break;
            case AIState.Alert:
                agent.speed = walkSpeed;
                anim.SetBool("IsMoving", true);
                break;
            case AIState.Chase:
                ;
                break;
            case AIState.Attack:
                break;
        }
    }

    #region Actions

    void MoveToSound()
    {
        agent.destination = soundSource;
        agent.isStopped = false;
    }
    void MoveToCheckpoint()
    {
        agent.destination = checkPoints[actualCheckPoint].transform.position;
        agent.isStopped = false;
    }

    void NextCheckpoint()
    {
        actualCheckPoint++;
        if (actualCheckPoint >= checkPoints.Length)
        {
            actualCheckPoint = 0;
        }
    }


    #endregion
    #region Decisions

    bool DestinationReached()
    {
        return agent.remainingDistance < agent.stoppingDistance && !agent.pathPending;

    }

    bool TimeOut(float timeToWait)
    {
        return stateTimer >= timeToWait;
    }

    bool PlayerIsHeard()
    {
        float distance = Vector3.Distance(player.transform.position, transform.position);
        bool result = !player.IsStealth && !player.IsMoving && distance < hearingRange;
        if (result)
        {
            soundSource = player.transform.position;
        }
        return result;
    }
    #endregion

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, hearingRange);
    }
}
