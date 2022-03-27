
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using System;

namespace AoinuWorks.VRCDarts
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class VRCDartsPlayerEntrySystem : UdonSharpBehaviour
    {
        [Header("for UdonChips")]
        public float entryFee = 100f;
        public float commissionFee = 0f;

        [UdonSynced][HideInInspector] public int[] entryPlayerIds = new int[4];
        [SerializeField] VRCDartsMachine machine;
        [HideInInspector] public UdonBehaviour udonChips;

        [System.NonSerialized] public VRCDartsEventListener vrcDartsEventListener;

        void Start()
        {
            GameObject udonChipsObj = GameObject.Find("UdonChips");
            if (udonChipsObj != null)
            {
                udonChips = (UdonBehaviour)udonChipsObj.GetComponent(typeof(UdonBehaviour));
            }
            if (udonChips != null)
            {
                Debug.Log("UdonChips Mode");
            }
            else
            {
                Debug.Log("UdonChips is not found.");
            }
            entryPlayerIds = new int[4];
            ClearSlots();
        }

        public bool InteractSlot(int slot)
        {
            if (slot >= 0 && slot < entryPlayerIds.Length)
            {
                if (entryPlayerIds[slot] == -1)
                {
                    // Entry to Empty Slot
                    return Entry(slot);

                }
                else if (entryPlayerIds[slot] == Networking.LocalPlayer.playerId)
                {
                    // Exit from Slot
                    return Exit(slot);
                }
                else
                {
                    // Slot is Occupied
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public void ClearSlots()
        {
            if (Networking.IsOwner(gameObject)){
                for (int i = 0; i < entryPlayerIds.Length; i++)
                {
                    entryPlayerIds[i] = -1;
                }
                _Sync();
            }
        }

        public uint GetNumOfEntryPlayers()
        {
            uint i = 0;
            foreach (var id in entryPlayerIds)
            {
                if (id != -1) i++;
            }
            return i;
        }

        public string[] GetEntryPlayerNames()
        {
            string[] names = new string[4];
            for (int i = 0; i < names.Length; i++)
            {
                names[i] = GetEntryPlayerName(i);
            }
            return names;
        }

        public string GetEntryPlayerName(int slot)
        {
            if (slot >= 0 && slot < entryPlayerIds.Length)
            {
                var p = VRCPlayerApi.GetPlayerById(entryPlayerIds[slot]);
                if (p != null && p.IsValid())
                {
                    return p.displayName;
                }
                else
                {
                    return $"Player{slot + 1}";
                }
            }
            else
            {
                return $"Player{slot + 1}";
            }
        }

        public VRCPlayerApi GetPlayerApi(uint slot)
        {
            return VRCPlayerApi.GetPlayerById(entryPlayerIds[slot]);
        }

        bool Entry(int slot)
        {
            if (udonChips != null)
            {
                var money = (float)udonChips.GetProgramVariable("money");
                if (money < entryFee) return false;
                udonChips.SetProgramVariable("money", money - entryFee);
            }
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            entryPlayerIds[slot] = Networking.LocalPlayer.playerId;
            if (vrcDartsEventListener != null)
            {
                vrcDartsEventListener.OnEntryLocalPlayer();
            }
            RefillSlot();
            _Sync();
            return true;
        }

        bool Exit(int slot)
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            entryPlayerIds[slot] = -1;
            if (vrcDartsEventListener != null)
            {
                vrcDartsEventListener.OnExitLocalPlayer();
            }
            RefillSlot();
            if (udonChips != null)
            {
                var money = (float)udonChips.GetProgramVariable("money");
                udonChips.SetProgramVariable("money", money + entryFee);
            }
            _Sync();
            return true;
        }

        void RefillSlot()
        {
            for (int i = 0; i < entryPlayerIds.Length; i++)
            {
                if (entryPlayerIds[i] == -1)
                {
                    for (int j = i+1; j < entryPlayerIds.Length; j++)
                    {
                        if (entryPlayerIds[j] != -1)
                        {
                            entryPlayerIds[i] = entryPlayerIds[j];
                            entryPlayerIds[j] = -1;
                            break;
                        }
                    }
                }
            }
        }

        public void _Sync()
        {
            if (Networking.IsClogged)
            {
                SendCustomEventDelayedSeconds("_Sync", UnityEngine.Random.value/2, VRC.Udon.Common.Enums.EventTiming.Update);
            }
            else
            {
                RequestSerialization();
                machine.UpdatePlayerNames();
            }
        }

        public override void OnDeserialization()
        {
            machine.UpdatePlayerNames();
        }

        public override void OnOwnershipTransferred(VRCPlayerApi player)
        {
            if (Networking.LocalPlayer.playerId == player.playerId)
            {
                _Sync();
            }
        }
    }
}
