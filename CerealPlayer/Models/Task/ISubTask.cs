using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CerealPlayer.Models.Task
{
    public interface ISubTask
    {
        void Start();
        void Stop();
    }
}
