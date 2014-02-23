using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ResumeEditor.Models
{
    public class UTorrent
    {
        private string _title;
        private string _resumePath;

        public string Title
        {
            get { return _title; }
            set { _title = value; }
        }

        public string ResumePath
        {
            get { return _resumePath; }
            set { _resumePath = value; }
        }
    }
}
