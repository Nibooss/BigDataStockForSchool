﻿using System;
using System.Collections.Generic;
using System.IO;

namespace StockAnalysis.Model
{
    public class BinaryFileOperations
    {
        private static string folderPath => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + $"\\StockAnalysis";

        private static HashSet<FileInfo> InternalFileInfoHashSet = new HashSet<FileInfo>();

        public static IEnumerable<FileInfo> AllBinaryFiles()
        {
            foreach (var fi in new DirectoryInfo(folderPath).EnumerateFiles("*.sa", new EnumerationOptions() { IgnoreInaccessible = true } ))
            {
                if(false == InternalFileInfoHashSet.Contains(fi))
                {
                    InternalFileInfoHashSet.Add(fi);
                }
            }
            return InternalFileInfoHashSet;
        }

        public static void SaveToBinaryFile(IEnumerable<StockMoment> Input, string Symbol)
        {
            SaveToBinaryFile(Input, enshureFileExists(Symbol));
        }

        public static void SaveToBinaryFile(IEnumerable<StockMoment> Input, FileInfo fi)
        {
            // TODO: Make this work again
            /*
            var fs = fi.OpenWrite();
            fs.Position = fs.Length;
            foreach (var mom in Input)
            {
                if(mom == null)
                {
                    continue;
                }
                if(mom.Time == 0)
                {
                    continue;
                }
                fs.Write(BitConverter.GetBytes(mom.Time));
                fs.Write(BitConverter.GetBytes(mom.High));
                fs.Write(BitConverter.GetBytes(mom.Low));
                fs.Write(BitConverter.GetBytes(mom.Open));
                fs.Write(BitConverter.GetBytes(mom.Close));
                fs.Write(BitConverter.GetBytes(mom.Volume));
            }
            fs.Close();
            */
        }

        public static StockMoment[] FromFile(string Symbol)
        {
            return FromFile(enshureFileExists(Symbol));
        }

        public static StockMoment[] FromFile(FileInfo fi)
        {
            // Open the file
            var fs = fi.OpenRead();

            // Calculate how many moments are in the file
            var FileLenght = fi.Length / 40;
            var ReturnMe = new StockMoment[FileLenght];

            // Create local variables
            byte[] buffer = new byte[8];
            int SMParamsCounter = 0;
            int SMCounter = 0;

            // Create Buffer for curret Stock momment. This will be overwriten later.
            var CurrentSM = new StockMoment();

            // Loop over whole file until we dont read 8 bytes anymore.
            while (fs.Read(buffer, 0, 8) == 8)
            {
                // 5 Parameters in out class so:
                // - Save the current  object in array
                // - Override temp buffer with new object
                // - Reset counters
                if(SMParamsCounter >= 6)
                {
                    SMParamsCounter = 0;
                    ReturnMe[SMCounter++] = CurrentSM;
                    CurrentSM = new StockMoment();
                }
                DecoderFuncs[SMParamsCounter].Invoke(CurrentSM, buffer);
                SMParamsCounter++;
            }
            return ReturnMe;
        }

        private static Action<StockMoment, byte[]>[] DecoderFuncs = new Action<StockMoment, byte[]>[]
        {
            (i, d) => { i.Time = new DateTime(BitConverter.ToInt64(d)); },
            (i, d) => { i.High = BitConverter.ToDouble(d); },
            (i, d) => { i.Low = BitConverter.ToDouble(d); },
            (i, d) => { i.Open = BitConverter.ToDouble(d); },
            (i, d) => { i.Close = BitConverter.ToDouble(d); },
            (i, d) => { i.Volume = BitConverter.ToDouble(d); },
        };

        private static FileInfo enshureFileExists(string Symbol)
        {
            var file = new FileInfo(folderPath + $"\\{Symbol}_StockData.sa");
            if (false == file.Exists)
            {
                var di              = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
                di.CreateSubdirectory("StockAnalysis");
                var OpenFilestream  = file.Create();
                OnNewFileCreated?.Invoke(null, file);
                OpenFilestream.Close();
            }
            return file;
        }

        public static event EventHandler<FileInfo> OnNewFileCreated;
    }
}
