using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    // The time the projectile will keep moving without
    // disappearing if it doesn't hit anything
    public float self_destruct_seconds;

    // Start is called before the first frame update
    void Start()
    {
        Invoke("SelfDestroy", self_destruct_seconds);
    }
    
    // Destroy itself if it hits something
    private void OnTriggerEnter2D(Collider2D collider)
    {
        if(!collider.gameObject.CompareTag("Detector")) SelfDestroy();
    }

    // Destroying function
    private void SelfDestroy()
    {
        Destroy(this.gameObject);
    }
}
