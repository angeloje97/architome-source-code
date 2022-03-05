using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartyFormation : MonoBehaviour
{
    // Start is called before the first frame update
    public List<Transform> spots;
    public List<Vector2> spotPositions;
    public Transform radiusCircle;
    public KeyBindings keyBindings;

    public string actionRelease;
    public bool isHolding;
    public void GetSpots()
    {
        foreach(Transform child in transform)
        {
            if(child.name.Equals("RadiusCircle"))
            {
                radiusCircle = child.transform;
            }
            else
            {
                spots.Add(child);

                var localPos = child.localPosition;
                localPos = localPos / (radiusCircle.localScale.x / 2);

                spotPositions.Add(new Vector2(localPos.x, localPos.z));
               
            }
        }

        if(keyBindings == null && GMHelper.KeyBindings())
        {
            keyBindings = GMHelper.KeyBindings();
        }
    }
    void Start()
    {
        GetSpots();
    }

    // Update is called once per frame
    void Update()
    {
        HandleRotation();
        HandleUserInputs();
    }
    public void MoveFormation(Vector3 location)
    {

        var originalLocation = location;
        location.y = transform.position.y;

        transform.LookAt(location);

        transform.position = originalLocation;
        HandleSpotPosition();
        StartCoroutine(HoldDelay());
    }

    public void HandleSpotPosition()
    {
        UpdateSpot();
        FixSpot();

        void UpdateSpot()
        {
            Debugger.InConsole(3458, $"Updating Spot");
            for(int i = 0; i < spots.Count; i++)
            {
                if(i >= spotPositions.Count) { continue; }
                var radius = radiusCircle.localScale.x;
                var currentPos = spotPositions[i] * (radius / 2);

                spots[i].transform.localPosition = new Vector3(currentPos.x, 0, currentPos.y);
            }
        }

        void FixSpot()
        {
            for(int i = 0; i < spots.Count; i++)
            {
                if(GMHelper.LayerMasks())
                {
                    var structureLayer = GMHelper.LayerMasks().wallLayer;
                    spots[i].transform.position = V3Helper.InterceptionPoint(radiusCircle.transform.position, spots[i].transform.position, structureLayer);
                }
                
            }
        }
    }

    public void HandleRotation()
    {
        if(isHolding)
        {
            var location = Mouse.CurrentPosition();
            location.y = transform.position.y;

            transform.LookAt(location);
            HandleSpotPosition();
        }
    }
    public IEnumerator HoldDelay()
    {
        yield return new WaitForSeconds(.125f);
        {
            if (Input.GetKey(keyBindings.keyBinds[actionRelease]))
            {
                isHolding = true;
            }
        }
    }
    void HandleUserInputs()
    {
        if(isHolding)
        {
            if (Input.GetKeyUp(keyBindings.keyBinds[actionRelease]))
            {
                isHolding = false;
            }
        }
    }
    
}
