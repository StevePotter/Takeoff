using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Takeoff.Models
{
    /// <summary>
    /// Keeps track of set bits in an integer.
    /// </summary>
    public class SimpleBitVector32
    {
        const int EmptyData = 0;
        private int m_data;

        public static SimpleBitVector32 Create()
        {
            SimpleBitVector32 vector = new SimpleBitVector32();
            vector.m_data = EmptyData;
            return vector;
        }

        public bool this[int bit]
        {
            get
            {
                return IsSet(bit);
            }
            set
            {
                if (value)
                {
                    Set(bit);
                }
                else
                {
                    Clear(bit);
                }
            }
        }

        public bool IsSet(int bit)
        {
            return (m_data & bit) != EmptyData;

        }

        public void Set(int bit)
        {
            this.m_data |= bit;
        }

        public void Clear(int bit)
        {
            this.m_data &= ~bit;
        }

        public void Reset()
        {
            m_data = EmptyData;
        }

        public bool IsEmpty
        {
            get
            {
                return m_data == EmptyData;
            }
        }


    }
}
