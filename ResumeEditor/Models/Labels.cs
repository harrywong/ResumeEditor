using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace ResumeEditor.Models
{
    public class Labels : IEnumerable<string>, INotifyCollectionChanged
    {
        private int _sum;
        private readonly Dictionary<string, int> _labels;

        public Labels()
        {
            this._labels = new Dictionary<string, int>();
        }

        public void AddLabel(string label, int count)
        {
            this._labels.Add(label, count);
            if (CollectionChanged != null)
            {
                this.CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, string.Format("{0} ({1})", label, count)));
            }
        }

        public void AddOthers()
        {
            this.CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, string.Format("{0} ({1})", "Others", _sum - _labels.Sum(c => c.Value))));
        }

        public void Clear()
        {
            this._labels.Clear();
            if (CollectionChanged != null)
            {
                this.CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        public void SetSum(int sum)
        {
            this._sum = sum;
        }

        public IEnumerator<string> GetEnumerator()
        {
            foreach (var keypair in this._labels)
            {
                yield return string.Format("{0} ({1})", keypair.Key, keypair.Value);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        public event NotifyCollectionChangedEventHandler CollectionChanged;
    }
}