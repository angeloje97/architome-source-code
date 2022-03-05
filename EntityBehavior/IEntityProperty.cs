using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome;

public interface IEntityProperty
{
    // Start is called before the first frame update
    EntityInfo entityInfo { get; set; }
    GameObject entityObject { get; set; }
}