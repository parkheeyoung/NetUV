﻿// Copyright (c) Johnny Z. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace NetUV.Core.Tests
{
    using System;
    using System.Runtime.InteropServices;
    using NetUV.Core.Handles;
    using NetUV.Core.Native;
    using Xunit;

    public sealed class PipeBindErrorTests : IDisposable
    {
        Loop loop;
        int closeCount;

        public PipeBindErrorTests()
        {
            this.loop = new Loop();
            this.closeCount = 0;
        }

        [Fact]
        public void AddressInUse()
        {
            Pipe pipe1 = this.loop.CreatePipe();
            string name = GetPipeName();
            pipe1.Bind(name);

            Pipe pipe2 = this.loop.CreatePipe();
            var error = Assert.Throws<OperationException>(() => pipe2.Bind(name));
            Assert.Equal((int)uv_err_code.UV_EADDRINUSE, error.ErrorCode);

            StreamListener<Pipe> listener = pipe1.Listen(OnConnection);

            error = Assert.Throws<OperationException>(() => pipe2.Listen(OnConnection));
            Assert.Equal((int)uv_err_code.UV_EINVAL, error.ErrorCode);

            pipe1.CloseHandle(this.OnClose);
            pipe2.CloseHandle(this.OnClose);

            this.loop.RunDefault();
            listener.Dispose();
            Assert.Equal(2, this.closeCount);
        }

        [Fact]
        public void AddressNotAvailable()
        {
            Pipe pipe = this.loop.CreatePipe();
            string name = GetBadPipeName();

            var error = Assert.Throws<OperationException>(() => pipe.Bind(name));
            Assert.Equal((int)uv_err_code.UV_EACCES, error.ErrorCode);

            pipe.CloseHandle(this.OnClose);
            this.loop.RunDefault();

            Assert.Equal(1, this.closeCount);
        }

        [Fact]
        public void Invalid()
        {
            Pipe pipe = this.loop.CreatePipe();
            string name = GetPipeName();
            pipe.Bind(name);

            var error = Assert.Throws<OperationException>(() => pipe.Bind($"{name}2"));
            Assert.Equal((int)uv_err_code.UV_EINVAL, error.ErrorCode);

            pipe.CloseHandle(this.OnClose);
            this.loop.RunDefault();

            Assert.Equal(1, this.closeCount);
        }

        [Fact]
        public void ListenWintoutBind()
        {
            Pipe pipe = this.loop.CreatePipe();

            StreamListener<Pipe> listener = null;
            var error = Assert.Throws<OperationException>(
                () => listener = pipe.Listen(OnConnection));

            Assert.Equal((int)uv_err_code.UV_EINVAL, error.ErrorCode);

            pipe.CloseHandle(this.OnClose);
            this.loop.RunDefault();
            listener?.Dispose();

            Assert.Equal(1, this.closeCount);
        }

        static void OnConnection(Pipe pipe, Exception exception)
        {
            //NOP
        }

        void OnClose(Pipe handle)
        {
            handle.Dispose();
            this.closeCount++;
        }

        static string GetPipeName() => 
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? "\\\\?\\pipe\\uv-test"
            : "/tmp/uv-test-sock";

        static string GetBadPipeName() => 
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? "bad-pipe"
            : "/path/to/unix/socket/that/really/should/not/be/there";

        public void Dispose()
        {
            this.loop?.Dispose();
            this.loop = null;
        }
    }
}
