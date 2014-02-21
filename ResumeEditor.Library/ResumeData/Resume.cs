using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ResumeEditor.BEncoding;

namespace ResumeEditor.ResumeData
{
    public class Resume
    {
        private BEncodedDictionary _originalDictionary;
        private string _resumePath;
        private List<ResumeItem> _resumeItems;
        private byte[] _fileGuard;

        #region LoadFunctions

        public static Resume Load(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException("path");
            }
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("Resume data file not found", path);
            }

            using (var file = File.OpenRead(path))
            {
                Resume resume = Resume.LoadCore(BEncodedDictionary.DecodeResumeData(file));
                return resume;
            }
        }

        internal static Resume LoadCore(BEncodedDictionary resumeData)
        {
            if (resumeData == null)
            {
                throw new ArgumentNullException("resumeData");
            }
            var resume = new Resume();
            resume.LoadInternal(resumeData);
            return resume;
        }

        protected void LoadInternal(BEncodedDictionary resumeData)
        {
            if (resumeData == null)
            {
                throw new ArgumentNullException("resumeData");
            }

            this._originalDictionary = resumeData;
            this._resumePath = "";
            this._resumeItems = new List<ResumeItem>(resumeData.Count);
            try
            {
                foreach (var keypair in resumeData)
                {
                    if (keypair.Key.Text == ".fileguard")
                    {
                        this._fileGuard = ((BEncodedString)keypair.Value).TextBytes;
                        continue;
                    }
                    var item = ResumeItem.Load((BEncodedDictionary)keypair.Value);
                    item.TorrentName = keypair.Key.Text;
                    this._resumeItems.Add(item);
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

        public string ResumePath
        {
            get { return _resumePath; }
            set { _resumePath = value; }
        }

        public IList<ResumeItem> ResumeItems
        {
            get { return _resumeItems.ToList(); }
            set { _resumeItems = value.ToList(); }
        }
    }
}
