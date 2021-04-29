using StockAnalysis.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockAnalysis.ViewModel
{
    class MainWindowContext
    {
        public ObservableCollection<FileInfo> DownloadedFiles => downloadedFiles ??= initDownlaodedFiles();
        private ObservableCollection<FileInfo> downloadedFiles;
        private ObservableCollection<FileInfo> initDownlaodedFiles()
        {
            var oc = new ObservableCollection<FileInfo>();
            foreach(var fi in BinaryFileOperations.AllBinaryFiles())
            {
                oc.Add(fi);
            }
            BinaryFileOperations.OnNewFileCreated += (s, e) => oc.Add(e);
            return oc;
        }
    }
}
