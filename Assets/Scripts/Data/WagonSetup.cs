using System.Collections.Generic;
using StationJam.Entities;

namespace StationJam.Data
{
    [System.Serializable]
    public class WagonSetup
    {
        public ColorType TargetColor;

        public int Capacity = 4;

        public List<ColorType> StartingPassengers =
            new List<ColorType>();
    }
}