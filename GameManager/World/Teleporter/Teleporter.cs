using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Architome
{
    [RequireComponent(typeof(TeleporterFX))]
    public class Teleporter : MonoBehaviour
    {
        public Transform spot;
        public Teleporter otherTeleporter;
        public float teleportDelay;

        LayerMask groundLayer, obstructionLayer;
        WorldActions worldActions;

        bool teleporting;

        public Dictionary<EntityInfo, Vector3> floatingEntities;


        public Action<EntityInfo, Vector3> BeforeTeleportEntity, AfterTeleportEntity;

        public enum Event
        {
            BeforeTeleport,
            AfterTeleport,
        }

        public void AddEventAction(Event trigger, Action<EntityInfo, Vector3> action)
        {
            switch (trigger)
            {
                case Event.BeforeTeleport:
                    BeforeTeleportEntity += action;
                    break;
                case Event.AfterTeleport:
                    AfterTeleportEntity += action;
                    break;
            }
        }


        private void Start()
        {
            GetDependencies();

        }
        void GetDependencies()
        {
            var layerMasksData = LayerMasksData.active;
            worldActions = WorldActions.active;
            if (layerMasksData)
            {
                groundLayer = layerMasksData.walkableLayer;
                obstructionLayer = layerMasksData.structureLayerMask;
            }
        }


        // Update is called once per frame
        public async void TeleportEntityToSpot(EntityInfo entity)
        {
            if (spot == null) return;

            var offset = entity.GetComponent<BoxCollider>().size.y / 2;



            BeforeTeleportEntity?.Invoke(entity, spot.position);

            await Task.Delay((int)(teleportDelay * 1000));

            worldActions.MoveEntity(entity, spot.position, groundLayer);


            AfterTeleportEntity?.Invoke(entity, entity.transform.position);
        }

        public async void TeleportPartyToSpot(PartyInfo party)
        {

            teleporting = true;

            var cameraAnchor = CameraAnchor.active;

            cameraAnchor.DelayFollowingUntil(TeleportingFinish());
            floatingEntities = new();
            HandleFloatingEntities();


            var members = party.members;

            var positions = V3Helper.PointsAroundPosition(otherTeleporter.spot.transform.position, members.Count, 2.5f, obstructionLayer);
                

            foreach (var member in members)
            {
                BeforeTeleportEntity?.Invoke(member, member.transform.position);
                await Task.Delay((int)(teleportDelay * 1000));
                AfterTeleportEntity?.Invoke(member, member.transform.position);
                member.transform.position = spot.position + new Vector3(0, 100, 0);
                
            }

            cameraAnchor.SetTarget(otherTeleporter.spot.transform);



            for(int i = 0; i < members.Count; i++)
            {
                var member = members[i];
                floatingEntities.Remove(member);

                otherTeleporter.BeforeTeleportEntity?.Invoke(member, positions[i]);
                await Task.Delay((int)(teleportDelay * 1000));
                worldActions.MoveEntity(member, positions[i], groundLayer);
                otherTeleporter.AfterTeleportEntity?.Invoke(member, positions[i]);
            }

            teleporting = false;
        }

        public async void HandleFloatingEntities()
        {
            while (teleporting)
            {
                await Task.Yield();
                foreach (var floating in floatingEntities)
                {
                    floating.Key.transform.position = floating.Value;
                }
            }
        }

        public async Task TeleportingFinish()
        {
            while (teleporting)
            {
                await Task.Yield();
            }
        }
    }
}
