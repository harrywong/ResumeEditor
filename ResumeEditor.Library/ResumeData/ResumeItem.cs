using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ResumeEditor.BEncoding;

namespace ResumeEditor.ResumeData
{
    public class ResumeItem
    {
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private DateTime _addOn = UnixEpoch;
        private int _blockSize;
        private string _caption;
        private DateTime _completedOn = UnixEpoch;
        private int _dht;
        private string _path;
        private int _trackerMode;
        private List<string> _trackers;
        private string _label;
        private List<string> _labels; 

        private string _torrentName;
        private BEncodedDictionary _origionalDictionary;
        #region LoadFunctions

        public static ResumeItem Load(BEncodedDictionary resumeData)
        {
            if (resumeData == null)
            {
                throw new ArgumentNullException("resumeData");
            }

            try
            {
                ResumeItem resumeItem =
                    ResumeItem.LoadCore(resumeData);
                return resumeItem;
            }
            catch (BEncodingException ex)
            {
                throw new ResumeException("Invalid torrent file specified", ex);
            }
        }

        internal static ResumeItem LoadCore(BEncodedDictionary resumeData)
        {
            if (resumeData == null)
            {
                throw new ArgumentNullException("resumeData");
            }
            var resume = new ResumeItem();
            resume.LoadInternal(resumeData);
            return resume;
        }

        protected void LoadInternal(BEncodedDictionary resumeData)
        {
            if (resumeData == null)
            {
                throw new ArgumentNullException("resumeData");
            }
            this._origionalDictionary = resumeData;
            try
            {
                foreach (var keypair in resumeData)
                {
                    switch (keypair.Key.Text)
                    {
                        case "added_on":
                            this._addOn = this._addOn.AddSeconds(((BEncodedNumber)keypair.Value).Number);
                            break;
                        case "blocksize":
                            this._blockSize = Convert.ToInt32(((BEncodedNumber)keypair.Value).Number);
                            break;
                        case "caption":
                            this._caption = ((BEncodedString)keypair.Value).Text;
                            break;
                        case "completed_on":
                            this._completedOn = this._completedOn.AddSeconds(((BEncodedNumber)keypair.Value).Number);
                            break;
                        case "dht":
                            this._dht = Convert.ToInt32(((BEncodedNumber)keypair.Value).Number);
                            break;
                        case "path":
                            this._path = ((BEncodedString)keypair.Value).Text;
                            break;
                        case "trackermode":
                            this._trackerMode = Convert.ToInt32(((BEncodedNumber)keypair.Value).Number);
                            break;
                        case "label":
                            this._label = ((BEncodedString)keypair.Value).Text;
                            break;
                        case "labels":
                            this._labels =
                                ((BEncodedList)keypair.Value).Cast<BEncodedString>().Select(c => c.Text).ToList();
                            break;
                        case "trackers":
                            this._trackers =
                                ((BEncodedList)keypair.Value).Cast<BEncodedString>().Select(c => c.Text).ToList();
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex is BEncodingException)
                {
                    throw;
                }
                throw new BEncodingException("");
            }
        }
        #endregion

        #region Properties

        public DateTime AddOn
        {
            get { return _addOn; }
            set { _addOn = value; }
        }

        public int BlockSize
        {
            get { return _blockSize; }
            set { _blockSize = value; }
        }

        public string Caption
        {
            get { return _caption; }
            set { _caption = value; }
        }

        public DateTime CompletedOn
        {
            get { return _completedOn; }
            set { _completedOn = value; }
        }

        public int Dht
        {
            get { return _dht; }
            set { _dht = value; }
        }

        public string Path
        {
            get { return _path; }
            set { _path = value; }
        }

        public int TrackerMode
        {
            get { return _trackerMode; }
            set { _trackerMode = value; }
        }

        public List<string> Trackers
        {
            get { return _trackers; }
            set { _trackers = value; }
        }

        public string Label
        {
            get { return _label; }
            set { _label = value; }
        }

        public List<string> Labels
        {
            get { return _labels; }
            set { _labels = value; }
        }

        public string TorrentName
        {
            get { return _torrentName; }
            set { _torrentName = value; }
        }

        #endregion
    }
}
