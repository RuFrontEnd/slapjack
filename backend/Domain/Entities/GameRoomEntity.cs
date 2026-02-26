using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    internal class GameRoomEntity
    {
        public string? RoomCode
        {
            get; set;
        }

        public List<string> PlayerIds { get; set; } = new();

        public bool IsStarted { get; set; }
    }
}
