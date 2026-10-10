using System.Collections.Generic;
using StationJam.Entities;

namespace StationJam.Core
{
    public sealed class LevelSpawnResult
    {
        public Passenger BufferPassenger { get; }

        public IReadOnlyList<TrainWagon> Wagons { get; }

        public LevelSpawnResult(
            Passenger bufferPassenger,
            IReadOnlyList<TrainWagon> wagons)
        {
            BufferPassenger =
                bufferPassenger;

            Wagons =
                wagons;
        }
    }
}