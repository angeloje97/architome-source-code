using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;
using UnityEngine.Events;

namespace Architome
{
    public class PartyFormation : MonoBehaviour
    {
        // Start is called before the first frame update
        public List<Transform> spots;
        public List<Vector2> spotPositions;
        public Transform radiusCircle;
        public KeyBindings keyBindings;

        public string actionRelease;
        public bool isHolding;

        public Action<bool> OnHoldingChange { get; set; }
        
        [Serializable]
        public struct Effects
        {
            public List<ParticleSystem> particlesToPlay;

            public void SetParticles(bool val)
            {
                particlesToPlay ??= new();
                foreach(var particle in particlesToPlay)
                {
                    if (val)
                    {
                        particle.Play(true);
                    }
                    else
                    {
                        particle.Stop(true);
                    }
                }
            }
        }

        public Effects effects;

        public Action OnMove { get; set; }
        public UnityEvent OnUnityMove;
        public UnityEvent OnHoldStart;
        public UnityEvent OnHoldEnd;
        public void GetSpots()
        {
            foreach (Transform child in transform)
            {
                if (child.name.Equals("RadiusCircle"))
                {
                    radiusCircle = child.transform;
                }

                if (child.GetComponent<PartyFormationSpotBehavior>())
                {
                    spots.Add(child);
                }
            }

            keyBindings = GMHelper.KeyBindings();
        }
        void Start()
        {
            GetSpots();
        }

        // Update is called once per frame
        public void MoveFormation(Vector3 location)
        {

            var originalLocation = location;
            location.y = transform.position.y;

            OnMove?.Invoke();
            OnUnityMove?.Invoke();
            transform.LookAt(location);

            transform.position = originalLocation;
            HandleSpotPosition();

            HandleHolding();
        }

        public void HandleSpotPosition()
        {
            UpdateSpot();

            void UpdateSpot()
            {
                Debugger.InConsole(3458, $"Updating Spot");
                for (int i = 0; i < spots.Count; i++)
                {
                    if (i >= spotPositions.Count) { continue; }
                    var radius = radiusCircle.localScale.x;
                    var currentPos = spotPositions[i] * (radius / 2);


                    spots[i].transform.localPosition = new Vector3(currentPos.x, 0, currentPos.y);

                    if (LayerMasksData.active == null) return;

                    spots[i].transform.position = V3Helper.InterceptionPoint(radiusCircle.transform.position, spots[i].transform.position, LayerMasksData.active.wallLayer);
                    spots[i].transform.position = V3Helper.GroundPosition(spots[i].transform.position, LayerMasksData.active.walkableLayer, 5f);
                    
                }
            }
        }

        public void HandleRotation()
        {
            if (isHolding)
            {
                var location = Mouse.CurrentPosition();

                if (location == new Vector3())
                {
                    location = Mouse.RelativePosition(transform.position);
                }
                location.y = transform.position.y;

                transform.LookAt(location);
                HandleSpotPosition();
            }
        }
        public IEnumerator HoldDelay()
        {
            OnHoldingChange?.Invoke(true);
            yield return new WaitForSeconds(.125f);


            if (Input.GetKey(keyBindings.keyBinds[actionRelease]))
            {
                isHolding = true;
            }
            else
            {
                OnHoldingChange?.Invoke(false);
            }
        }

        async void HandleHolding()
        {
            if (isHolding) return;
            OnHoldingChange?.Invoke(true);
            OnHoldStart?.Invoke();
            effects.SetParticles(true);
            await Task.Delay(125);
            isHolding = true;

            while (Input.GetKey(keyBindings.keyBinds[actionRelease]))
            {
                await Task.Yield();
                HandleRotation();
            }

            isHolding = false;
            OnHoldEnd?.Invoke();
            effects.SetParticles(false);

            OnHoldingChange?.Invoke(false);
        }

    }

}