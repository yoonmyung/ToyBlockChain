using System;
using Xunit;
using practiceBlockchain;


namespace BlockChain_Test
{
    public class BlockChain_Test
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
            var nonce = 5;

            return BlockChain.MakeBlock(
                previousHash: null,
                hashValue: hashValue,
                timeStamp: timestamp,
                nonce: nonce
            );
        }

        private Block MineNext(Block previousBlock)
        {
            var previousHash = BlockChain.GetHash(previousBlock);

            return BlockChain.MakeBlock(
                previousHash: previousHash,
                hashValue: Block.CalculateHash(previousHash),
                timeStamp: previousBlock.TimeStamp.AddDays(1),
                nonce: 1
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
            Assert.Equal(5, genesis.Nonce);

            Block next = MineNext(genesis);
            Assert.Equal(BlockChain.GetHash(genesis), next.PreviousHash);
            Assert.Equal(new DateTime(2021, 03, 05), next.TimeStamp);

            var previous_difficulty = BlockChain.Difficulty;
            var next_difficulty = UpdateDifficulty(genesis, next);
            Assert.True(previous_difficulty < next_difficulty);
        }
    }
}
