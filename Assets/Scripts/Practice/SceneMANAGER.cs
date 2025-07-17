using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class SceneMANAGER : MonoBehaviour
{
    public string UserName;

    private void Awake()
    {
        if (Shared.SceneMANAGER == null)
        {
            Shared.SceneMANAGER = this;

            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(this);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        //PlayerPrefs.DeleteAll();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
