
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class TimeText : UdonSharpBehaviour
{
    private Transform parent;
    private UdonBehaviour parentUdonBehaviour;
    private int timeLeft;
    private float initialWidth;

    // Start is called before the first frame update
    void Start()
    {
        parent = this.transform.parent;
        parentUdonBehaviour =  (UdonBehaviour)parent.GetComponent(typeof(UdonBehaviour));
        initialWidth = this.transform.localScale.x;
    }

    // Update is called once per frame
    void Update()
    {
        object udonTimeLeft = parentUdonBehaviour.GetProgramVariable("timeLeft");
        int udonTimeLeftValue = (int)udonTimeLeft;
        Vector3 localScale = this.transform.localScale;

        if (timeLeft != udonTimeLeftValue) {
            timeLeft = udonTimeLeftValue;

            TimeSpan timeSpan = TimeSpan.FromSeconds(timeLeft);
            string timeTextString = string.Format("{0:D2}:{1:D2}", timeSpan.Minutes, timeSpan.Seconds);

            Debug.Log(timeTextString);
            // TextMesh textMeshComponent = (UdonBehaviour)GetComponent(typeof(TextMesh));
            // textMeshComponent.text = TimeTextOld;

            // if (Camera.main.transform.position.z > this.transform.position.z) {
            //     this.transform.localScale = new Vector3(initialWidth * -1.0f, localScale.y, localScale.z);
            // } else {
            //     this.transform.localScale = new Vector3(initialWidth, localScale.y, localScale.z);
            // }
        }
    }
}
