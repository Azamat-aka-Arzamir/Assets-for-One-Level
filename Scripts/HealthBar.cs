using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBar : MonoBehaviour
{
    public Entity Entity;
    float health;
    float startLength;
    // Start is called before the first frame update
    void Start()
    {
        startLength = transform.localScale.x;
    }

    // Update is called once per frame
    void Update()
    {
        health = (float)Entity.HealthPoints/Entity.MaxHealthPoints;
        transform.localScale = new Vector3(startLength * health,transform.localScale.y,1);
    }
}
