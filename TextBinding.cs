using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2Pchatt
{
    //This is a simple textbinding class. 
    class TextBinding
    {
        private string NameValue;

        public string Name
        {
            get { return NameValue; }
            set { NameValue = value; }
        }
        private string TextValue;

        public string Text
        {
            get { return TextValue; }
            set { TextValue = value; }
        }

    }
}
