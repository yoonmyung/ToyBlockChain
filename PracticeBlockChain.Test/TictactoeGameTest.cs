using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using System.Numerics;
using PracticeBlockChain.TicTacToeGame;
using PracticeBlockChain.Cryptography;

namespace PracticeBlockChain.Test
{
    public class TictactoeGameTest
    {
        private static Action MakeActionTest(
            Player player,
            Position position,
            long txNonce
        )
        {
            // player랑 position을 담은 정보를 payload에 넣은 다음에
            // Action을 어차피 hashing하기 위해서 serialize를 하니까 
            // payload는 굳이 byte배열로 저장할 필요가 없더라
            (Player player, Position position) payload = (player, position);
            Action action = new Action(
                txNonce: txNonce,
                signer: player.Address,
                payload: payload
            );
            return action;
        }

        private static Nonce FindNonceTest(
            Block previousBlock,
            BigInteger previousHash,
            BlockChain blockChain
        )
        {
            Nonce nonce =
                HashCash.CalculateHash(
                    previousBlock: previousBlock,
                    previousHash: previousHash,
                    blockChain: blockChain
                );
            return nonce;
        }

        private static void AddBlocktoChainTest(
            BlockChain blockChain,
            Block block
        )
        {
            blockChain.MakeBlock(hashValue: block.Hash(), block: block);
        }

        private static Position DeserializeActionTest(byte[] serializedPayload)
        {
            Position deserializedPayload =
                (Position)Serialization.Deserialize(serializedPayload);
            return deserializedPayload;
        }

        [Fact]
        public static void Main()
        {
            var _blockIndex = 0;
            var _player1TxNonce = 0;
            var _player2TxNonce = 0;
            var _board = new Board();
            BlockChain _blockChain = new BlockChain();
            Nonce _nonce = null;

            // 0. Player 생성
            Player _player1 = new Player("Kim");
            Player _player2 = new Player("Yoony");

            // Make a genesis block.
            // Function here.

            // 1. Player1이 트랜젝션(여기선 액션)을 만들고 자신의 개인키로 서명한다.
            Action _action = 
                MakeActionTest(
                    _player1, 
                    new Position(1, 1),
                    _player1TxNonce++
                );
            byte[] _signature = _player1.Sign(_action.Hash());

            // 2. Player2는 Player1의 공개키로 이 트랜젝션을 검증한다.
            bool isValidAction = 
                _player2.Verify(_action.Hash(), _signature, _player1.PublicKey);

            // 3. Player2의 검증을 통과하면, Player1은 작업증명을 수행한다.
            if (isValidAction)
            {
                _nonce = FindNonceTest(null, 0, null);
            }
            Block _block = new Block(
                index: _blockIndex++,
                previousHash: 0,
                timeStamp: DateTimeOffset.Now,
                nonce: _nonce,
                signature: _signature,
                state: _board
            );

            // 4. 만족하는 논스를 찾은 논스와 당시 difficulty, 해시값이 유효한지 Validation을 거친다.
            // Validation

            // 5. Validation을 통과하면 블록체인에 블록을 생성한다 + 동시에 트랜젝션을 execute한다.
            AddBlocktoChainTest(
                _blockChain, _block
            );
        }
    }
}
