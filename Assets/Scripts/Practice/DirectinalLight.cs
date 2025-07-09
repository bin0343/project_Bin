using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class DirectinalLight : MonoBehaviour
{
    public Light spotlight;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.LeftControl))
        {
            if(Input.GetKey(KeyCode.KeypadPlus))
            {
                spotlight.intensity += 0.003f;
            }
            else if(Input.GetKey(KeyCode.KeypadMinus))
            {
                spotlight.intensity -= 0.003f;
            }
        }
    }

    
}
