using System.Collections.Generic;
using SimulatorLib.Common;
using SimulatorLib.DDR;

namespace SimulatorLib
{
    internal static class Extensions
    {

        public static string Info<T>(this List<T> lst)
        {
            
            string text = "[";

            if (lst.Count == 0)
                return "[]";

            if (lst.Count == 1)
                return lst[0].ToString();

            for (int i = 0; i < lst.Count - 1; i++)
                text += $"{lst[i]} , ";

            text += $"{lst[lst.Count - 1]}]";

            return text;

        }


        public static string Text<T>(this List<T> lst)
        {
            
            string text = "[";


            Request r;

            if (lst.Count == 1)
            {
                r = lst[0] as Request;
                return r.BlockAddress.ToString();
            }
            for (int i = 0; i < lst.Count - 1; i++)
            {
                r = lst[i] as Request;
                text += $"{r} , ";
            }

            r = lst[lst.Count - 1] as Request;
            text += $"{r} , ";

            return text;

        }

        public static string NextAct<T>(this List<T> list)
        {

            string s = "[";
            for (int i = 0; i < list.Count - 1; i++)
            {
                Bank bank = list[i] as Bank;

                s += bank.NextACT + ", ";
            }

            s += list[list.Count - 1];

            return s;
        }

    }


}
