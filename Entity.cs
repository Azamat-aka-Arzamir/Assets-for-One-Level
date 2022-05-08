using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    public int HealthPoints;
    public int Stamina;
    public int StaminaRemains;
    public int StaminaRegenerationSpeed;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void RegenerateStamina()
    {
        if (StaminaRemains < Stamina)
        {
            StaminaRemains += StaminaRegenerationSpeed;
        }
    }
}
