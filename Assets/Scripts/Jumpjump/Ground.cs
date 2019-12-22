using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Ground : MonoBehaviour
{
	// Start is called before the first frame update
	MeshRenderer render;
    private float deltime;
    void Start()
	{
		render = gameObject.GetComponent<MeshRenderer>();
	}

    // Update is called once per frame
    void Update()
	{
        Color color1 = new Color(122.0f / 255, 122.0f / 255, 122.0f / 255);
        Color color2 = new Color(255.0f / 255, 124.0f / 255, 126.0f / 255);
        Color color3 = new Color(57.0f / 255, 130.0f / 255, 123.0f / 255);
        Color color4 = new Color(95.0f / 255, 54.0f / 255, 134.0f / 255);

  
        if (deltime <= 20.0f)
        {
            deltime += Time.deltaTime;
            Debug.Log(deltime);
            render.material.color = Color.Lerp(color1, color2, deltime / 20.0f);
        }
        else if (deltime > 20.0f && deltime <= 40.0f)
        {
            deltime += Time.deltaTime;
            Debug.Log(deltime);
            render.material.color = Color.Lerp(color2, color3, (deltime - 20) / 20.0f);
        }
        else if (deltime > 40.0f && deltime <= 60.0f)
        {
            deltime += Time.deltaTime;
            render.material.color = Color.Lerp(color3, color4, (deltime - 40) / 20.0f);
        }

    }
}