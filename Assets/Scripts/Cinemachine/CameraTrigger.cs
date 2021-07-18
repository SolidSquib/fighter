using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTrigger : MonoBehaviour
{
    [SerializeField] string _triggerName;

    public string triggerName { get { return _triggerName; } private set { _triggerName = value; } }

    // Start is called before the first frame update
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            CameraManager.instance.AddActiveCameraTrigger(this);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            CameraManager.instance.RemoveActiveCameraTrigger(this);
        }
    }
}
