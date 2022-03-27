
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.Components;

namespace AoinuWorks.VRCDarts
{
    public class VRCDart : UdonSharpBehaviour
    {
        [SerializeField] VRCDartsMachine machine;
        [SerializeField] float releaseSpeed = 5.5f;
        [Tooltip("If this value is set 1.0, Dart always will be thrown with velocity of releaseSpeed.")] public float speedCorrectionCoefficient = 0.75f;
        [SerializeField] Collider[] colliders;
        [HideInInspector] public Rigidbody rbody;
        VRC_Pickup pickup;
        VRCObjectSync objSync;

        void Start()
        {
            rbody = gameObject.GetComponent<Rigidbody>();
            pickup = (VRC_Pickup)gameObject.GetComponent(typeof(VRC_Pickup));
            objSync = (VRCObjectSync)gameObject.GetComponent(typeof(VRCObjectSync));
        }

        void FixedUpdate()
        {
            if (Networking.IsOwner(gameObject))
            {
                if (rbody.useGravity && !pickup.IsHeld)
                {
                    ApplyFlightsDrag();
                    CheckHits();
                }
            }
        }

        public override void OnDrop()
        {
            OnDartReleased();
        }

        public override void OnPickupUseUp()
        {
            ReleaseDart();
        }

        public override void OnPickup()
        {
            transform.parent = null;
            Unfreeze();
        }

        public void Freeze()
        {
            setKinematic(true);
        }

        public void Unfreeze()
        {
            setKinematic(false);
        }

        void setKinematic(bool state)
        {
            objSync.SetKinematic(state);
            objSync.SetGravity(!state);
            foreach (var col in colliders)
            {
                col.enabled = !state;
            }
        }

        void ReleaseDart()
        {
            pickup.Drop();
            OnDartReleased();
        }

        void OnDartReleased()
        {
            rbody.angularVelocity = Vector3.zero;
            var speed = rbody.velocity.magnitude;
            if (speed < releaseSpeed * 0.2f) return;
            rbody.velocity *= ((releaseSpeed - speed) * speedCorrectionCoefficient + speed) / speed;
        }

        void ApplyFlightsDrag()
        {
            if (rbody.velocity.magnitude > 0.1f)
            {
                var foward = Quaternion.LookRotation(rbody.velocity);
                rbody.MoveRotation(Quaternion.Slerp(rbody.rotation, foward, Time.deltaTime * 2f));
            }
        }

        void CheckHits()
        {
            RaycastHit hit;
            if (Physics.Raycast(rbody.position, rbody.velocity, out hit, rbody.velocity.magnitude * Time.deltaTime))
            {
                if (hit.collider != null && hit.collider.gameObject.GetInstanceID() == machine.gameObject.GetInstanceID())
                {
                    machine._OnDartsHit(hit.point);
                    Freeze();
                    rbody.velocity = Vector3.zero;
                    rbody.angularVelocity = Vector3.zero;
                    rbody.position = hit.point;
                    rbody.rotation = Quaternion.LookRotation(-machine.center.forward);
                }
            }
        }
    }
}
