using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.Win32.SafeHandles;

namespace MSR.CVE.BackMaker
{
    public class NamedPipeBase
    {
        public delegate bool ServerHandler(object request, ref ISerializable reply);

        protected const int PIPE_ACCESS_DUPLEX = 3;
        protected const int PIPE_ACCESS_INBOUND = 1;
        protected const int PIPE_ACCESS_OUTBOUND = 2;
        protected const uint PIPE_TYPE_MESSAGE = 4u;
        protected const uint PIPE_READMODE_MESSAGE = 2u;
        protected const uint PIPE_WAIT = 0u;
        protected const uint PIPE_UNLIMITED_INSTANCES = 255u;
        protected const uint BUFFER_SIZE = 4096u;
        protected const uint NMPWAIT_USE_DEFAULT_WAIT = 0u;
        protected const uint NMPWAIT_WAIT_FOREVER = 4294967295u;
        protected const uint GENERIC_READ = 2147483648u;
        protected const uint GENERIC_WRITE = 1073741824u;
        protected const uint OPEN_EXISTING = 3u;
        protected const uint Error_PipeEnded = 109u;
        protected const string PipePrefix = "\\\\.\\pipe\\";
        protected const int ClientWaitSeconds = 10;
        protected const int LengthBufferLength = 147;
        protected BinaryFormatter binaryFormatter = new BinaryFormatter();
        protected SafeFileHandle pipeHandle;

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern SafeFileHandle CreateNamedPipe(string lpName, uint dwOpenMode, uint dwPipeMode,
            uint nMaxInstances, uint nOutBufferSize, uint nInBufferSize, uint nDefaultTimeOut,
            IntPtr lpSecurityAttributes);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern SafeFileHandle CreateFile(string lpFileName, uint dwDesiredAccess, uint dwShareMode,
            IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern int ConnectNamedPipe(SafeFileHandle hNamedPipe, IntPtr lpOverlapped);

        [DllImport("kernel32.dll")]
        internal static extern int GetLastError();

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadFile(IntPtr hHandle, byte[] lpBuffer, uint nNumberOfBytesToRead,
            byte[] lpNumberOfBytesRead, uint lpOverlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteFile(IntPtr hHandle, byte[] lpBuffer, uint nNumberOfBytesToWrite,
            byte[] lpNumberOfBytesWritten, uint lpOverlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool FlushFileBuffers(IntPtr hHandle);

        protected byte[] Serialize(ISerializable obj)
        {
            MemoryStream memoryStream = new MemoryStream();
            binaryFormatter.Serialize(memoryStream, obj);
            byte[] array = new byte[memoryStream.Length];
            Array.Copy(memoryStream.GetBuffer(), array, memoryStream.Length);
            return array;
        }

        protected object Deserialize(byte[] buffer)
        {
            MemoryStream serializationStream = new MemoryStream(buffer);
            return binaryFormatter.Deserialize(serializationStream);
        }

        protected void SendMessage(ISerializable request)
        {
            byte[] array = Serialize(request);
            LengthRecord obj = new LengthRecord(array.Length);
            byte[] array2 = Serialize(obj);
            D.Assert(array2.Length == 147);
            WriteBuffer(array2, array2.Length);
            WriteBuffer(array, array.Length);
        }

        protected byte[] ReadBuffer(int len)
        {
            byte[] array = new byte[4];
            byte[] array2 = new byte[len];
            byte[] array3 = new byte[len];
            int i = 0;
            while (i < len)
            {
                if (!ReadFile(pipeHandle.DangerousGetHandle(), array2, (uint)(len - i), array, 0u))
                {
                    if (GetLastError() == 109L)
                    {
                        throw new EndOfStreamException();
                    }

                    throw new IOException(string.Format("ReadBuffer sees error {0}", GetLastError()));
                }
                else
                {
                    int num = BitConverter.ToInt32(array, 0);
                    Array.Copy(array2, 0, array3, i, num);
                    i += num;
                }
            }

            return array3;
        }

        protected void WriteBuffer(byte[] buffer, int len)
        {
            byte[] array = new byte[4];
            for (int i = 0; i < len; i = 0)
            {
                if (!WriteFile(pipeHandle.DangerousGetHandle(), buffer, (uint)(len - i), array, 0u))
                {
                    throw new IOException(string.Format("ReadBuffer sees error {0}", GetLastError()));
                }

                i += BitConverter.ToInt32(array, 0);
                Array.Copy(buffer, i, buffer, 0, len - i);
                len -= i;
            }

            FlushFileBuffers(pipeHandle.DangerousGetHandle());
        }

        protected object ReadMessage()
        {
            byte[] buffer = ReadBuffer(147);
            LengthRecord lengthRecord = (LengthRecord)Deserialize(buffer);
            byte[] buffer2 = ReadBuffer(lengthRecord.length);
            return Deserialize(buffer2);
        }

        public object RPC(ISerializable request)
        {
            SendMessage(request);
            return ReadMessage();
        }

        public void RunServer(ServerHandler serverHandler)
        {
            bool flag = true;
            while (flag)
            {
                object request;
                try
                {
                    request = ReadMessage();
                }
                catch (EndOfStreamException)
                {
                    break;
                }

                ISerializable request2 = null;
                flag = serverHandler(request, ref request2);
                SendMessage(request2);
            }
        }
    }
}
