using System.Collections;
using UnityEngine;

namespace Architome
{
    public class SkillCheckHandler : MonoActor
    {
        [SerializeField] bool chancePerSecond;
        [SerializeField][Range(0f, 360f)] float angle;
        [SerializeField][Range(0f, 360f)] float range;

        public void HandleSkillChecks(TaskEventData eventData)
        {

        }
    }
}