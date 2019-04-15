using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * */

public class enemyScript : MonoBehaviour
{
    public bool test;

    // The script the enemy will access to shoot or jump
    private CharacterMovement characterScript;

    [Header("States")]
    // All states an enemy can be
    public EnemyState currentState;

    public Animator animator;
    // The object whose children's x position will
    // be used as the points where the enemy will go

    [Header("Path")]
    public Transform path;
    // The speed the enemy will walk at
    public float walkSpeed;
    // The speed the enemy will chase at
    public float chaseSpeed;
    // The collider of the gameObjects that will act as detection zone
    public List<GameObject> detection;
    // The minimum distance (x) from next path to sitch to the next path
    public float margen = 0.5f;
    // Time waiting when reaching one path position
    public float seconds_waiting;

    // The list of colliders got from detection
    private List<Collider2D> colliders = new List<Collider2D>();
    // The player the enemy will try to kill
    public GameObject player;
    // If the player is being detected by the enemy
    private bool playerDetected;
    // The place where the player has been seen last
    private Vector3 last_seen;
    // If the enemy has forgotten the player
    private bool seen_timeout = false;
    // The positions of the objects got from path's children
    private List<Vector3> pathPositions = new List<Vector3>();
    // 
    private int currentPosition = 0;
    // The position in pathPositions of the next stop
    private int nextPosition;
    // If the enemy's movement is towards +x
    private bool forwardMovement = true;
    // If the enemy is waiting in place
    public bool waiting = false;

    [Header("Abillities")]
    // If the enemy can attack
    public bool can_attack;
    // If the enemy will aim at the player
    public bool aim_player;
    // Minimum distance for the enemy to attack
    public float min_dist_attack;
    // If the enemy can actually walk or it should stand still
    public bool can_walk = true;
    // If the enemy can jump
    public bool can_jump = true;
    // [Not implemented ] If the enemy will have 360 vision
    //public bool can_see = true;
    // The time it will take for the enemy to attack again
    public float attack_every_seconds;
    // Explode itself if the player is near
    public bool near_player_inmolate;
    public float inmolate_distance = 1f;
    public GameObject explosion_object;
    public float attack_while_moving;

    // If the enemy has attacked recently
    private bool attacking;
    private int side;

    [Header("Audio")]
    public AudioClip audioWalk;
    public AudioClip audioRun;

    private AudioSource source;

    [Header("RayCast")]
    // If the lines must be shown
    public bool draw_lines = true;
    // If the npc will look if it can keep walking
    public bool careful_walk = true;
    // The position of the ray that will look if the npc
    // can keep walking
    public float vision_max;
    // The position the careful_walk will spawn at
    public Vector2 walking_ray_position;
    // The length of the ray
    public float walking_RayLength = 0.5f;


    // Layers for ray cast detection
    LayerMask Ground;
    LayerMask Player_layer;

    private Vector3 fly_to_pos;




    // Start is called before the first frame update
    void Start()
    {
        //rb = transform.GetComponent<Rigidbody2D>();

        for (int path_num = 0; path_num < path.childCount; path_num++)
        {
            pathPositions.Add(path.GetChild(path_num).position);
        }

        if (pathPositions.Count > 1) nextPosition = currentPosition + 1;

        foreach (GameObject detector in detection)
        {
            colliders.Add(detector.GetComponent<Collider2D>());
        }

        Ground = 1 << LayerMask.NameToLayer("Ground");
        Player_layer = 1 << LayerMask.NameToLayer("Player");

        characterScript = this.gameObject.GetComponent<CharacterMovement>();


    }

    // All possible enemy states
    public enum EnemyState
    {
        Patrol,
        Chase,
        Stay,
        Fly
    };



    // Update is called once per frame
    void Update()
    {
        switch (currentState)
        {
            case EnemyState.Patrol:
                Patrol();
                break;
            case EnemyState.Chase:
                Chase();
                break;
            case EnemyState.Stay:
                if (!waiting)
                {
                    Invoke("Stay", 3f);
                    waiting = true;
                }
                break;
            case EnemyState.Fly:
                if (waiting)
                {
                    Fly();
                }
                else FlyTo();
                break;
            default:
                currentState = EnemyState.Patrol;
                break;
        }
        if (can_jump)
        {
            can_jump = false;
            Jump();
        }

        if (careful_walk) CanWalk();
        /*
        if (can_see && (Physics2D.Raycast(transform.position, player.transform.position, vision_max, Player_layer) &&
                        !Physics2D.Raycast(transform.position, player.transform.position, vision_max, Ground)))
        {
            OnTriggerEnter2D(player.transform.position);
            if (draw_lines) Debug.DrawLine(transform.position, player.transform.position,
                                        Color.red);
        }
        */
    }

    // Walks a predefined path. If detects a player, it starts chasing him
    private void Patrol()
    {
        animator.SetBool("Walking", true);
        animator.SetBool("Running", false);
        if ((can_walk || !careful_walk) && Vector3.Distance(transform.position, pathPositions[nextPosition]) > margen)
        {
            Vector3 position_diff = new Vector3((transform.position.x < pathPositions[nextPosition].x ? 1 : -1), 0, 0);
            transform.Translate(position_diff * walkSpeed * Time.deltaTime);
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * position_diff.x, transform.localScale.y, transform.localScale.z);
        }
        else if (!waiting)
        {
            waiting = true;
            Invoke("CalculateNextPosition", seconds_waiting);
        }
        else if (!can_walk && careful_walk && Vector3.Distance(transform.position, pathPositions[nextPosition]) > margen) CalculateNextPosition();

        if (playerDetected)
        {
            currentState = EnemyState.Chase;
        }
    }

    // Just selects the next position the enemy will go to
    private void CalculateNextPosition()
    {
        currentPosition = nextPosition;
        if (forwardMovement)
        {
            if (currentPosition == pathPositions.Count - 1)
            {
                forwardMovement = false;
                nextPosition--;
            }
            else nextPosition++;
        }
        else
        {
            if (currentPosition == 0)
            {
                forwardMovement = true;
                nextPosition++;
            }
            else nextPosition--;
        }
        waiting = false;
    }

    // Goes after the enemy. If the enemy can attack, it will do it
    // once he started chasing the detected player
    private void Chase()
    {
        animator.SetBool("Walking", false);
        animator.SetBool("Running", true);
        source.PlayOneShot(audioRun);
        if ((can_walk || !careful_walk) && can_attack && Mathf.Abs(transform.position.x - last_seen.x) > margen)
        {
            side = transform.position.x > last_seen.x ? -1 : 1;
            transform.Translate(new Vector3(chaseSpeed * side * Time.deltaTime, 0, 0));
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * side, transform.localScale.y, transform.localScale.z);
        }
        if (!playerDetected && (seen_timeout || Mathf.Abs(transform.position.x - last_seen.x) <= margen))
        {
            if ((can_walk || !careful_walk) && pathPositions.Count > 0) currentState = EnemyState.Patrol;
            else currentState = EnemyState.Stay;
        }
        if (can_attack && Vector3.Distance(player.transform.position, transform.position) < min_dist_attack)
        {
            can_attack = false;
            if (aim_player) AutoAttack();
            else Attack();
            if (Vector3.Distance(player.transform.position, transform.position) < inmolate_distance) Inmolate();
        }
    }

    // Stays still, looking right to left an left to right
    private void Stay()
    {
        transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
        waiting = false;
        if (playerDetected && can_walk)
        {
            currentState = EnemyState.Chase;
        }
    }

    private void Fly()
    {
        test = true;
        fly_to_pos = new Vector3(Random.Range(pathPositions[0].x, pathPositions[1].x),
                                        Random.Range(pathPositions[0].y, pathPositions[1].y),
                                        transform.position.z);
        if (can_attack && Vector3.Distance(player.transform.position, transform.position) < min_dist_attack)
        {
            can_attack = false;
            if (aim_player) AutoAttack();
            else Attack();
            if (Vector3.Distance(player.transform.position, transform.position) < inmolate_distance) Inmolate();
        }
    }

    private void FlyTo()
    {
        if (Vector3.Distance(transform.position, fly_to_pos) > margen)
        {
            transform.Translate((fly_to_pos - transform.position) / 10);
        }
        else
        {
            waiting = true;
            Invoke("WaitTimeout", Random.Range(1, 6));
        }
    }

    // Sends a raycast which sets if the enemy can keep walking
    void CanWalk()
    {
        Vector2 ray_start = new Vector2(transform.position.x + walking_ray_position.x * transform.localScale.x,
                                        transform.position.y + walking_ray_position.y);
        can_walk = Physics2D.Raycast(ray_start, -Vector3.up, walking_RayLength, Ground);
        if (draw_lines) Debug.DrawLine(ray_start, ray_start - Vector2.up * walking_RayLength,
                                        Color.cyan);
    }

    // Runs CharacterMovement's Jump()
    private void Jump()
    {
        Invoke("JumpTimeout", 1f);
        characterScript.Jump();
    }

    void Attack()
    {
        characterScript.Attack();
        Invoke("AttackingTimeout", attack_every_seconds);
    }

    // Runs CharacterMovement's Attack() but directed to the current
    // player's position
    private void AutoAttack()
    {
        characterScript.Attack((transform.position - player.transform.position) * 1000);
        Invoke("AttackingTimeout", attack_every_seconds);
    }

    void Inmolate()
    {
        Instantiate(explosion_object, transform.position, Quaternion.identity);
        characterScript.Die();
    }

    // If the enemy has been detected, it gets executed
    public void OnTriggerEnter2D(Vector3 location)
    {
        playerDetected = true;
        seen_timeout = false;
        last_seen = location;
        Invoke("DetectionTimeout", 5f);
    }

    // The enemy has lost the player
    private void DetectionTimeout()
    {
        seen_timeout = true;
        playerDetected = false;
    }

    // The enemy can keep jumping
    private void JumpingTimeout()
    {
        can_jump = true;
    }
    // The enemy can keep attacking
    private void AttackingTimeout()
    {
        test = true;
        can_attack = true;
    }
    private void WaitTimeout()
    {
        waiting = false;
    }
}

