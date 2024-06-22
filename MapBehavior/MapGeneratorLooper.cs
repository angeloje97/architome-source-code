using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Architome
{
    public class MapGeneratorLooper : MonoBehaviour
    {
        List<RoomInfo> skeleton, available;
        MapRoomGenerator roomGenerator;

        public bool limitLoops;
        public int maxLoops;
        public int currentLoops;
        public float loopInterval;
        public void Start()
        {
            GetDependencies();
        }

        void GetDependencies()
        {
            roomGenerator = MapRoomGenerator.active;
            if (roomGenerator == null) return;

            skeleton = roomGenerator.skeletonRooms;
            available = roomGenerator.availableRooms;
            roomGenerator.ignoreBackgorund = true;
            roomGenerator.OnRoomsGenerated += OnRoomsGenerated;
        }

        async void OnRoomsGenerated(MapRoomGenerator generator)
        {
            if (limitLoops && currentLoops >= maxLoops) return;
            if (!generator.CanGenerate()) return;

            await Task.Delay((int)loopInterval * 1000);

            generator.DestroyRooms();

            generator.availableRooms = available.ToList();
            generator.skeletonRooms = skeleton.ToList();
            generator.GenerateRooms();
            currentLoops++;
        }

    }
}
