using System;
using System.Collections.Generic;
using System.Text;

namespace practiceBlockchain
{
    public class Nonce
    {
        private static byte[] nonceValue = null;

        public byte[] NonceValue
        {
            get
            {
                return nonceValue;
            }
            set
            {
                updateNonce();
            }
        }

        public void updateNonce()
        {
            //libplanet: Nonce에 매번 랜덤값을 할당하는 것으로 1씩 증가하는 것을 대신함
            nonceValue = new byte[10];
            Random random = new Random();
            random.NextBytes(nonceValue);
        }
    }
}
