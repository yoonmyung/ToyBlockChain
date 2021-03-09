using System;
using System.Collections;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace practiceBlockchain
{
    //버전관리는 사람이 수정할 수 있는 파일만 대상으로 (bin, obj 폴더는 올리는 게 아니다)
    //모르면 gitignore.io를 활용하자 
    //gitignore를 C#으로 설정해두면 C# 개발 시 필요없는 파일들은 자동으로 ignore해줌
    //solution이었나 csproject였나 둘중 하나는 XML 파일이어서 사람이 얼마든지 수정가능해서 버전관리함

    //닷넷에서 스페이스바는 알파벳 대문자로 함 _이 아니고
    public class Block
    {
        readonly static byte[] previousHash;
        static readonly DateTimeOffset timeStamp;
        static Nonce nonce = new Nonce();
        //변수는 웬만하면 private와 readonly로 만들 것
        //전반적으로 쓰이는 변수나 함수만 public, static으로 선언

        public Block(byte[] previousHash, DateTimeOffset timeStamp, Nonce nonce) 
        {
            PreviousHash = previousHash;
            TimeStamp = timeStamp;
            Nonce = nonce;
        }

        public static byte[] CalculateHash(byte[] previousHash)
        {
            SHA256 hashAlgo = SHA256.Create();
            BigInteger hashValue;
            long difficulty = BlockChain.Difficulty;

            do
            {
                if (previousHash == null)   //genesis block
                {
                    hashValue = 0;
                    break;
                }
                nonce.updateNonce();
                //값 클래스, 값을 generate하는 클래스 따로 만들어서
                //generate하는 클래스는 값을 만드는 클래스형 객체를 리턴하도록 만들기!
                //=> nonce (nonce라는 이름은 값 클래스를 예상할 수 있게 만드는 이름이기 때문)
                byte[] hashInput = MakeHashInput(previousHash, nonce);
                //=> makehashinput 현재 가장 문제 많은 함수!
                //Hash(field값 + 이전해시값 + nonce) 모두 포함해서 해시값을 만드는 것
                hashValue = new BigInteger(hashAlgo.ComputeHash(hashInput));
            } while (hashValue < difficulty);

            return hashValue.ToByteArray();
        }

        //static 아닌 instance method로 선언
        //field + 이전 해시 + nonce 모두 serialize해서 해시함수의 input으로 넣어야 함
        //field를 serialize + 이전 해시 serialize + nonce 더해서 해시함수의 input으로 넣는 것인가
        private static byte[] MakeHashInput(byte[] previousHash, Nonce nonce)
        {
            return previousHash.Concat(nonce.NonceValue).ToArray();
        }

        public Nonce Nonce
        {
            get; set;
        }

        public byte[] PreviousHash
        {
            get; set;
        }

        public DateTimeOffset TimeStamp
        {
            get; set;
        }
    }
}
