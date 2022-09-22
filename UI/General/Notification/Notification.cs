
using System;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using Architome.Enums;
using UnityEngine;
using TMPro;

namespace Architome
{
    [RequireComponent(typeof(NotificationFX))]
    public class Notification : MonoBehaviour
    {
        public Data current { get; set; }
        public NotificationManager.NotificationTypeInfo currentTypeInfo { get; set; }

        [Header("Notification Components")]
        public TextMeshProUGUI title;
        public TextMeshProUGUI description;
        public TextMeshProUGUI notificationNumber;
        public CanvasGroup canvasGroup;


        public SizeFitter sizeFitter;
        public ArchButton dismissButton;

        [Header("Bins")]
        public Transform footerBin;


        public Action<Notification> OnInterval, OnDismiss, OnComplete, OnStart, OnAppear, OnFirstAppear, OnHide, BeforeHide, OnBump;
        

        public List<Task> tasksBeforeDismiss;
        public List<Task> tasksBeforeHide;

        bool fixingSelf;
        bool settingCanvas;
        bool appeared;

        protected virtual void Start()
        {
            HandleNotificationNumber();
        }

        
        void Update()
        {
        
        }

        void HandleNotificationNumber()
        {
            var notificationManager = NotificationManager.active;

            OnAppear += (Notification notification) => {
                var index = notificationManager.notifications.IndexOf(this);
                var count = notificationManager.notifications.Count;

                SetNotificationNumber($"{index+1}/{count}");

            };
        }

        public void AddActionEvent(NotificationEvent notificationEvent, Action action)
        {
            switch (notificationEvent)
            {
                case NotificationEvent.OnComplete:
                    OnComplete += (Notification notification) => { action(); };
                    break;
                case NotificationEvent.OnInterval:
                    OnComplete += (Notification notification) => { action(); };
                    break;
                case NotificationEvent.OnDismiss:
                    OnDismiss += (Notification notification) => { action(); };
                    break;
                case NotificationEvent.OnAppear:
                    OnAppear += (Notification notification) => { action(); };
                    break;

                case NotificationEvent.OnFirstAppear:
                    OnFirstAppear += (Notification notification) => { action(); };
                    break;
                case NotificationEvent.OnHide:
                    OnHide += (Notification notification) => { action(); };
                    break;
                case NotificationEvent.BeforeHide:
                    BeforeHide += (Notification notification) => { action(); };
                    break;
                case NotificationEvent.OnBump:
                    OnBump += (Notification notification) => { action(); };
                    break;
                default:
                    action();
                    break;
            }
        }


        public void SetNotification(Data data, NotificationManager.NotificationTypeInfo typeInfo)
        {
            current = data;
            currentTypeInfo = typeInfo;
            SetData(data);
            HandleDismissButton();
            HandleCanvas();

            void HandleDismissButton()
            {
                if (!data.dismissable) return;

                var dismissButton = Instantiate(this.dismissButton.gameObject, footerBin).GetComponent<ArchButton>();

                dismissButton.SetButton("Dismiss", () => {
                    _ = Dismiss();

                });
            }
        }

        async void HandleCanvas()
        {
            ArchUI.SetCanvas(canvasGroup, false);
            fixingSelf = true;

            int count = 3;

            while (count > 0)
            {
                sizeFitter.AdjustToSize();
                await Task.Delay(62);
                count--;
            }

            if (!fixingSelf) return;

            fixingSelf = false;

            appeared = true;
            OnFirstAppear?.Invoke(this);
            OnAppear?.Invoke(this);

            ArchUI.SetCanvas(canvasGroup, true);
        }

        public async Task SetCanvas(bool active, bool forceSetActive = false)
        {
            if (forceSetActive)
            {
                fixingSelf = false;
            }
            else
            {
                while (fixingSelf)
                {
                    await Task.Yield();
                }

                while (settingCanvas)
                {
                    await Task.Yield();
                }
            }

            settingCanvas = true;

            if (!active)
            {
                tasksBeforeHide = new();
                BeforeHide?.Invoke(this);

                foreach (var task in tasksBeforeHide)
                {
                    await task;
                }
            }

            ArchUI.SetCanvas(canvasGroup, active);

            if (active)
            {
                OnAppear?.Invoke(this);
            }
            else
            {
                OnHide?.Invoke(this);
            }

            settingCanvas = false;
        }

        public void Bump()
        {
            OnBump?.Invoke(this);
        }

        public virtual void SetData(Data data)
        {
            title.text = data.name;
            description.text = data.description;
        }

        public async Task Dismiss()
        {
            tasksBeforeDismiss = new();
            OnDismiss?.Invoke(this);

            await Task.WhenAll(tasksBeforeDismiss);


            ArchAction.Yield(() => Destroy(gameObject));
        }

        public void SetNotificationNumber(string text)
        {
            notificationNumber.text = text;
        }


        [Serializable]
        public class Data
        {
            public NotificationType type;
            public string name;
            public string description;
            public string tips;

            public bool dismissable = true;

            public Data(NotificationType type)
            {
                this.type = type;
            }
        }
    }
}
