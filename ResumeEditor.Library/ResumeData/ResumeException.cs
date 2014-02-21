using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace ResumeEditor.ResumeData
{
    [Serializable]
    public class ResumeException : Exception
    {
        public ResumeException()
        {
        }

        public ResumeException(string message)
            : base(message)
        {
        }

        public ResumeException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected ResumeException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }
    }
}
