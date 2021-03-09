using System;
using Xunit;
using practiceBlockchain;


namespace BlockChain_Test
{
    //유닛은 가장 작은 단위를 테스트해야한다
    //꼭 블록 하나를 만드는 게 유닛의 단위가 아닌
    //nonce를 잘 만든다
    //difficulty를 잘 갱신한다 이런 게 유닛의 단위

    public class PracticeBlockChainTest
    {
        private Block MineGenesis()
        {
            var timestamp = new DateTime(2021, 03, 04);
            var hashValue = new byte[]
            {
                0xd6, 0x93, 0xda, 0x38, 0x66, 0xa3, 0x4d, 0x65, 0x9e, 0x01, 0x4f, 0x97,
                0xc8, 0xfe, 0xb0, 0x8a, 0xfe, 0x2e, 0x97, 0xc9, 0x9e, 0x3f, 0x33, 0x89,
                0xda, 0x02, 0x5f, 0xd0, 0x66, 0x5c, 0x62, 0x1c
            };

            return BlockChain.MakeBlock(
                previousHash: null,
                hashValue: hashValue,
                timeStamp: timestamp,
                nonce: new Nonce()
            );
        }

        private Block MineNext(Block previousBlock)
        {
            var previousHash = BlockChain.GetHash(previousBlock);

            return BlockChain.MakeBlock(
                previousHash: previousHash,
                hashValue: Block.CalculateHash(previousHash),
                timeStamp: previousBlock.TimeStamp.AddDays(1),
                nonce: new Nonce()
            );
        }

        private long UpdateDifficulty(Block previousBlock, Block currentBlock)
        {
            var updatedDifficulty = 
                BlockChain.UpdateDifficulty(previousBlock.TimeStamp, currentBlock.TimeStamp);

            return updatedDifficulty;
        }

        [Fact]
        public void CanMine()
        {
            Block genesis = MineGenesis();

            Assert.Null(genesis.PreviousHash);
            Assert.Equal(new DateTime(2021, 03, 04), genesis.TimeStamp);
            Assert.NotNull(genesis.Nonce.NonceValue);

            Block next = MineNext(genesis);
            Assert.Equal(BlockChain.GetHash(genesis), next.PreviousHash);
            Assert.Equal(new DateTime(2021, 03, 05), next.TimeStamp);

            var previous_difficulty = BlockChain.Difficulty;
            //스페이스바 닷넷에서 대문자로 구별
            //프로젝트 시작명은 대문자로
            //bin, obj 폴더는
            var next_difficulty = UpdateDifficulty(genesis, next);
            Assert.True(previous_difficulty < next_difficulty);
            //assert는 "항상" 통과하는 케이스 를 의미함
            //현재 테스트케이스는 assert의 정의 상 틀린 것..
            //difficulty는 유동적인 개념이기 때문에 늘 next_difficulty가 더 높다는 보장이 없다
        }
    }
}
