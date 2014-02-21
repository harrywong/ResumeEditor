using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ResumeEditor.BEncoding;
using ResumeEditor.Torrents;

namespace ResumeEditor.Test
{
    [TestClass]
    public class ReadTest
    {
        [TestMethod]
        public void TestReadResumeData()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resumes", "resume1.dat");
            var resumeInformation = (BEncodedDictionary)BEncodedDictionary.DecodeResumeData(File.OpenRead(path));
            foreach (var pair in resumeInformation)
            {
                Console.WriteLine(pair.Key.Text);
            }
        }
    }
}
