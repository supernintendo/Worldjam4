
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace AoinuWorks.VRCDarts
{
    public class VRCDartsMachineButton : UdonSharpBehaviour
    {
        [SerializeField] VRCDartsMachine machine;
        [SerializeField] string eventName = "OnPressedMainButton";

        public override void Interact()
        {
            machine.SendCustomEvent(eventName);
        }
    }
}
