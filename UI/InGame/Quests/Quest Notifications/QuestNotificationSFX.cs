using UnityEngine;

namespace Architome
{
    public class QuestNotificationSFX : MonoBehaviour
    {
        public AudioClip completed, failed, started;

        QuestNotification notification;
        AudioManager audioManager;
        void Start()
        {
            GetDependencies();
        }

        void GetDependencies()
        {
            notification = GetComponent<QuestNotification>();
            audioManager = GetComponent<AudioManager>();

            if (notification && audioManager)
            {
                notification.OnQuestCompleted += () => {
                    audioManager.PlayAudioClip(completed);
                };

                notification.OnQuestFailed += () => {
                    audioManager.PlayAudioClip(failed);
                };

                notification.OnQuestStart += () => {
                    audioManager.PlayAudioClip(started);
                };
            }
        }
    }
}
