using System;



namespace SimulatorLib.Common;

public class MemoryMap : IAddressTranslator
{


    Parameters Param;
    char[] cmapping;
    ReadOnlyMemory<char> mmapping;
    public MemoryMap(Parameters parameters)
    {
        Param = parameters;
        string[] MappingArray = Param.ADDRESS_MAPPING.ToString().Split("_");
        mmapping = Param.ADDRESS_MAPPING.ToString().AsMemory();

        cmapping = new char[MappingArray.Length];
        for (int i = 0; i < MappingArray.Length; i++)
        {
            cmapping[i] = CharValue(MappingArray[i]);
        }

        char CharValue(string s)
        {
            switch (s.ToUpper())
            {
                case "ROW": return 'r';
                case "RANK": return 'R';
                case "BANKG": return 'G';
                case "BANK": return 'B';
                case "CHAN": return 'C';
                case "COL": return 'c';
            }
            return '0';
        }

    }

    public MemoryAddress Translate3(long physical_address)
    {
        int tx = (Param.PrefetchSize * Param.ChannelWidth / 8);
        int tx_bits = (int)Math.Log(tx, 2);

        string[] mapping = Param.ADDRESS_MAPPING.ToString().Split("_");

        int channelBits = (int)Math.Log(Param.NUM_CHANNELS, 2);
        int rankBits = (int)Math.Log(Param.NUM_RANKS, 2);
        int bankBits = (int)Math.Log(Param.NUM_BANKS, 2);
        int bankGBits = (Param.NUM_BANK_GROUPS == 0) ? 0 : (int)Math.Log(Param.NUM_BANK_GROUPS, 2);
        int rowBits = (int)Math.Log(Param.NUM_ROWS, 2);
        int colBits = (int)Math.Log(Param.NUM_COLUMNS, 2);

        colBits -= (int)Math.Log(Param.PrefetchSize, 2);

        int byteOffset = tx_bits;

        long ADDR = physical_address >> byteOffset;

        MemoryAddress memAddr = new MemoryAddress();
        memAddr.BlockAddress = physical_address >> byteOffset;

        for (int m = mapping.Length - 1; m >= 0; m--)
        {
            switch (mapping[m].ToUpper())
            {
                case "ROW":
                    memAddr.Row = (int)ADDR & ((1 << rowBits) - 1);
                    ADDR >>= rowBits;
                    break;
                case "RANK":
                    memAddr.Rank = (int)ADDR & ((1 << rankBits) - 1);
                    ADDR >>= rankBits;
                    break;
                case "BANKG":
                    memAddr.BankGroup = (int)ADDR & ((1 << bankGBits) - 1);
                    ADDR >>= bankGBits;
                    break;
                case "BANK":
                    memAddr.Bank = (int)ADDR & ((1 << bankBits) - 1);
                    ADDR >>= bankBits;
                    break;
                case "CHAN":
                    memAddr.Channel = (int)ADDR & ((1 << channelBits) - 1);
                    ADDR >>= channelBits;
                    break;
                case "COL":
                    memAddr.Column = (int)ADDR & ((1 << colBits) - 1);
                    ADDR >>= colBits;
                    break;

                default:
                    break;
            }
        }


        string line = $"{physical_address:x}\t{physical_address}\t{memAddr.Bank}\t{memAddr.Row}\t{memAddr.Column}";

        return (memAddr);

    }

    public MemoryAddress Translate2(long physical_address)
    {

        string[] mapping = Param.ADDRESS_MAPPING.ToString().Split("_");

        int byteOffset = Param.tx_bits;

        long ADDR = physical_address >> byteOffset;

        MemoryAddress memAddr = new MemoryAddress();
        memAddr.BlockAddress = physical_address >> byteOffset;

        for (int m = mapping.Length - 1; m >= 0; m--)
        {
            switch (mapping[m].ToUpper())
            {
                case "ROW":
                    memAddr.Row = (int)ADDR & ((1 << Param.rowBits) - 1);
                    ADDR >>= Param.rowBits;
                    break;
                case "RANK":
                    memAddr.Rank = (int)ADDR & ((1 << Param.rankBits) - 1);
                    ADDR >>= Param.rankBits;
                    break;
                case "BANKG":
                    memAddr.BankGroup = (int)ADDR & ((1 << Param.bankGBits) - 1);
                    ADDR >>= Param.bankGBits;
                    break;
                case "BANK":
                    memAddr.Bank = (int)ADDR & ((1 << Param.bankBits) - 1);
                    ADDR >>= Param.bankBits;
                    break;
                case "CHAN":
                    memAddr.Channel = (int)ADDR & ((1 << Param.channelBits) - 1);
                    ADDR >>= Param.channelBits;
                    break;
                case "COL":
                    memAddr.Column = (int)ADDR & ((1 << Param.colBits) - 1);
                    ADDR >>= Param.colBits;
                    break;

                default:
                    break;
            }
        }


        string line = $"{physical_address:x}\t{physical_address}\t{memAddr.Bank}\t{memAddr.Row}\t{memAddr.Column}";

        return (memAddr);



    }

    public MemoryAddress Translate1(long physical_address)
    {

        long ADDR = physical_address >> Param.tx_bits;

        MemoryAddress memAddr = new MemoryAddress();
        memAddr.BlockAddress = physical_address >> Param.tx_bits; 

        for (int m = cmapping.Length - 1; m >= 0; m--)
        {
            switch (cmapping[m])
            {
                case 'r':
                    memAddr.Row = (int)ADDR & ((1 << Param.rowBits) - 1);
                    ADDR >>= Param.rowBits;
                    break;
                case 'R':
                    memAddr.Rank = (int)ADDR & ((1 << Param.rankBits) - 1);
                    ADDR >>= Param.rankBits;
                    break;
                case 'G':
                    memAddr.BankGroup = (int)ADDR & ((1 << Param.bankGBits) - 1);
                    ADDR >>= Param.bankGBits;
                    break;
                case 'B':
                    memAddr.Bank = (int)ADDR & ((1 << Param.bankBits) - 1);
                    ADDR >>= Param.bankBits;
                    break;
                case 'C':
                    memAddr.Channel = (int)ADDR & ((1 << Param.channelBits) - 1);
                    ADDR >>= Param.channelBits;
                    break;
                case 'c':
                    memAddr.Column = (int)ADDR & ((1 << Param.colBits) - 1);
                    ADDR >>= Param.colBits;
                    break;

                default:
                    break;
            }
        }


        return (memAddr);



    }

    public MemoryAddress sTranslate(long physical_address)
    {

        long ADDR = physical_address >> Param.tx_bits;

        MemoryAddress memAddr = new MemoryAddress();
        memAddr.BlockAddress = physical_address >> Param.tx_bits; 

        ReadOnlySpan<char> mapping = Param.ADDRESS_MAPPING.ToString().AsSpan();

        int index = mapping.LastIndexOf("_");

        for ( ; index > 0; )
        {
            switch (mapping.Slice(index+1).ToString())
            {
                case "ROW":
                    memAddr.Row = (int)ADDR & ((1 << Param.rowBits) - 1);
                    ADDR >>= Param.rowBits;
                    break;
                case "RANK":
                    memAddr.Rank = (int)ADDR & ((1 << Param.rankBits) - 1);
                    ADDR >>= Param.rankBits;
                    break;
                case "BANKG":
                    memAddr.BankGroup = (int)ADDR & ((1 << Param.bankGBits) - 1);
                    ADDR >>= Param.bankGBits;
                    break;
                case "BANK":
                    memAddr.Bank = (int)ADDR & ((1 << Param.bankBits) - 1);
                    ADDR >>= Param.bankBits;
                    break;
                case "CHAN":
                    memAddr.Channel = (int)ADDR & ((1 << Param.channelBits) - 1);
                    ADDR >>= Param.channelBits;
                    break;
                case "COL":
                    memAddr.Column = (int)ADDR & ((1 << Param.colBits) - 1);
                    ADDR >>= Param.colBits;
                    break;

                default:
                    break;
            }
            mapping = mapping.Slice(0, index);
            index = mapping.LastIndexOf("_");
        }


        return (memAddr);



    }
    public MemoryAddress mTranslate(long physical_address)
    {

        long ADDR = physical_address >> Param.tx_bits;

        MemoryAddress memAddr = new MemoryAddress();
        memAddr.BlockAddress = physical_address >> Param.tx_bits; 

        ReadOnlySpan<char> mapping = mmapping.Span;

        int index = mapping.LastIndexOf("_");

        for ( ; index > 0; )
        {
            switch (mapping.Slice(index+1).ToString())
            {
                case "ROW":
                    memAddr.Row = (int)ADDR & ((1 << Param.rowBits) - 1);
                    ADDR >>= Param.rowBits;
                    break;
                case "RANK":
                    memAddr.Rank = (int)ADDR & ((1 << Param.rankBits) - 1);
                    ADDR >>= Param.rankBits;
                    break;
                case "BANKG":
                    memAddr.BankGroup = (int)ADDR & ((1 << Param.bankGBits) - 1);
                    ADDR >>= Param.bankGBits;
                    break;
                case "BANK":
                    memAddr.Bank = (int)ADDR & ((1 << Param.bankBits) - 1);
                    ADDR >>= Param.bankBits;
                    break;
                case "CHAN":
                    memAddr.Channel = (int)ADDR & ((1 << Param.channelBits) - 1);
                    ADDR >>= Param.channelBits;
                    break;
                case "COL":
                    memAddr.Column = (int)ADDR & ((1 << Param.colBits) - 1);
                    ADDR >>= Param.colBits;
                    break;

                default:
                    break;
            }
            mapping = mapping.Slice(0, index);
            index = mapping.LastIndexOf("_");
        }


        return (memAddr);



    }

    public MemoryAddress csTranslate(long physical_address)
    {

        long ADDR = physical_address >> Param.tx_bits;

        MemoryAddress memAddr = new MemoryAddress();
        memAddr.BlockAddress = physical_address >> Param.tx_bits; 

        ReadOnlySpan<char> mapping = cmapping.AsSpan();

        for (int m = mapping.Length - 1; m >= 0; m--)
        {
            switch (mapping[m])
            {
                case 'r':
                    memAddr.Row = (int)ADDR & ((1 << Param.rowBits) - 1);
                    ADDR >>= Param.rowBits;
                    break;
                case 'R':
                    memAddr.Rank = (int)ADDR & ((1 << Param.rankBits) - 1);
                    ADDR >>= Param.rankBits;
                    break;
                case 'G':
                    memAddr.BankGroup = (int)ADDR & ((1 << Param.bankGBits) - 1);
                    ADDR >>= Param.bankGBits;
                    break;
                case 'B':
                    memAddr.Bank = (int)ADDR & ((1 << Param.bankBits) - 1);
                    ADDR >>= Param.bankBits;
                    break;
                case 'C':
                    memAddr.Channel = (int)ADDR & ((1 << Param.channelBits) - 1);
                    ADDR >>= Param.channelBits;
                    break;
                case 'c':
                    memAddr.Column = (int)ADDR & ((1 << Param.colBits) - 1);
                    ADDR >>= Param.colBits;
                    break;

                default:
                    break;
            }
        }


        return (memAddr);



    }
    public MemoryAddress Translate(long physical_address)
    {

        long ADDR = physical_address >> Param.tx_bits;

        MemoryAddress memAddr = new MemoryAddress();
        memAddr.BlockAddress = physical_address >> Param.tx_bits; 


        for (int m = cmapping.Length - 1; m >= 0; m--)
        {
            switch (cmapping[m])
            {
                case 'r':
                    memAddr.Row = (int)ADDR & ((1 << Param.rowBits) - 1);
                    ADDR >>= Param.rowBits;
                    break;
                case 'R':
                    memAddr.Rank = (int)ADDR & ((1 << Param.rankBits) - 1);
                    ADDR >>= Param.rankBits;
                    break;
                case 'G':
                    memAddr.BankGroup = (int)ADDR & ((1 << Param.bankGBits) - 1);
                    ADDR >>= Param.bankGBits;
                    break;
                case 'B':
                    memAddr.Bank = (int)ADDR & ((1 << Param.bankBits) - 1);
                    ADDR >>= Param.bankBits;
                    break;
                case 'C':
                    memAddr.Channel = (int)ADDR & ((1 << Param.channelBits) - 1);
                    ADDR >>= Param.channelBits;
                    break;
                case 'c':
                    memAddr.Column = (int)ADDR & ((1 << Param.colBits) - 1);
                    ADDR >>= Param.colBits;
                    break;

                default:
                    break;
            }
        }


        return (memAddr);

    }


}

