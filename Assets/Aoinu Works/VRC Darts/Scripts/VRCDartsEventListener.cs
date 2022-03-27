
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace AoinuWorks.VRCDarts
{
    public class VRCDartsEventListener : UdonSharpBehaviour
    {
        public void OnResetDarts()
        {
            Debug.Log("OnResetDarts");
        }

        public void OnEntryLocalPlayer()
        {
            Debug.Log("OnEntryLocalPlayer");
        }

        public void OnExitLocalPlayer()
        {
            Debug.Log("OnExitLocalPlayer");
        }

        public void OnFinishGame(VRCPlayerApi winner)
        {
            Debug.Log($"OnFinishGame\nWinner: {winner.displayName}");
        }
    }
}
