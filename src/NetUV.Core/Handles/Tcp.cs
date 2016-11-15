﻿// Copyright (c) Johnny Z. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace NetUV.Core.Handles
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Net;
    using NetUV.Core.Channels;
    using NetUV.Core.Native;

    public sealed class Tcp : ServerStream
    {
        internal Tcp(LoopContext loop)
            : base(loop, uv_handle_type.UV_TCP)
        { }

        public IStream<Tcp> TcpStream()
        {
            this.Validate();
            return this.CreateStream<Tcp>();
        }

        public int GetSendBufferSize()
        {
            this.Validate();
            return this.SendBufferSize(0);
        }

        public int SetSendBufferSize(int value)
        {
            Contract.Requires(value > 0);

            this.Validate();
            return this.SendBufferSize(value);
        }

        public int GetReceiveBufferSize()
        {
            this.Validate();
            return this.ReceiveBufferSize(0);
        }

        public int SetReceiveBufferSize(int value)
        {
            Contract.Requires(value > 0);

            this.Validate();
            return this.ReceiveBufferSize(value);
        }

        public void Shutdown(Action<Tcp, Exception> completedAction = null) => 
            this.ShutdownStream((state, error) => completedAction?.Invoke((Tcp)state, error));

        public void QueueWrite(byte[] array, Action<Tcp, Exception> completedAction = null)
        {
            Contract.Requires(array != null);

            this.QueueWrite(array, 0, array.Length, completedAction);
        }

        public void QueueWrite(byte[] array, int offset, int count, Action<Tcp, Exception> completion = null)
        {
            Contract.Requires(array != null && array.Length > 0);
            Contract.Requires(offset >= 0 && count > 0);
            Contract.Requires((offset + count) <= array.Length);

            this.QueueWriteStream(array, offset, count, 
                (state, error) => completion?.Invoke((Tcp)state, error));
        }

        public Tcp RegisterRead(Action<Tcp, IStreamReadCompletion> readAction)
        {
            Contract.Requires(readAction != null);

            this.RegisterReadAction(
                (stream, completion) => readAction.Invoke((Tcp)stream, completion));

            return this;
        }

        internal Tcp Bind(IPEndPoint endPoint, bool dualStack = false)
        {
            Contract.Requires(endPoint != null);

            this.Validate();
            NativeMethods.TcpBind(this.InternalHandle, endPoint, dualStack);

            return this;
        }

        public IPEndPoint GetLocalEndPoint()
        {
            this.Validate();
            return NativeMethods.TcpGetSocketName(this.InternalHandle);
        }

        public IPEndPoint GetPeerEndPoint()
        {
            this.Validate();
            return NativeMethods.TcpGetPeerName(this.InternalHandle);
        }

        public Tcp NoDelay(bool value)
        {
            this.Validate();
            NativeMethods.TcpSetNoDelay(this.InternalHandle, value);

            return this;
        }

        public Tcp KeepAlive(bool value, int delay)
        {
            this.Validate();
            NativeMethods.TcpSetKeepAlive(this.InternalHandle, value, delay);

            return this;
        }

        public Tcp SimultaneousAccepts(bool value)
        {
            this.Validate();
            NativeMethods.TcpSimultaneousAccepts(this.InternalHandle, value);

            return this;
        }

        protected override unsafe StreamHandle NewStream()
        {
            IntPtr loopHandle = ((uv_stream_t*)this.InternalHandle)->loop;
            var loop = HandleContext.GetTarget<LoopContext>(loopHandle);
            return new Tcp(loop);
        }

        public void CloseHandle(Action<Tcp> callback = null) =>
            base.CloseHandle(state => callback?.Invoke((Tcp)state));
    }
}
