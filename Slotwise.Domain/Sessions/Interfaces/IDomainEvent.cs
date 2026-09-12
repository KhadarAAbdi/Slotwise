using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slotwise.Domain.Sessions.Interfaces
{
    public interface IDomainEvent
    {
        public DateTime OccuredOn { get; } 
    }
}
