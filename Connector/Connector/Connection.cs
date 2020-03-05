﻿using Flattiverse.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Flattiverse
{
    class Connection
    {
        private const ushort PROTOCOL_VERSION = 8;

        private Socket socket;
        private SocketAsyncEventArgs eventArgs;

        private byte[] recvBuffer = new byte[262144];
        private int recvBufferPosition;

        private byte[] recvPlain = new byte[262144];
        private int recvPlainPosition;

        private byte[] sendBuffer = new byte[262144];
        private int sendBufferPosition;

        private ICryptoTransform sendAES;
        private ICryptoTransform recvAES;

        public delegate void DisconnectedHandler();
        public event DisconnectedHandler Disconnected;

        public delegate void ReceivedHandler(List<Packet> packets);
        public event ReceivedHandler Received;

        private object sync = new object();
        private bool closed;

        private IndexList<Session> sessions = new IndexList<Session>(256);

        public async Task Connect(string user, string password)
        {
            byte[] packetData = new byte[64];

            // LoginPacket Connector -> Server:
            // 0-15:  IV für die Verschlüsselung von Connector zu Server.
            // 16-47: Userhash XOR mit IV.

            // AuthChallenge Server -> Connector:
            // 0-15:  IV für die Verschlüsselung von Server zu Connector.
            // 16-47: Verschlüsselte Zufällige 32 Bytes.

            // AuthChallenge Response Connector -> Server:
            // 0-15: Verschlüsselte XOR der empfangenen Bytes 16-31 ^ 32-47.

            // Disconnect oder Confirmation-Packet.
            // 0-13:  14 Zufällige Bytes, nicht verschlüsselt.
            // 14-15: 2 Bytes Protokoll-Version.

            byte[] iv = new byte[16];

            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(iv);

                Buffer.BlockCopy(iv, 0, packetData, 0, 16);

                byte[] userHash = new SHA256Managed().ComputeHash(Encoding.UTF8.GetBytes(user.ToLower()));
                byte[] passwordHash = null;

                unsafe
                {
                    fixed (byte* bInitialPacket = packetData)
                    fixed (byte* bUserHash = userHash)
                    fixed (byte* bIV = iv)
                    {
                        *(ulong*)(bInitialPacket + 16) = *(ulong*)bUserHash ^ *(ulong*)bIV;
                        *(ulong*)(bInitialPacket + 24) = *(ulong*)(bUserHash + 8) ^ *(ulong*)(bIV + 8);
                        *(ulong*)(bInitialPacket + 32) = *(ulong*)(bUserHash + 16) ^ *(ulong*)bIV;
                        *(ulong*)(bInitialPacket + 40) = *(ulong*)(bUserHash + 24) ^ *(ulong*)(bIV + 8);
                    }
                }

                await Task.Run(() => passwordHash = Crypto.HashPassword(user, password)).ConfigureAwait(false);

                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                socket.NoDelay = true;
                socket.ReceiveTimeout = 60000;
                socket.Blocking = false;

                await Task.Factory.FromAsync(socket.BeginConnect, socket.EndConnect, "galaxy.flattiverse.com", 22, null).ConfigureAwait(false);
                // await Task.Factory.FromAsync(socket.BeginConnect, socket.EndConnect, "127.0.0.1", 22, null).ConfigureAwait(false);

                int amount = await Task.Factory.FromAsync(socket.BeginSend(packetData, 0, 48, SocketFlags.None, null, null), socket.EndSend).ConfigureAwait(false);

                if (amount != 48)
                {
                    socket.Close();

                    throw new System.IO.InvalidDataException("Couldn't sent auth.");
                }

                SocketError socketError;

                amount = await Task.Factory.FromAsync(socket.BeginReceive(packetData, 0, 64, SocketFlags.None, out socketError, null, null), socket.EndReceive).ConfigureAwait(false);

                if (socketError != SocketError.Success && socketError != SocketError.IOPending || amount != 48)
                {
                    socket.Close();

                    throw new ArgumentException("Wrong username. Check your credentials.", "user");
                }

                byte[] serverIV = new byte[16];
                byte[] challenge = new byte[32];

                Buffer.BlockCopy(packetData, 0, serverIV, 0, 16);

                AesManaged aes = new AesManaged();

                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.None;

                sendAES = aes.CreateEncryptor(passwordHash, iv);
                recvAES = aes.CreateDecryptor(passwordHash, serverIV);

                if (recvAES.TransformBlock(packetData, 16, 32, challenge, 0) != 32)
                {
                    socket.Close();

                    throw new InvalidOperationException("The framework failed in decrypting data.");
                }

                unsafe
                {
                    fixed (byte* bChallenge = challenge)
                    {
                        *(ulong*)bChallenge = *(ulong*)bChallenge ^ *(ulong*)(bChallenge + 16);
                        *(ulong*)(bChallenge + 8) = *(ulong*)(bChallenge + 8) ^ *(ulong*)(bChallenge + 24);
                    }
                }

                if (sendAES.TransformBlock(challenge, 0, 16, packetData, 0) != 16)
                {
                    socket.Close();

                    throw new InvalidOperationException("The framework failed in decrypting data.");
                }

                amount = await Task.Factory.FromAsync(socket.BeginSend(packetData, 0, 16, SocketFlags.None, null, null), socket.EndSend).ConfigureAwait(false);

                if (amount != 16)
                {
                    socket.Close();

                    throw new System.IO.InvalidDataException("Couldn't sent auth challenge.");
                }

                amount = await Task.Factory.FromAsync(socket.BeginReceive(packetData, 0, 64, SocketFlags.None, out socketError, null, null), socket.EndReceive).ConfigureAwait(false);

                if (socketError != SocketError.Success && socketError != SocketError.IOPending || amount != 16)
                {
                    socket.Close();

                    throw new ArgumentException("Wrong password. Check your credentials.", "password");
                }

                // VERSION CHECK
                if (packetData[14] + packetData[15] * 256 != PROTOCOL_VERSION)
                {
                    socket.Close();

                    throw new InvalidOperationException($"Invalid protocol version. Server required protocol version {packetData[14] + packetData[15] * 256} while this connector speaks version {PROTOCOL_VERSION}. Please update your connector.");
                }

                eventArgs = new SocketAsyncEventArgs();

                eventArgs.Completed += received;
                eventArgs.SetBuffer(recvBuffer, 0, 262144);

                if (!socket.ReceiveAsync(eventArgs))
                    ThreadPool.QueueUserWorkItem(delegate { received(socket, eventArgs); });
            }
        }

        public void ProcessSessionPacket(Packet packet)
        {
            sessions[packet.Session]?.Answer(packet);
        }

        public async Task Connect(string user, byte[] passwordHash)
        {
            byte[] packetData = new byte[64];

            // LoginPacket Connector -> Server:
            // 0-15:  IV für die Verschlüsselung von Connector zu Server.
            // 16-47: Userhash XOR mit IV.

            // AuthChallenge Server -> Connector:
            // 0-15:  IV für die Verschlüsselung von Server zu Connector.
            // 16-47: Verschlüsselte Zufällige 32 Bytes.

            // AuthChallenge Response Connector -> Server:
            // 0-15: Verschlüsselte XOR der empfangenen Bytes 16-31 ^ 32-47.

            // Disconnect oder Confirmation-Packet.
            // 0-13:  14 Zufällige Bytes, nicht verschlüsselt.
            // 14-15: 2 Bytes Protokoll-Version.

            byte[] iv = new byte[16];

            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(iv);

                Buffer.BlockCopy(iv, 0, packetData, 0, 16);

                byte[] userHash = new SHA256Managed().ComputeHash(Encoding.UTF8.GetBytes(user.ToLower()));

                unsafe
                {
                    fixed (byte* bInitialPacket = packetData)
                    fixed (byte* bUserHash = userHash)
                    fixed (byte* bIV = iv)
                    {
                        *(ulong*)(bInitialPacket + 16) = *(ulong*)bUserHash ^ *(ulong*)bIV;
                        *(ulong*)(bInitialPacket + 24) = *(ulong*)(bUserHash + 8) ^ *(ulong*)(bIV + 8);
                        *(ulong*)(bInitialPacket + 32) = *(ulong*)(bUserHash + 16) ^ *(ulong*)bIV;
                        *(ulong*)(bInitialPacket + 40) = *(ulong*)(bUserHash + 24) ^ *(ulong*)(bIV + 8);
                    }
                }

                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                socket.NoDelay = true;
                socket.ReceiveTimeout = 60000;
                socket.Blocking = false;

                await Task.Factory.FromAsync(socket.BeginConnect, socket.EndConnect, "galaxy.flattiverse.com", 22, null).ConfigureAwait(false);
                // await Task.Factory.FromAsync(socket.BeginConnect, socket.EndConnect, "127.0.0.1", 22, null);

                int amount = await Task.Factory.FromAsync(socket.BeginSend(packetData, 0, 48, SocketFlags.None, null, null), socket.EndSend).ConfigureAwait(false);

                if (amount != 48)
                {
                    socket.Close();

                    throw new System.IO.InvalidDataException("Couldn't sent auth.");
                }

                SocketError socketError;

                amount = await Task.Factory.FromAsync(socket.BeginReceive(packetData, 0, 64, SocketFlags.None, out socketError, null, null), socket.EndReceive).ConfigureAwait(false);

                if (socketError != SocketError.Success && socketError != SocketError.IOPending || amount != 48)
                {
                    socket.Close();

                    throw new ArgumentException("Wrong username. Check your credentials.", "user");
                }

                byte[] serverIV = new byte[16];
                byte[] challenge = new byte[32];

                Buffer.BlockCopy(packetData, 0, serverIV, 0, 16);

                AesManaged aes = new AesManaged();

                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.None;

                sendAES = aes.CreateEncryptor(passwordHash, iv);
                recvAES = aes.CreateDecryptor(passwordHash, serverIV);

                if (recvAES.TransformBlock(packetData, 16, 32, challenge, 0) != 32)
                {
                    socket.Close();

                    throw new InvalidOperationException("The framework failed in decrypting data.");
                }

                unsafe
                {
                    fixed (byte* bChallenge = challenge)
                    {
                        *(ulong*)bChallenge = *(ulong*)bChallenge ^ *(ulong*)(bChallenge + 16);
                        *(ulong*)(bChallenge + 8) = *(ulong*)(bChallenge + 8) ^ *(ulong*)(bChallenge + 24);
                    }
                }

                if (sendAES.TransformBlock(challenge, 0, 16, packetData, 0) != 16)
                {
                    socket.Close();

                    throw new InvalidOperationException("The framework failed in decrypting data.");
                }

                amount = await Task.Factory.FromAsync(socket.BeginSend(packetData, 0, 16, SocketFlags.None, null, null), socket.EndSend).ConfigureAwait(false);

                if (amount != 16)
                {
                    socket.Close();

                    throw new System.IO.InvalidDataException("Couldn't sent auth challenge.");
                }

                amount = await Task.Factory.FromAsync(socket.BeginReceive(packetData, 0, 16, SocketFlags.None, out socketError, null, null), socket.EndReceive).ConfigureAwait(false);

                if (socketError != SocketError.Success && socketError != SocketError.IOPending || amount != 16)
                {
                    socket.Close();

                    throw new ArgumentException("Wrong password. Check your credentials.", "password");
                }

                // VERSION CHECK
                if (packetData[14] + packetData[15] * 256 != PROTOCOL_VERSION)
                {
                    socket.Close();

                    throw new InvalidOperationException($"Invalid protocol version. Server required protocol version {packetData[14] + packetData[15] * 256} while this connector speaks version {PROTOCOL_VERSION}. Please update your connector.");
                }

                eventArgs = new SocketAsyncEventArgs();

                eventArgs.Completed += received;
                eventArgs.SetBuffer(recvBuffer, 0, 262144);

                if (!socket.ReceiveAsync(eventArgs))
                    ThreadPool.QueueUserWorkItem(delegate { received(socket, eventArgs); });
            }
        }

        internal Session NewSession()
        {
            if (closed)
                throw new FlattiverseNotConnectedException();

            Session session = new Session(this);

            session.Setup((byte)sessions.Insert(session));

            return session;
        }

        internal void DeleteSession(Session session)
        {
            sessions.Wipe(session.ID);
        }

        private unsafe void received(object socketObject, SocketAsyncEventArgs eventArgs)
        {
            Socket socket = (Socket)socketObject;

            try
            {
                do
                {
                    if (eventArgs.SocketError != SocketError.Success || eventArgs.BytesTransferred == 0)
                        lock (sync)
                        {
                            closed = true;

                            try
                            {
                                Disconnected?.Invoke();
                            }
                            catch { }

                            eventArgs.Dispose();

                            socket.Close();

                            foreach (Session session in sessions)
                                session.Disconnected();

                            return;
                        }

                    recvBufferPosition += eventArgs.BytesTransferred;

                    if (recvBufferPosition > 15)
                    {
                        int useableData = recvBufferPosition - (recvBufferPosition % 16);

                        if (recvPlainPosition + useableData > 262144)
                            useableData = (262144 - recvPlainPosition) - ((262144 - recvPlainPosition) % 16);

                        recvAES.TransformBlock(recvBuffer, 0, useableData, recvPlain, recvPlainPosition);

                        recvPlainPosition += useableData;

                        if (useableData == recvBufferPosition)
                            recvBufferPosition = 0;
                        else
                        {
                            Buffer.BlockCopy(recvBuffer, useableData, recvBuffer, 0, recvBufferPosition - useableData);
                            recvBufferPosition -= useableData;
                        }

                        List<Packet> packets = new List<Packet>();

                        Packet packet = new Packet();

                        fixed (byte* bRecvPlain = recvPlain)
                        {
                            BinaryMemoryReader reader = new BinaryMemoryReader(bRecvPlain, recvPlainPosition);

                            while (packet.Parse(ref reader))
                            {
                                if (packet.OutOfBand == 0)
                                    packets.Add(packet);

                                packet = new Packet();
                            }

                            if (Received != null && packets.Count > 0)
                                Received(packets);

                            if (reader.Position - bRecvPlain == recvPlainPosition)
                                recvPlainPosition = 0;
                            else
                            {
                                Buffer.BlockCopy(recvPlain, (int)(reader.Position - bRecvPlain), recvPlain, 0, recvPlainPosition - (int)(reader.Position - bRecvPlain));
                                recvPlainPosition -= (int)(reader.Position - bRecvPlain);
                            }
                        }
                    }

                    eventArgs.SetBuffer(recvBufferPosition, 262144 - recvBufferPosition);
                }
                while (!socket.ReceiveAsync(eventArgs));
            }
            catch
            {
                lock (sync)
                    if (!closed)
                    {
                        closed = true;

                        try
                        {
                            Disconnected?.Invoke();
                        }
                        catch { }

                        eventArgs.Dispose();

                        socket.Close();

                        foreach (Session session in sessions)
                            session.Disconnected();
                    }
            }
        }

        public void Close()
        {
            lock (sync)
                if (!closed)
                    socket.Close();
        }

        public void Flush()
        {
            lock (sync)
            {
                if (closed || sendBufferPosition == 0)
                    return;

                SocketError socketError;

                if (sendBufferPosition % 16 == 0)
                {
                    try
                    {
                        if (socket.Send(sendBuffer, 0, sendBufferPosition, SocketFlags.None, out socketError) != sendBufferPosition || socketError != SocketError.Success)
                            socket.Close();
                    }
                    catch
                    {
                        socket.Close();
                    }

                    sendBufferPosition = 0;
                }
                else
                {
                    Packet p = new Packet() { OutOfBand = (byte)(16 - (sendBufferPosition % 16)) };

                    p.write(sendBuffer, ref sendBufferPosition);

                    sendAES.TransformBlock(sendBuffer, sendBufferPosition - 16, 16, sendBuffer, sendBufferPosition - 16);

                    try
                    {
                        if (socket.Send(sendBuffer, 0, sendBufferPosition, SocketFlags.None, out socketError) != sendBufferPosition || socketError != SocketError.Success)
                            socket.Close();
                    }
                    catch
                    {
                        socket.Close();
                    }

                    sendBufferPosition = 0;
                }
            }
        }

        public void Send(params Packet[] packets)
        {
            lock (sync)
            {
                if (closed)
                    return;

                foreach (Packet packet in packets)
                {
                    int currentBlock = sendBufferPosition - (sendBufferPosition % 16);

                    if (!packet.write(sendBuffer, ref sendBufferPosition))
                    {
                        Debug.Assert(currentBlock >= 16, "Too few data in buffer, but can't happen theoretically.");

                        SocketError socketError;

                        try
                        {
                            if (socket.Send(sendBuffer, 0, currentBlock, SocketFlags.None, out socketError) != currentBlock || socketError != SocketError.Success)
                            {
                                socket.Close();

                                return;
                            }
                        }
                        catch
                        {
                            socket.Close();

                            return;
                        }

                        if (sendBufferPosition == currentBlock)
                            sendBufferPosition = 0;
                        else
                        {
                            Buffer.BlockCopy(sendBuffer, currentBlock, sendBuffer, 0, sendBufferPosition - currentBlock);
                            sendBufferPosition -= currentBlock;
                        }

                        currentBlock = 0;

                        packet.write(sendBuffer, ref sendBufferPosition);
                    }

                    int newBlock = sendBufferPosition - (sendBufferPosition % 16);

                    if (currentBlock != newBlock)
                        sendAES.TransformBlock(sendBuffer, currentBlock, newBlock - currentBlock, sendBuffer, currentBlock);
                }
            }
        }
    }
}
