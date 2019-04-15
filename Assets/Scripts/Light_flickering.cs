using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Light_flickering : MonoBehaviour
{
    public float radius_min = 4;
    public float radius_max = 5;
    public float intensity_min = 11;
    public float intensity_max = 12;
    public float variation = 0.3f;

    private float radius;
    public int radius_direction = 1;
    private float intensity;
    public int intensity_direction = 1;

    private Light bulb;
    // Start is called before the first frame update
    void Start()
    {
        radius = Random.Range(radius_min, radius_max);
        intensity = Random.Range(intensity_min, intensity_max);
        bulb = this.gameObject.GetComponent<Light>();
    }

    // Update is called once per frame
    void Update()
    {
        if (radius > radius_max) radius_direction = -1;
        else if (radius < radius_min) radius_direction = 1;
        if (intensity > intensity_max) intensity_direction = -1;
        else if (intensity < intensity_min) intensity_direction = 1;
        radius += variation * radius_direction * Time.deltaTime;
        intensity += variation * intensity_direction * Time.deltaTime;
        bulb.intensity = intensity;
        bulb.range = radius;
    }
}
