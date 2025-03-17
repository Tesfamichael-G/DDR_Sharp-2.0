using SimulatorLib.Common;
using System;
using System.Collections.Generic;

namespace SimulatorLib.DeActivator;

public partial class RowBufferLRU
{

    public static UInt64 NULL = UInt64.MaxValue;

    public bool TryReplacement(List<RowBufferData> RowBuffers, out int BufferIndex)
    {
        BufferIndex = -1; ;

        long timestamp = Int64.MaxValue;

        RowBufferData row;

        for (int i = 0; i < RowBuffers.Count; i++)
        {
            row = RowBuffers[i];

            if (RowBuffers[i].Row == -1)
            {
                BufferIndex = i;
                return true;
            }
            if (row.TimeStamp < timestamp)
            {
                BufferIndex = i;
                timestamp = row.TimeStamp;
            }
        }

        return (BufferIndex != -1);

    }

    public bool TryAdd(List<RowBufferData> RowBuffers, int ROW, long Cycle, out RowBufferData? EvictedRow)
    {
        EvictedRow = null;

        int BufferIndex;
        bool CanReplace = TryReplacement(RowBuffers, out BufferIndex);

        if (CanReplace)
        {
            EvictedRow = new RowBufferData(RowBuffers[BufferIndex].Row, RowBuffers[BufferIndex].TimeStamp);
            RowBuffers[BufferIndex].Update(Cycle, ROW);
            return true;
        }

        return false;

    }


}


