using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particle : MonoBehaviour
{
    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if(particle.isPlaying)
            {
                particle.Stop();
            }
            else
            {
                particle.Play();
            }
        }
    }

    public enum ParticleAction
    {
        Idle,
        Run,
        End
    }
    public ParticleSystem particle;

    /*public void OnAnimation()
    {
        if (particle.isPlaying)
        {
            particle.Stop();
        }
        else
        {
            particle.Play();
        }
    }*/

    public void PariticleAction(string Action)
    {
        if (!System.Enum.TryParse(Action, out ParticleAction action))
        {
            Debug.LogWarning($"[ParticleAction] 알 수 없는 명령: {Action}");
            return;
        }

        switch (action)
        {
            case ParticleAction.Idle:
                particle.Stop();
                break;
            case ParticleAction.Run:
                particle.Play();
                break;
            case ParticleAction.End:
                particle.Stop();
                break;
        }
    }
}
