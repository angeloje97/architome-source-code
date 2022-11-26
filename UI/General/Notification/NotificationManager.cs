using System;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;

namespace Architome
{
    public class NotificationManager : MonoBehaviour
    {
        public static NotificationManager active;

        [Serializable]
        public class NotificationTypeInfo
        {
            [HideInInspector] public string name;
            public NotificationType type;

            public Color color;
            public AudioClip audioClip;

            public NotificationTypeInfo(NotificationType type)
            {
                this.type = type;
                this.name = type.ToString();
            }
        }

        public List<NotificationTypeInfo> typeInfos;

        [Header("Prefabs")]
        public Notification general;
        public Direction direction;

        [Header("Components")]
        public Transform notificationBin;
        public CanvasGroup notificationBinCanvasGroup;
        public List<Notification> notifications;
        public Dictionary<NotificationType, NotificationTypeInfo> typeMap;
        

        [Header("Actions")]
        [SerializeField] bool updateTypes;

        public Action<Notification> OnNewNotification;

        int activeIndex;
        Notification activeNotification;
        bool transitioning;
        

        static bool isVisible = true;
        static bool started = false;

        void Start()
        {
            CreateTypeMap();
            DeleteExistingNotifications();

            if (!started)
            {
                started = true;
                isVisible = true;
            }
            UpdateVisibility();
        }

        async void DeleteExistingNotifications()
        {
            foreach(var notification in notificationBin.GetComponentsInChildren<Notification>())
            {
                await notification.Dismiss();
            }
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        private void OnValidate()
        {
            HandleCreateTypes();
        }

        private void Awake()
        {
            active = this;
        }


        void CreateTypeMap()
        {
            typeMap = new();
            foreach (var typeInfo in typeInfos)
            {
                if (typeMap.ContainsKey(typeInfo.type)) continue;
                typeMap.Add(typeInfo.type, typeInfo);
            }
        }

        public void ToggleVisibility()
        {
            isVisible = !isVisible;
            UpdateVisibility();
        }

        void UpdateVisibility()
        {
            ArchUI.SetCanvas(notificationBinCanvasGroup, isVisible);
        }

        void HandleCreateTypes()
        {
            if (!updateTypes) return;
            updateTypes = false;

            if (typeInfos == null) typeInfos = new();

            foreach (NotificationType type in Enum.GetValues(typeof(NotificationType)))
            {
                bool add = true;
                for (int i = 0; i < typeInfos.Count; i++)
                {
                    if (typeInfos[i].type != type) continue;

                    if (!add)
                    {
                        typeInfos.RemoveAt(i);
                        i--;
                        continue;
                    }

                    add = false;
                }

                if (add)
                {
                    typeInfos.Add(new(type));
                }
            }
        }

        public async Task<Notification> CreateNotification(Notification.Data data)
        {
            if (general == null) return null;

            await CloseActiveNotification();

            var notification = Instantiate(general, notificationBin).GetComponent<Notification>();

            notification.SetNotification(data, typeMap[data.type]);

            AddNotification(notification);

            return notification;
        }
        public async Task<Direction> CreateDirectionNotification(Notification.Data data)
        {
            if (direction == null) return null;

            await CloseActiveNotification();

            var directionNotification = Instantiate(direction, notificationBin).GetComponent<Direction>();

            directionNotification.SetNotification(data, typeMap[data.type]);

            AddNotification(directionNotification);

            return directionNotification;
        }
        void AddNotification(Notification notification)
        {
            if (notifications == null) return;
            if (notifications.Contains(notification)) return;


            activeNotification = notification;

            activeIndex = notifications.Count;

            notification.OnDismiss += (Notification notif) =>
            {
                var index = notifications.IndexOf(notif);
                notifications.RemoveAt(index);

                _= SetNotification(index - 1);
            };

            notifications.Add(notification);
            OnNewNotification?.Invoke(notification);
        }

        public async Task CloseActiveNotification()
        {
            if (activeNotification)
            {
                await activeNotification.SetCanvas(false);
            }

        }

        public async Task SetNotification(int index)
        {
            while(transitioning)
            {
                await Task.Yield();
            }


            if (notifications.Count == 0) return;
            if(index >= notifications.Count) { return; }
            if (index < 0)
            {
                index = notifications.Count - 1;
            }
            if (activeIndex == index) return;



            transitioning = true;
            await activeNotification.SetCanvas(false);

            activeNotification = notifications[index];
            await activeNotification.SetCanvas(true);
            activeIndex = index;

            transitioning = false;
        }

        public async void NextNotification()
        {
            await SetNotification((activeIndex + 1) % notifications.Count);
        }

        public async void PreviousNotification()
        {
            
            await SetNotification((activeIndex - 1) % notifications.Count);
        }

    }
}
