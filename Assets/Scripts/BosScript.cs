using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BosScript : MonoBehaviour
{
    public enum enemyPart
    {
        Weapon,
        Leg,
        Body
    }
    public GameObject player;
    private CharacterMovement characterScript;
    public float force_attack_spawn = 1;
    public GameObject throwable_object;
    public Vector3 throwable_spawn_start;
    public Vector3 throwable_spawn_end;
    public GameObject mini_object;
    public Vector3 mini_spawn;
    public Transform spikes_father;
    public Animator animator;
    public GameObject shield_object;
    // animator.SetBool("Lleig",true)

    private List<GameObject> Spikes = new List<GameObject>();
    private float spike_y;
    private bool can_attack;
    private int spike_shown;
    private EnemyState state = EnemyState.First;

    public enum EnemyState
    {
        First,
        Second,
        Third
    }
    // Start is called before the first frame update
    void Start()
    {
        characterScript = this.gameObject.GetComponent<CharacterMovement>();
        for (int spike_num = 0; spike_num < spikes_father.childCount; spike_num++)
        {
            Spikes.Add(spikes_father.GetChild(spike_num).gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(characterScript.lives > 20)
        {
            First();
        }
        else if(characterScript.lives > 15)
        {
            if(state == EnemyState.First)
            {
                state = EnemyState.Second;
                Instantiate(mini_object, transform.position + new Vector3(-2,0, transform.position.z),
                                                Quaternion.identity);
                Instantiate(mini_object, transform.position + new Vector3(-2, 2, transform.position.z),
                                                Quaternion.identity);
                InvokeRepeating("ShowSpikes", 0, 10);
                InvokeRepeating("HideSpikes", 4, 10);
                
            }
            Second();
        }
        else if(characterScript.lives > 10)
        {
            if (state == EnemyState.Second)
            {
                state = EnemyState.Third;
                CancelInvoke();
                InvokeRepeating("SpawnMini", 0, 10);
                InvokeRepeating("ThrowInArea", 5, 7);
                InvokeRepeating("SpawnShield", 5, 7);
                Show_spikes();
            }
            Third();
        }
    }

    void AutoAttack()
    {
        characterScript.SendMessage("Attack", (player.transform.position - this.transform.position) *force_attack_spawn);
    }

    void ThrowInArea()
    {

        Instantiate(throwable_object, transform.position + new Vector3(Random.Range(throwable_spawn_start.x,throwable_spawn_end.x),
                                                                    Random.Range(throwable_spawn_start.y, throwable_spawn_end.y),
                                                                    transform.position.z),
                                                Quaternion.identity);
    }

    void Show_spikes()
    {
        Spikes[spike_shown].transform.Translate(0, 1, 0);
        if (spike_shown < Spikes.Count - 1)
        {
            spike_shown++;
            Show_spikes();
        }
    }
    void Hide_spikes()
    {
        Spikes[spike_shown].transform.Translate(0, -1, 0);
        if (spike_shown > 0)
        {
            spike_shown--;
            Show_spikes();
        }
    }
    void First()
    {
        if (can_attack)
        {
            characterScript.Attack();
            can_attack = false;
            Invoke("AttackTimeout", 0.2f);
        }
    }
    void Second()
    {
        if (can_attack)
        {
            AutoAttack();
            can_attack = false;
            Invoke("AttackTimeout", 0.2f);
        }
    }
    void Third()
    {
        if (can_attack)
        {
            AutoAttack();
            Invoke("AutoAttack", 0.5f);
            Invoke("AutoAttack", 1);
            can_attack = false;
            Invoke("AttackTimeout", 0.2f);
        }
    }

    void AttackTimeout()
    {
        can_attack = true;
    }

    void SpawnMini()
    {
        Instantiate(mini_object, transform.position + new Vector3(-2, 0, transform.position.z),
                                                Quaternion.identity);
    }

    void SpawnShield()
    {
        Instantiate(shield_object, transform.position, Quaternion.identity, this.gameObject.transform);
    }
}
