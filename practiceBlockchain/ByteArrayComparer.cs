using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//byte[]끼리 비교해야 하는 경우 아예 자료형을 만들어서 비교
//libplanet/Libplanet.Stun/Stun/TurnClient.cs
namespace practiceBlockchain
{
    class ByteArrayComparer : IEqualityComparer<byte[]>
    {
        public bool Equals(byte[] x, byte[] y)
        {
            if (x == null || y == null)
            {
                return x == y;
            }

            return x.SequenceEqual(y);
        }

        public int GetHashCode(byte[] obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            return obj.Sum(b => b);
        }
    }
}
