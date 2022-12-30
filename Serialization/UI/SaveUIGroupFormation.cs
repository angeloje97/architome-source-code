using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome.Serialization
{
    [Serializable]
    public class SaveUIGroupFormation
    {
        [Serializable]
        public class GroupSpotData
        {
            public int entitySaveIndex;
            public Vector2 localPosition;

            public void Update(Vector2 position)
            {
                localPosition = position;
            }

            public GroupSpotData(EntityInfo entity, Vector2 spotPosition)
            {
                entitySaveIndex = entity.SaveIndex;
                localPosition = spotPosition;
            }

        }

        public List<GroupSpotData> groupSpotDatas;
        public void SaveEntity(EntityInfo entity, Vector2 localPosition)
        {
            if (entity.SaveIndex == -1) return;
            var spotData = EntitySpotData(entity);

            if(spotData == null)
            {
                spotData = new(entity, localPosition);
                groupSpotDatas.Add(spotData);
                return;
            }

            spotData.Update(localPosition);
        }

        public void LoadSpotData(EntityInfo entity, Transform uiTrans)
        {
            var spotData = EntitySpotData(entity);
            if (spotData == null) return;

            uiTrans.localPosition = spotData.localPosition;
        }


        public GroupSpotData EntitySpotData(EntityInfo entity)
        {
            groupSpotDatas ??= new();
            var saveIndex = entity.SaveIndex;
            foreach(var data in groupSpotDatas)
            {
                if(data.entitySaveIndex == saveIndex)
                {
                    return data;
                }
            }

            return null;
        }
    }
}
