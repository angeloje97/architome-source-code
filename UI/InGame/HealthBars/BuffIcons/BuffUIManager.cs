using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Architome
{
    public class BuffUIManager : MonoBehaviour
    {
        // Start is called before the first frame update

        public EntityInfo entity;

        [SerializeField]
        private GameObject buffIconPrefab;

        public void SetEntity(EntityInfo newEntity)
        {
            HandlePreviousEntity();
            HandleNewEntity(newEntity);

            void HandlePreviousEntity()
            {
                if (entity == null) return;

                entity.OnNewBuff -= OnNewBuff;

                var buffIconBehaviors = GetComponentsInChildren<BuffIconBehavior>();

                foreach (var icon in buffIconBehaviors)
                {
                    icon.DestroySelf();
                }
            }

            void HandleNewEntity(EntityInfo newEntity)
            {
                this.entity = newEntity;
                entity.OnNewBuff += OnNewBuff;

                foreach (var buff in entity.Buffs().Buffs())
                {
                    CreateBuff(buff);
                }
            }
        }

        

        public void OnNewBuff(BuffInfo buff, EntityInfo source)
        {
            if (!buffIconPrefab) return;
            CreateBuff(buff);
            
        }

        public void CreateBuff(BuffInfo buff)
        {
            Instantiate(buffIconPrefab, transform).GetComponent<BuffIconBehavior>()?.SetBuff(buff);
            SortBuffs();
        }

        public void SortBuffs()
        {
            var buffs = GetComponentsInChildren<BuffIconBehavior>();
            buffs = buffs.OrderBy(iconBehavior => (int) iconBehavior.buff.buffTargetType).ToArray();

            foreach (var buff in buffs)
            {
                buff.transform.SetAsLastSibling();
            }
        }
    }

}