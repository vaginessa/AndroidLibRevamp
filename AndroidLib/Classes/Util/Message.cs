/**
 * Message.cs: Created By Headygains - 07/17
 * **/

namespace Headygains.Android.Classes.Util
{
    /// <summary>
    /// An Output Message Container.
    /// </summary>
    internal class Message
    {
        public int Tag { get; set; }
        public string Source { get; set; }
        public string Content { get; set; }
        public string SerialNumber { get; set; }

        public Message(int tag, string content)
        {
            Tag = tag;
            Content = content;
        }

        public Message(int tag, string source, string content)
        {
            Tag = tag;
            Source = source;
            Content = content;
        }

        public Message(int tag, string source, string content, string deviceSerialNumber)
        {
            Tag = tag;
            Source = source;
            Content = content;
            SerialNumber = deviceSerialNumber;
        }

    }
}
