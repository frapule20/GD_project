using UnityEngine;
using UnityEngine.AI;
using System.Collections;


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
    // stao inziale della guardia
    public AIState actualState = AIState.Idle;

    [Header("Movement Settings")]
    public GameObject[] checkPoints;

    [SerializeField] public float walkSpeed = 2f;
    [SerializeField] public float runSpeed = 4f;
    [SerializeField] public float waitTime = 5f;

    [Header("Sensory Settings")]
    [SerializeField] public float hearingRange = 5f;
    [SerializeField] public float sightRange = 5f;
    [Range(0, 360)] public float sightAngle = 120;
    private AudioSource audioSource;
    private AudioSource audioSource2;
    public AudioClip attackClip;
    public AudioClip swordClip;
    

    Animator anim;
    private float stateTimer = 0f;
    private int actualCheckPoint = 0;
    private NavMeshAgent agent;
    PlayerController player;
    Vector3 soundSource;
    Ray ray;
    RaycastHit hit;


    void Start()
    {
        player = FindFirstObjectByType<PlayerController>();
        agent = GetComponent<NavMeshAgent>();
        anim = transform.GetComponentInChildren<Animator>();
        audioSource = GetComponent<AudioSource>();
        audioSource2 = GetComponent<AudioSource>();
        audioSource.spatialBlend = 1f;
        audioSource.rolloffMode = AudioRolloffMode.Logarithmic;

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
                if (PlayerInSight())
                {
                    ChangeState(AIState.Chase);
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
                if (PlayerInSight())
                {
                    ChangeState(AIState.Chase);
                }
                break;
            case AIState.Alert:
                MoveToSound();
                if(DestinationReached())
                {
                    ChangeState(AIState.Wait);
                }
                if(PlayerIsHeard())
                {
                    ChangeState(AIState.Alert);
                }
                if (PlayerInSight())
                {
                    ChangeState(AIState.Chase);
                }
                break;
            case AIState.Chase:
                MoveToPlayer();
                if (!PlayerAlive())
                {
                    ChangeState(AIState.Wait);
                    break;
                }
                if (PlayerInSight() && DestinationReached() && TimeOut(0.3f))
                {
                    player.CanMove = false;
                    ChangeState(AIState.Attack);
                }
                break;
            case AIState.Attack:
                break;
        }
    }

    void ChangeState(AIState newState)
    {
        actualState = newState;
        stateTimer = 0f;

        UpdateAnimation();

        if (newState == AIState.Attack)
        {
            
            StartCoroutine(AttackRoutine());
        }

    }

    void UpdateAnimation()
    {
        switch (actualState)
        {
            case AIState.Idle:
                agent.speed = 0f;
                agent.isStopped = true;
                anim.ResetTrigger("Attack");
                anim.SetBool("IsMoving", false);
                anim.SetBool("IsAlert", false);
                break;
            case AIState.Patrol:
                agent.speed = walkSpeed;
                agent.isStopped = false;
                anim.SetBool("IsMoving", true);
                anim.SetBool("IsAlert", false);
                anim.ResetTrigger("Attack");
                break;
            case AIState.Wait:
                agent.speed = 0f;
                agent.isStopped = true;
                anim.SetBool("IsMoving", false);
                anim.SetBool("IsAlert", false);
                anim.ResetTrigger("Attack");
                break;
            case AIState.Alert:
                agent.speed = walkSpeed;
                anim.SetBool("IsMoving", true);
                anim.SetBool("IsAlert", false);
                anim.ResetTrigger("Attack");
                break;
            case AIState.Chase:
                agent.speed = runSpeed;
                anim.SetBool("IsMoving", true);
                anim.SetBool("IsAlert", true);
                anim.ResetTrigger("Attack");
                break;
            case AIState.Attack:
                agent.speed = 0f;
                agent.isStopped = true;
                if (!audioSource.isPlaying)
                {
                    audioSource.PlayOneShot(attackClip);
                    audioSource2.PlayOneShot(swordClip);
                }
                anim.SetBool("IsMoving", false);
                anim.SetBool("IsAlert", false);
                anim.SetTrigger("Attack");
                break;
        }
    }

    #region Actions
    void MoveToPlayer()
    {
        agent.destination = player.transform.position;
        agent.isStopped = false;
    }

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

    public void HitPlayer()
    {
        Debug.Log("Hit Player");
        player.KillMe();
        ChangeState(AIState.Wait);
    }


    #endregion
    #region Decisions

    bool PlayerAlive()
    {
        return !player.IsDead;
    }
    bool DestinationReached()
    {
        // verifica se ha raggiunto al destinazione
        return agent.remainingDistance < agent.stoppingDistance && !agent.pathPending;

    }

    bool TimeOut(float timeToWait)
    {
        return stateTimer >= timeToWait;
    }

    bool PlayerIsHeard()
    {
        if (player.IsDead) 
        return false;
        
        float distance = Vector3.Distance(player.transform.position, transform.position);
        bool result = !player.IsStealth && player.IsMoving && distance < hearingRange;
        if (result)
        {
            soundSource = player.transform.position;
        }
        return result;
    }

    bool PlayerInSight()
    {
        if (player.IsDead) 
        return false;
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
        if (distanceToPlayer < sightRange)
        {
            Debug.DrawLine(transform.position, player.transform.position, Color.red);
            // calcoliamo la direzione e l'angolo tra la guardia e il giocatore
            Vector3 directionToPlayer = (player.transform.position - transform.position).normalized;
            float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
            // vediamo se il giocatore è all'interno dell'angolo di visione della guardia
            if (angleToPlayer < sightAngle / 2)
            {
                Vector3 startPos;
                if (player.IsStealth)
                {
                    startPos = transform.position + Vector3.up * 0.7f;
                }
                else
                {
                    startPos = transform.position + Vector3.up * 1.5f;
                }
                ray = new Ray(startPos, directionToPlayer);
                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.transform.gameObject.tag == "Player")
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }
    #endregion

    IEnumerator AttackRoutine()
    {
        agent.isStopped = true;
        anim.SetTrigger("Attack"); 
        yield return null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, hearingRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }
}
