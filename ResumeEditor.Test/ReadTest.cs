using System;
using System.IO;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ResumeEditor.BEncoding;
using ResumeEditor.ResumeData;
using ResumeEditor.Torrents;

namespace ResumeEditor.Test
{
    [TestClass]
    public class ReadTest
    {
        [TestMethod]
        public void TestReadResumeData()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resumes", "resume3.dat");
            var resumeInformation = (BEncodedDictionary)BEncodedDictionary.DecodeResumeData(File.OpenRead(path));
            foreach (var pair in resumeInformation)
            {
                Console.WriteLine(pair.Key.Text);
            }
        }

        [TestMethod]
        public void TestLoadReume()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resumes", "resume3.dat");
            var resume = Resume.Load(path);
            foreach (var item in resume.ResumeItems)
            {
                foreach (var property in typeof (ResumeItem).GetProperties())
                {
                    string name = property.Name;
                    if (!property.PropertyType.IsGenericType)
                    {
                        object value = property.GetValue(item);
                        Console.WriteLine("{0, -15}: {1}", name, value);
                    }
                }
                break;
            }
        }
    }
}
