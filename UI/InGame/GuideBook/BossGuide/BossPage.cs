using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class BossPage : MonoBehaviour
    {
        public EntityInfo currentBoss;
        public void SetBossPage(EntityInfo entity)
        {
            currentBoss = entity;
        }
    }
}
