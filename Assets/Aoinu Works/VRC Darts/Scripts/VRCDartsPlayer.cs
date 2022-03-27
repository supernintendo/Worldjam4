
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using System;
using TMPro;

namespace AoinuWorks.VRCDarts
{
    public class VRCDartsPlayer : UdonSharpBehaviour
    {
        [SerializeField] VRCDartsMachine machine;

        public void _EntrySlot(VRCPlayerApi player)
        {
            Networking.SetOwner(player, gameObject);
        }

        public void _ExitSlot()
        {
        }

        public VRCPlayerApi _GetPlayer()
        {
            return Networking.GetOwner(gameObject);
        }

        public override void OnOwnershipTransferred()
        {
            machine.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(machine.UpdatePlayerNames));
        }

        public void RespawnDarts()
        {
            machine.RespawnDarts();
        }
    }
}
