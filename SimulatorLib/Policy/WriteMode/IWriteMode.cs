using SimulatorLib.CPU;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimulatorLib.Memory.WriteMode
{

    public interface IWriteMode
    {

        public bool DrainWrites { get; }

        public void Tick();

        public void AlertWriteMode(Common.COMMAND cmd);
       
        public void FORCE_Writes(bool write);


    }


}
