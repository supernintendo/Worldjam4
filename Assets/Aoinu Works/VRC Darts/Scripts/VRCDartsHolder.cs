
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace AoinuWorks.VRCDarts
{
    public class VRCDartsHolder : UdonSharpBehaviour
    {
        [SerializeField] VRCDartsMachine machine;
        [SerializeField] string eventName = "RespawnDarts";

        public override void OnPickupUseDown()
        {
            machine.SendCustomEvent(eventName);
        }
    }
}
