using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StockAnalysis.Model
{
    /// <summary>
    /// A Viewmodel is supposed to subscribe to events from instances of this class.
    /// It is supposed to encapsulate those events.
    /// </summary>
    public class MProgress
    {
        public string Name { get; set; }

        private MProgressEventArgs reuseEventArgs = new MProgressEventArgs();

        public event EventHandler<MProgressEventArgs> OnProgress
        {
            add => onProgress += value;
            remove => onProgress -= value;
        }
        private event EventHandler<MProgressEventArgs> onProgress;

        public event EventHandler<MProgressEventArgs> OnFinished
        {
            add => onFinished += value;
            remove => onFinished -= value;
        }
        private event EventHandler<MProgressEventArgs> onFinished;

        public event EventHandler<MProgressEventArgs> OnStarted 
        { 
            add => onStarted += value;
            remove => onStarted -= value; 
        }
        private event EventHandler<MProgressEventArgs> onStarted;

        public MProgress Start(int numItems)
        {
            reuseEventArgs = new MProgressEventArgs()
            {
                NumItems = numItems,
            };
            reuseEventArgs.IsBusy = true;

            onStarted?.Invoke(this, reuseEventArgs);
            return this;
        }

        public int Advance(int NumProgresses = 1)
        {
            reuseEventArgs.DoneItems += NumProgresses;
            reuseEventArgs.IsBusy = true;

            onProgress?.Invoke(this, reuseEventArgs);
            return reuseEventArgs.NumItems;
        }

        public int SetProgress(double NumProgresses)
        {
            reuseEventArgs.DoneItems = NumProgresses;
            reuseEventArgs.IsBusy = true;

            onProgress?.Invoke(this, reuseEventArgs);
            return reuseEventArgs.NumItems;
        }

        public int Done()
        {
            reuseEventArgs.DoneItems = reuseEventArgs.NumItems;
            reuseEventArgs.IsBusy = false;

            onProgress?.Invoke(this, reuseEventArgs);
            onFinished?.Invoke(this, reuseEventArgs);
            return reuseEventArgs.NumItems;
        }
    }
    public class MProgressEventArgs : EventArgs
    {
        public bool IsFinished
        {
            get
            {
                return isBusy == false;
            }
            set
            {
                isBusy = value == false;
            }
        }
        public bool IsBusy
        {
            get
            {
                return isBusy;
            }
            set
            {
                isBusy = value;
            }
        }
        private bool isBusy;
        public int NumItems
        {
            get
            {
                return numItems;
            }
            set
            {
                numItems = value;
            }
        }
        private int numItems;
        public double DoneItems
        {
            get
            {
                return doneItems == 0 ? 1 : doneItems;
            }
            set
            {
                doneItems = value;
            }
        }
        private double doneItems;
        public double Current
        {
            get => current ?? (double)DoneItems / (double)NumItems;
            set => current = value;
        }
        private double? current;
        public string CurrentString => $"{DoneItems}/{NumItems}";
    }
}
