using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FocusLostLoosy : MonoBehaviour
{
    [SerializeField]
    private float interval = 5;

    private float timer = 0;

    private bool isClose = false;

    public void SetClose(bool enabled)
    {
        isClose = enabled;
    }

// Update is called once per frame
    void Update()
    {
        if (isClose)
        {
            timer += Time.deltaTime;
            if (timer > interval)
            {
                timer = 0;
                isClose = false;
                gameObject.SetActive(false);
            }
        }
        else
        {
            timer = 0;

        }
    }
}
