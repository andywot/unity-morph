using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundImage : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;

    private void Update()
    {
        transform.position = new Vector3(mainCamera.transform.position.x, transform.position.y, transform.position.z);
    }
}
