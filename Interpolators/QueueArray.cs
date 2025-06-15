using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpolators
{
    /// <summary>
    /// Implementation fo a shift register
    /// of objects using an array as the 
    /// backing store.
    /// </summary>
    /// <typeparam name="T">The type of 
    /// objects held in the shift register
    /// </typeparam>
    
    public class QueueArray<T>
    {
        private T[] values;
        private int head;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="size">The length of
        /// the shift register</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if a meaningless array size is
        /// specified</exception>
        
        public QueueArray(int size)
        {
            if (size <= 0)
                throw new ArgumentOutOfRangeException
                    ("Size must be greater than zero");
            values = new T[size];
            head = 0;
            for(int i = 0; i < values.Length; i++)
                values[i] = default!; // Initialize to default value for T
        }

        private int TrueIndex(int index)
        {
            return (head + index) % values.Length;
        }   

        private void ValidateIndex(int index)
        {
            if (index < 0 || index >= values.Length)
                throw new IndexOutOfRangeException("Index out of range");
        }

        /// <summary>
        /// Index into the array of elements,
        /// starting with the oldest T at index 0,
        /// with the most recently added T at the
        /// index Length - 1.
        /// </summary>
        /// <param name="index">The index into the
        /// shift register</param>
        /// <returns>The element at the specified
        /// index.</returns>
        
        public T this[int index]
        {
            get
            {
                ValidateIndex(index);
                return values[TrueIndex(index)];
            }
            set
            {
                ValidateIndex(index);
                values[TrueIndex(index)] = value;
            }
        }

        /// <summary>
        /// Overwrite the oldest element in the
        /// shift register. The zero-value
        /// index now points at the remaining oldest
        /// element.
        /// </summary>
        /// <param name="t">The new value to be
        /// inserted</param>
        
        public void Insert(T t)
        {
            values[head] = t;
            head = (head + 1) % values.Length;
        }
    }
}
