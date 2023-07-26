using Architome.History;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Architome
{
    public abstract class GuildHistoryDisplay : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI display;
        protected List<string> currentLogs;
        protected SaveHistory saveHistory;
        List<string> displayingLogs;

        protected DataMap currentDataMap;



        protected virtual void Start()
        {
            GetDependencies();
            UpdateLogs();
        }

        protected virtual void GetDependencies()
        {
            saveHistory = SaveHistory.active;
            currentDataMap = DataMap.active;
        }

        public void UpdateLogs()
        {
            currentLogs ??= new();
            displayingLogs = new();

            foreach(var log in currentLogs)
            {
                displayingLogs.Add(log);
            }
            

            display.text = ArchString.NextLineList(currentLogs);
        }

    }
}
