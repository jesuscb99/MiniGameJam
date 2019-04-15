using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;

public class Object : MonoBehaviour
{
    private bool player_detected;
    public GameObject player;

    private CharacterMovement player_script;    
    public enum obj
    {
        palanca,
        puerta
    };
    public obj obj_type = obj.palanca;

    public Scene next_scene;

    public Animator animator;

    private CharacterMovement playerScript;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (player_detected && Input.GetKeyDown("E"))
        {
            switch (obj_type)
            {
                case obj.palanca:
                    playerScript.ResetTimer();
                    if(animator.GetBool("Active")) animator.SetBool("Active", true);
                    else animator.SetBool("Active", false);
                    break;
                case obj.puerta:
                    SceneManager.LoadScene(next_scene.buildIndex);
                    break;
            }
        }
    }

    void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.gameObject.CompareTag("Player"))
        {
            player_detected = true;
        }
    }
}
