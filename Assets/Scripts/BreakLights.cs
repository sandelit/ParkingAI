using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakLights : MonoBehaviour
{

    [SerializeField] private Light rearLeftLight;
    [SerializeField] private Light rearRightLight;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey(KeyCode.Space))
        {
            RearLights("On");
        }
        else
        {
            RearLights("Off");
        }
    }

    private void RearLights(string state)
    {

        if (state == "On")
        {
            rearLeftLight.intensity = 0.9f;
            rearRightLight.intensity = 0.9f;
        }
        else
        {
            rearLeftLight.intensity = 0;
            rearRightLight.intensity = 0;
        }
    }
}
