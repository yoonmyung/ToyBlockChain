using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Game;
using PracticeBlockChain.Crypto;

namespace PracticeBlockChain.Test
{
    public class TictactoeTest
    {
        private static Player PrepareForGame(string name)
        {
            PrivateKey privateKey = new PrivateKey();
            return new Player(
                name,
                privateKey,
                privateKey.PublicKey,
                new Address(privateKey.PublicKey)
            );
        }

        private static Action MakeActionTest(Player player, byte[] payload, long nonce)
        {
            Action action =
                new Action(
                    nonce++,
                    player.Address,
                    player.PublicKey,
                    player.PrivateKey,
                    payload
                );
            return action;
        }

        private static Nonce GetHashCashTest(
            Block previousBlock, 
            HashDigest previousHash, 
            BlockChain blockChain
        )
        {
            return HashCash.CalculateHash(
                previousBlock: previousBlock,
                previousHash: previousHash,
                blockChain: blockChain
            );
        }

        private static Action MakeActionTest(
            Player player, 
            Position position,
            long txNonce
        )
        {
            Position payload = player.Put(position);
            byte[] serializedPayload = TicTacToe.Serialize(payload);
            Action action = new Action(
                nonce: txNonce, 
                signer: player.Address,
                publicKey: player.PublicKey,
                privateKey: player.PrivateKey,
                payload: serializedPayload
            );
            return action;
        }

        private static byte[] SignActionTest(
            Player player,
            Action action
        )
        {
            byte[] signature = DefaultCryptoBackend.Sign(
                        messageHash: Action.Hash(action),
                        privateKey: player.PrivateKey
                    );
            Assert.True(DefaultCryptoBackend.Verify(
                Action.Hash(action), signature, player.PublicKey)
            );
            return signature;
        }

        private static Nonce FindNonceTest(
            Block previousBlock,
            HashDigest previousHash,
            BlockChain blockChain
        )
        {
            Nonce nonce =
                GetHashCashTest(
                    previousBlock: previousBlock,
                    previousHash: previousHash,
                    blockChain: blockChain
                );
            return nonce;
        }

        private static BlockChain AddBlocktoChainTest(
            BlockChain blockChain,
            long index,
            HashDigest previousHash,
            HashDigest hashValue,
            DateTimeOffset timeStamp,
            Nonce nonce,
            byte[] signature
        )
        {
            if (previousHash == null)
            {
                return new BlockChain(
                            index: index,
                            previousHash: null,
                            hashValue: Block.Hash(nonce),
                            timeStamp: DateTimeOffset.Now,
                            nonce: nonce,
                            signature: signature
                        );
            }
            else
            {
                blockChain.MakeBlock(
                    index: index,
                    previousHash: null,
                    hashValue: Block.Hash(nonce),
                    timeStamp: DateTimeOffset.Now,
                    nonce: nonce,
                    signature: signature
                );
                return blockChain;
            }
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
            // Put -> Make Action -> Sign Action -> Proof of work 
            // -> Update board(Update state) -> Print board(Print state)
            var _blockIndex = 0;
            var _player1TxNonce = 0;
            var _player2TxNonce = 0;
            var _ticTacToe = new TicTacToe();
            BlockChain _blockChain;

            Player _player1 = PrepareForGame("Kim");
            Player _player2 = PrepareForGame("Yoony");

            Action _action = 
                MakeActionTest(
                    _player1, 
                    new Position(1, 1),
                    _player1TxNonce++
                );

            byte[] _signature = SignActionTest(_player1, _action);

            Nonce _nonce = GetHashCashTest(null, null, null);

            _blockChain = AddBlocktoChainTest(
                blockChain: null,
                index: _blockIndex++,
                previousHash: null,
                hashValue: Block.Hash(_nonce),
                timeStamp: DateTimeOffset.Now,
                nonce: _nonce,
                signature: _signature
            );

            Position _payload = DeserializeActionTest(_action.Payload);

            _ticTacToe.Execute(
                (
                    player: _player1.Name,
                    position: _payload
                )
            );
        }
    }
}
