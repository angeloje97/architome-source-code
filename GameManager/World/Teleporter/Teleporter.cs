using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

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

        public UnityEvent AfterTeleportEntityUnity;
        public UnityEvent BeforeTeleportEntityUnity;

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
            if (teleporting) return;
            teleporting = true;

            var offset = entity.GetComponent<BoxCollider>().size.y / 2;



            BeforeTeleportEntity?.Invoke(entity, spot.position);

            await Task.Delay((int)(teleportDelay * 1000));

            worldActions.MoveEntity(entity, spot.position, groundLayer);


            AfterTeleportEntity?.Invoke(entity, entity.transform.position);
            AfterTeleportEntityUnity?.Invoke();
            teleporting = false;
        }

        public async void TeleportEntityDoingTask(bool setCameraAnchor)
        {
            if (otherTeleporter == null) return;
            if (teleporting) return;
            var entity = EntityDoingTask();
            if (entity == null) return;
            var cameraAnchor = CameraAnchor.active;

            teleporting = true;

            var offset = entity.GetComponent<BoxCollider>().size.y / 2;

            BeforeTeleportEntity?.Invoke(entity, entity.transform.position);

            cameraAnchor.DelayFollowingUntil(TeleportingFinish());

            await Task.Delay((int)(teleportDelay * 1000));



            AfterTeleportEntity?.Invoke(entity, entity.transform.position);
            entity.transform.position = transform.position + new Vector3(0, 100, 0);

            if (setCameraAnchor)
            {
                cameraAnchor.SetTarget(otherTeleporter.transform, true);
            }

            otherTeleporter.BeforeTeleportEntity?.Invoke(entity, otherTeleporter.spot.position);

            await Task.Delay((int) otherTeleporter.teleportDelay * 1000);

            worldActions.MoveEntity(entity, otherTeleporter.spot.position, groundLayer);

            otherTeleporter.AfterTeleportEntity?.Invoke(entity, entity.transform.position);

            AfterTeleportEntityUnity?.Invoke();
            teleporting = false;


        }

        public void RevealRoom()
        {
            var room = GetComponentInParent<RoomInfo>();

            room.ShowRoom(true, transform.position, true);
        }

        public void TeleportPartyToSpot()
        {
            var gameManager = GameManager.active;
            if (gameManager == null) return;
            if (gameManager.playableParties == null || gameManager.playableParties.Count <= 0) return;
            var party = gameManager.playableParties[0];

            TeleportPartyToSpot(party);
        }
        public async void TeleportPartyToSpot(PartyInfo party)
        {
            if (teleporting) return;
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
                await Task.Delay((int)(teleportDelay/2 * 1000));
                
            }

            foreach (var member in members)
            {
                await Task.Delay((int) (teleportDelay / 2 * 1000));

                AfterTeleportEntity?.Invoke(member, member.transform.position);
                member.transform.position = spot.position + new Vector3(0, 100, 0);
            }


            await Task.Delay((int)(teleportDelay * 1000));
            cameraAnchor.SetTarget(otherTeleporter.spot.transform, true);
            otherTeleporter.RevealRoom();



            for(int i = 0; i < members.Count; i++)
            {
                var member = members[i];
                floatingEntities.Remove(member);

                otherTeleporter.BeforeTeleportEntity?.Invoke(member, positions[i]);
                await Task.Delay((int)(otherTeleporter.teleportDelay/2 * 1000));
            }

            for (int i = 0; i < members.Count; i++)
            {
                var member = members[i];
                await Task.Delay((int)(otherTeleporter.teleportDelay / 2 * 1000));
                worldActions.MoveEntity(member, positions[i], groundLayer);
                otherTeleporter.AfterTeleportEntity?.Invoke(member, positions[i]);
                otherTeleporter.AfterTeleportEntityUnity?.Invoke();
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

        public EntityInfo EntityDoingTask()
        {
            var entityLayer = LayerMasksData.active.entityLayerMask;

            var entities = Physics.OverlapSphere(transform.position, 8f, entityLayer);

            for (int i = 0; i < entities.Length; i++)
            {
                var entity = entities[i].GetComponent<EntityInfo>();

                if (entity == null) continue;
                if (entity.target != transform) continue;
                return entity;
            }

            return null;
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
