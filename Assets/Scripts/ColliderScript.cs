using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderScript : MonoBehaviour
{
    public string detection_tag; 
    public List<GameObject> listeners = new List<GameObject>();
    private List<Component> _listeners = new List<Component>();
    public bool player_detected = false;
    public bool test;
    public bool draw_rays = true;
    private GameObject detected;
    // Start is called before the first frame update
    void Start()
    {
        foreach (GameObject listener in listeners)
        {
            _listeners.Add(listener.GetComponent("enemyScript"));
        }
    }

    private void Update()
    {
        if (draw_rays)
        {
            foreach (GameObject enemy in listeners)
            {
                Debug.DrawLine(transform.position, enemy.transform.position, Color.blue);
            }
        }
        if (player_detected)
        {
            foreach (Component listener in _listeners)
            {
                listener.SendMessage("OnTriggerEnter2D", detected.transform.position);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if(collider.gameObject.CompareTag(detection_tag))
        {
            player_detected = true;
            detected = collider.gameObject;
        }
    }

    void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.gameObject.CompareTag(detection_tag))
        {
            player_detected = false;
        }
    }
}