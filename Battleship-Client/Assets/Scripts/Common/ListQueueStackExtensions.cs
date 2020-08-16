using System;
using System.Collections.Generic;
using System.Linq;

namespace BattleshipGame.Common
{
    public static class ListQueueStackExtensions
    {
        public static Stack<T> CloneToStack<T>(this IEnumerable<T> original)
        {
            var array = original as T[] ?? original.ToArray();
            return new Stack<T>(array);
        }

        public static Stack<T> CloneToStack<T>(this Queue<T> original)
        {
            var array = new T[original.Count];
            original.CopyTo(array, 0);
            return new Stack<T>(array);
        }

        public static Queue<T> CloneToQueue<T>(this Stack<T> original)
        {
            var array = new T[original.Count];
            original.CopyTo(array, 0);
            Array.Reverse(array);
            return new Queue<T>(array);
        }
    }
}