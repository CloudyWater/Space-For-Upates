using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;


namespace iBoxDB.LocalServer.IO
{
    public class MMapConfig : BoxFileStreamConfig
    {
        private static int MaxFile = 1024;
        private static long FileSize = 1024 * 1024 * 256;
        private static long MaxMemory = FileSize * 4;

        Dictionary<String, MManager> map = new Dictionary<String, MManager>();

        public MMapConfig()
        {
            this.ReadStreamCount = 1;
        }



        public override IBStream CreateStream(String path, StreamAccess access)
        {
            if (path.EndsWith(".swp") || path.EndsWith(".buf")
                    || path.EndsWith(".frag"))
            {
                return base.CreateStream(path, access);
            }
            path = BoxFileStreamConfig.RootPath + path;
            MManager manager;
            if (!map.TryGetValue(path, out manager))
            {
                manager = new MManager(path);
                map.Add(path, manager);
            }
            return manager.Get(access);
        }


        public override void Dispose()
        {
            foreach (MManager m in map.Values)
            {
                m.Dispose();
            }
            map.Clear();
            base.Dispose();
        }

        private class MManager
        {

            private String fullPath;
            private long length;

            private MemoryMappedFile[] files = new MemoryMappedFile[MaxFile];
            private MemoryMappedViewAccessor[] mapReaders = new MemoryMappedViewAccessor[MaxFile];
            private MemoryMappedViewAccessor[] mapWriters = new MemoryMappedViewAccessor[MaxFile];

            public MManager(String fPath)
            {
                fullPath = fPath;
                length = 0;
                if (File.Exists(fullPath))
                {
                    length += FileSize;
                }
            }

            public void Dispose()
            {
                Flush();
                if (files != null)
                {
                    foreach (var a in mapReaders) { if (a != null) { a.Dispose(); } }
                    foreach (var a in mapWriters) { if (a != null) { a.Dispose(); } }
                    foreach (var a in files) { if (a != null) { a.Dispose(); } }
                }
                files = null;
                mapWriters = null;
                mapReaders = null;
            }

            public void Flush()
            {
                lock (this)
                {
                    if (mapWriters != null)
                    {
                        foreach (MemoryMappedViewAccessor m in mapWriters)
                        {
                            if (m != null)
                            {
                                m.Flush();
                            }
                        }
                    }
                }
            }

            public IBStream Get(StreamAccess access)
            {
                return new MStream(this, access == StreamAccess.Read ? mapReaders
                        : mapWriters);
            }

            private class MStream : IPartitionStream
            {

                MemoryMappedViewAccessor[] bufs;
                MManager manager;
                public MStream(MManager manager, MemoryMappedViewAccessor[] mappedByteBuffers)
                {
                    this.bufs = mappedByteBuffers;
                    this.manager = manager;
                }


                public void Dispose()
                {
                    bufs = null;
                }


                public void Flush()
                {
                }


                public long Length
                {
                    get
                    {
                        return manager.length;
                    }
                }


                public void SetLength(long value)
                {
                }


                public int Read(long position, byte[] buffer, int offset, int count)
                {
                    lock (manager.mapReaders)
                    {
                        long filePos;
                        GetCurrent(position, out filePos).ReadArray(filePos, buffer, offset, count);
                        return count;
                    }
                }


                public void Write(long position, byte[] buffer, int offset,
                        int count)
                {
                    long filePos;
                    GetCurrent(position, out filePos).WriteArray(filePos, buffer, offset, count);
                }


                public bool Suitable(long position, int len)
                {
                    int fileOffset = (int)(position / FileSize);
                    int fileOffset1 = (int)((position + len) / FileSize);
                    return fileOffset == fileOffset1;
                }

                private MemoryMappedViewAccessor GetCurrent(long position, out long filePos)
                {
                    int fileOffset = (int)(position / FileSize);
                    MemoryMappedViewAccessor current = bufs[fileOffset];
                    if (current == null)
                    {
                        lock (manager.mapWriters)
                        {
                            current = bufs[fileOffset];
                            if (current == null)
                            {

                                String pfile = fileOffset == 0 ? manager.fullPath
                                        : manager.fullPath + fileOffset;
                                manager.files[fileOffset] = MemoryMappedFile.CreateFromFile(pfile,
                                    FileMode.OpenOrCreate, pfile, FileSize, MemoryMappedFileAccess.ReadWrite);
                                manager.length += FileSize;
                                manager.mapWriters[fileOffset] = manager.files[fileOffset].CreateViewAccessor();
                                manager.mapReaders[fileOffset] = manager.files[fileOffset].CreateViewAccessor();

                                if (position > MaxMemory)
                                {
                                    manager.Flush();
                                }
                            }
                        }
                        current = bufs[fileOffset];
                    }
                    filePos = position - (fileOffset * FileSize);
                    return current;
                }


                public void BeginWrite(long appID, int maxLen)
                {

                }

                public void EndWrite()
                {

                }
            }

        }


    }

}
