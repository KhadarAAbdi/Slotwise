using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slotwise.Application.Sessions.Commands.CreateSession
{
    public sealed record class CreateSessionCommand
    {
        public string Title { get; }
        public DateTime Start { get; }
        public DateTime End { get; }
        public int Capacity { get; }

        public CreateSessionCommand(string title, DateTime start, DateTime end, int capacity)
        {
            Title = title;
            Start = start;
            End = end;
            Capacity = capacity;
        }
    }
}
