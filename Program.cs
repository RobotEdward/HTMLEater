using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace HTMLEater
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(StripTags(args[0],256));
        }

        static string StripTags(string InputFileName, int TargetLength)
        {
            using (StreamReader sr = new StreamReader(InputFileName))
            {
                //Find the story div (assume it's on a single line)
                bool FoundDiv = false;
                string s = "";
                while (!FoundDiv && !sr.EndOfStream)
                {
                    s = sr.ReadLine();
                    FoundDiv = s.Contains("<div id=\"story\">");
                }
                if (!FoundDiv)
                {
                    sr.Close();
                    return "";
                }
                //Okay, we're probably in the right place.
                bool CapturingText = false; // Use this to indicate when we've met a paragraph.
                bool InTag = false; // we're currently inside a Tag
                bool InEntity = false;
                bool FirstCharOfTagIsNext = false; //we're about to read the first character of a tag
                bool PIfNextIsSpace = false; //This will be a P if the next character is a space or closing tag
                bool LastCharWasSpace = true;
                char[] outArray = new char[TargetLength];
                int outIndex = 0;
                int i;
                char c;
                #region loop through the file stripping tags until we've got long enough text
                while ((outIndex < TargetLength) && !sr.EndOfStream)
                {
                    s = sr.ReadLine();
                    i = 0;
                    while ((i < s.Length) && (outIndex < TargetLength))
                    {
                        c = s[i];
                        if (CapturingText)
                        {
                        #region We're grabbing text now
                            switch (c)
                            {
                                case '<':
                                    InTag = true;
                                    break;
                                case '>':
                                    InTag = false;
                                    /* On the whole it looks better without a space after tags
                                    if (!LastCharWasSpace)
                                    {
                                        outArray[outIndex] = ' ';
                                        outIndex++;
                                        LastCharWasSpace = true;
                                    }
                                     */
                                    break;
                                case '&':
                                    InEntity = true;
                                    break;
                                case ';':
                                    InEntity = false;
                                    break;
                                case ' ':
                                case '\r':
                                case '\n':
                                    if (!InTag && !InEntity && !LastCharWasSpace)
                                    {
                                        outArray[outIndex] = ' ';
                                        outIndex++;
                                        LastCharWasSpace = true;
                                    }
                                    break;
                                default:
                                    if (!InTag && !InEntity)
                                    {
                                        outArray[outIndex] = c;
                                        outIndex++;
                                        LastCharWasSpace = false;
                                    }
                                    break;
                            }
                        #endregion
                        }
                        else
                        {
                        #region We're looking for the first P now.
                            switch (c)
                            {
                                case '<':
                                    InTag = true;
                                    FirstCharOfTagIsNext = true;
                                    break;
                                case '>':
                                    FirstCharOfTagIsNext = false;
                                    if(PIfNextIsSpace) {
                                        CapturingText = true;
                                        PIfNextIsSpace = false;
                                    }
                                    InTag = false;
                                    break;
                                case 'p':
                                case 'P':
                                    if(FirstCharOfTagIsNext) {
                                        PIfNextIsSpace = FirstCharOfTagIsNext;
                                        FirstCharOfTagIsNext = false;
                                    }
                                    break;
                                case ' ':
                                    if (PIfNextIsSpace)
                                    {
                                        CapturingText = PIfNextIsSpace;
                                        PIfNextIsSpace = false;
                                    }
                                    break;
                                default:
                                    if (PIfNextIsSpace) PIfNextIsSpace = false;
                                    break;
                            }
                        #endregion
                        }
                        i++;
                    }
                    //Stick a space in at the end of the line if there isn't one already
                    if(CapturingText && !LastCharWasSpace && (outIndex < TargetLength))
                    {
                        outArray[outIndex] = ' ';
                        outIndex++;
                        LastCharWasSpace = true;
                    }
                    //just in case we got a <P at the end of the line
                    if(!CapturingText) CapturingText = PIfNextIsSpace;
                }
                #endregion
                sr.Close();
                return new string(outArray, 0, outIndex);
            }
        }


    }
}
